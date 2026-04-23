using System.Collections.Immutable;
using System.Text;
using Preflight.App.Models;
using Schneegans.Unattend;
// Our UnattendConfig and Schneegans.Unattend both declare several enums with the same
// short name - alias the library types so the adapter can reference both without ambiguity.
using SchneegansEdition = Schneegans.Unattend.WindowsEdition;
using SchneegansExpressSettings = Schneegans.Unattend.ExpressSettingsMode;
using SchneegansStickyKeys = Schneegans.Unattend.StickyKeys;

namespace Preflight.App.Services;

/// <summary>
/// Thin adapter over the vendored <see cref="Schneegans.Unattend.UnattendGenerator"/>.
/// Translates our mutable UI-facing <see cref="UnattendConfig"/> into Schneegans'
/// immutable <see cref="Configuration"/> record and serializes the result for the
/// Advanced preview panel / download button.
///
/// Registered as a singleton so the generator (which parses a dozen embedded JSON
/// resources on construction) is paid for exactly once per session.
/// </summary>
public sealed class UnattendXmlBuilder
{
    private readonly UnattendGenerator _generator = new();

    /// <summary>
    /// Build an <c>autounattend.xml</c> string from the current config.
    /// Returns an XML comment describing the failure if mapping or generation throws -
    /// the Advanced panel displays whatever we return, so a thrown exception would blank
    /// the preview instead of explaining the problem.
    /// </summary>
    public string Build(UnattendConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        try
        {
            var schneegans = MapToConfiguration(config);
            var doc = _generator.GenerateXml(schneegans);
            var bytes = UnattendGenerator.Serialize(doc);
            // Serialize emits ASCII-encoded bytes with a utf-8 declaration - decoding as
            // UTF-8 is correct (ASCII is a strict UTF-8 subset).
            return Encoding.UTF8.GetString(bytes);
        }
        catch (Exception ex)
        {
            return $"<!-- preflight.xml: failed to generate XML - {ex.GetType().Name}: {ex.Message} -->";
        }
    }

    // ─── Mapping ────────────────────────────────────────────────────

    private Configuration MapToConfiguration(UnattendConfig ui)
    {
        var baseConfig = Configuration.Default with
        {
            LanguageSettings = MapLanguage(ui.Region),
            AccountSettings = MapAccounts(ui.Users, ui.FirstLogon),
            EditionSettings = MapEdition(ui.Edition),
            Bloatwares = MapBloatware(ui.Bloatware),
            ExpressSettings = MapExpressSettings(ui.Privacy.ExpressSettings),
            LockKeySettings = MapLockKeys(ui.LockKeys),
            StickyKeysSettings = MapStickyKeys(ui.StickyKeys),
        };

        baseConfig = MapSystemTweaks(baseConfig, ui.Tweaks);
        baseConfig = MapVmSupport(baseConfig, ui.VmSupport);
        return baseConfig;
    }

    private ILanguageSettings MapLanguage(RegionSettings region)
    {
        // Any lookup failure → fall back to the interactive (Windows Setup asks) path so
        // the preview still renders. The user sees this as "language settings unspecified"
        // in the XML, which is the right signal that something in the UI needs attention.
        var image = TryLookup<ImageLanguage>(region.DisplayLanguage);
        var locale = TryLookup<UserLocale>(region.InputLanguage);
        var geo = TryLookup<GeoLocation>(region.HomeLocation);

        if (image is null || locale is null || geo is null)
            return new InteractiveLanguageSettings();

        // UserLocale.KeyboardLayout is the Microsoft-recommended default for that locale;
        // if the JSON entry omits it (rare) we fall back to the US English keyboard so the
        // required LocaleAndKeyboard record can still be constructed.
        var keyboard = locale.KeyboardLayout
            ?? TryLookup<KeyboardIdentifier>("00000409")
            ?? throw new InvalidOperationException("Default US keyboard identifier missing from embedded data.");

        return new UnattendedLanguageSettings(
            ImageLanguage: image,
            LocaleAndKeyboard: new LocaleAndKeyboard(locale, keyboard),
            LocaleAndKeyboard2: null,
            LocaleAndKeyboard3: null,
            GeoLocation: geo);
    }

    private static IAccountSettings MapAccounts(List<UserAccount> users, FirstLogonSettings firstLogon)
    {
        // Empty list / no admin → defer to Windows Setup so we don't throw from the
        // UnattendedAccountSettings constructor's "must have one admin" guard.
        var named = users.Where(u => !string.IsNullOrWhiteSpace(u.Name)).ToList();
        if (named.Count == 0 || !named.Any(u => u.Group == AccountGroup.Administrators))
            return new InteractiveLocalAccountSettings();

        var accounts = named
            .Select(u => new Account(
                name: u.Name.Trim(),
                displayName: u.DisplayName ?? u.Name.Trim(),
                password: u.Password ?? string.Empty,
                group: u.Group == AccountGroup.Administrators
                    ? Constants.AdministratorsGroup
                    : Constants.UsersGroup))
            .ToImmutableList();

        IAutoLogonSettings autoLogon = firstLogon.Mode switch
        {
            FirstLogonMode.FirstAdminAccount => new OwnAutoLogonSettings(),
            FirstLogonMode.BuiltInAdministrator => new BuiltinAutoLogonSettings(string.Empty),
            FirstLogonMode.DoNotLogon => new NoneAutoLogonSettings(),
            _ => new NoneAutoLogonSettings(),
        };

        return new UnattendedAccountSettings(accounts, autoLogon, firstLogon.ObscurePasswordsWithBase64);
    }

    private IEditionSettings MapEdition(WindowsEditionSettings edition) => edition.KeyMode switch
    {
        ProductKeyMode.Interactive => new InteractiveEditionSettings(),
        ProductKeyMode.FromBios => new FirmwareEditionSettings(),
        ProductKeyMode.Custom when !string.IsNullOrWhiteSpace(edition.ProductKey)
            => SafeCustom(edition.ProductKey!),
        // Generic (GVLK) and Custom-without-key both fall through to an edition lookup.
        _ => TryLookup<SchneegansEdition>(MapEditionId(edition.Edition)) is { } we
            ? new UnattendedEditionSettings(we)
            : new InteractiveEditionSettings(),
    };

    private static IEditionSettings SafeCustom(string productKey)
    {
        try { return new CustomEditionSettings(productKey); }
        catch (ConfigurationException) { return new InteractiveEditionSettings(); }
    }

    private static string MapEditionId(Preflight.App.Models.WindowsEdition edition) => edition switch
    {
        Preflight.App.Models.WindowsEdition.Home => "home",
        Preflight.App.Models.WindowsEdition.HomeN => "home_n",
        Preflight.App.Models.WindowsEdition.HomeSingleLanguage => "home_single",
        Preflight.App.Models.WindowsEdition.Pro => "pro",
        Preflight.App.Models.WindowsEdition.ProN => "pro_n",
        Preflight.App.Models.WindowsEdition.ProEducation => "pro_edu",
        Preflight.App.Models.WindowsEdition.ProForWorkstations => "pro_wks",
        Preflight.App.Models.WindowsEdition.Education => "education",
        Preflight.App.Models.WindowsEdition.EducationN => "education_n",
        Preflight.App.Models.WindowsEdition.Enterprise => "enterprise",
        Preflight.App.Models.WindowsEdition.EnterpriseN => "enterprise_n",
        _ => "pro",
    };

    private ImmutableList<Bloatware> MapBloatware(BloatwareSettings bloatware)
    {
        if (bloatware.AppsToRemove.Count == 0)
            return ImmutableList<Bloatware>.Empty;

        // Skip unknown ids so a stale preset (or a bloatware id that was later removed
        // upstream) doesn't blow up the whole preview.
        return bloatware.AppsToRemove
            .Select(id => TryLookup<Bloatware>(id))
            .Where(b => b is not null)
            .Select(b => b!)
            .ToImmutableList();
    }

    // ─── System tweaks ──────────────────────────────────────────────
    // Every named bool on Configuration gets passed through explicitly. If schneegans
    // renames or drops a property, this method fails to compile rather than silently
    // emitting a stale XML preview.
    private static Configuration MapSystemTweaks(Configuration baseConfig, SystemTweaks t) => baseConfig with
    {
        // Explorer & shell
        ShowFileExtensions = t.ShowFileExtensions,
        ShowAllTrayIcons = t.ShowAllTrayIcons,
        HideTaskViewButton = t.HideTaskViewButton,
        ClassicContextMenu = t.ClassicContextMenu,
        LeftTaskbar = t.LeftTaskbar,
        LaunchToThisPC = t.LaunchToThisPC,
        ShowEndTask = t.ShowEndTask,
        HideInfoTip = t.HideInfoTip,

        // System & performance
        EnableLongPaths = t.EnableLongPaths,
        HardenSystemDriveAcl = t.HardenSystemDriveAcl,
        DeleteJunctions = t.DeleteJunctions,
        AllowPowerShellScripts = t.AllowPowerShellScripts,
        DisableLastAccess = t.DisableLastAccess,
        PreventAutomaticReboot = t.PreventAutomaticReboot,
        DisableFastStartup = t.DisableFastStartup,
        DisableSystemRestore = t.DisableSystemRestore,
        TurnOffSystemSounds = t.TurnOffSystemSounds,
        DisableAppSuggestions = t.DisableAppSuggestions,
        DisableWidgets = t.DisableWidgets,
        DisableWindowsUpdate = t.DisableWindowsUpdate,
        DisablePointerPrecision = t.DisablePointerPrecision,
        DeleteWindowsOld = t.DeleteWindowsOld,
        DisableBingResults = t.DisableBingResults,
        PreventDeviceEncryption = t.PreventDeviceEncryption,
        DisableCoreIsolation = t.DisableCoreIsolation,
        DisableAutomaticRestartSignOn = t.DisableAutomaticRestartSignOn,
        DisableWpbt = t.DisableWpbt,

        // Edge
        HideEdgeFre = t.HideEdgeFre,
        DisableEdgeStartupBoost = t.DisableEdgeStartupBoost,
        MakeEdgeUninstallable = t.MakeEdgeUninstallable,
        DeleteEdgeDesktopIcon = t.DeleteEdgeDesktopIcon,
    };

    private static Configuration MapVmSupport(Configuration baseConfig, VmSupport v) => baseConfig with
    {
        VBoxGuestAdditions = v.VirtualBoxGuestAdditions,
        VMwareTools = v.VmwareTools,
        VirtIoGuestTools = v.VirtIoAndQemuAgent,
        ParallelsTools = v.ParallelsTools,
    };

    private static SchneegansExpressSettings MapExpressSettings(Preflight.App.Models.ExpressSettingsMode mode) => mode switch
    {
        Preflight.App.Models.ExpressSettingsMode.Interactive => SchneegansExpressSettings.Interactive,
        Preflight.App.Models.ExpressSettingsMode.EnableAll => SchneegansExpressSettings.EnableAll,
        Preflight.App.Models.ExpressSettingsMode.DisableAll => SchneegansExpressSettings.DisableAll,
        _ => SchneegansExpressSettings.Interactive,
    };

    private static ILockKeySettings MapLockKeys(LockKeysSettings k) => k.Mode switch
    {
        LockKeysMode.Default => new SkipLockKeySettings(),
        LockKeysMode.Configure => new ConfigureLockKeySettings(
            CapsLock: new LockKeySetting(
                Initial: k.CapsLockInitial == LockKeyInitialState.On ? LockKeyInitial.On : LockKeyInitial.Off,
                Behavior: k.LockCapsLock ? LockKeyBehavior.Ignore : LockKeyBehavior.Toggle),
            NumLock: new LockKeySetting(
                Initial: k.NumLockInitial == LockKeyInitialState.On ? LockKeyInitial.On : LockKeyInitial.Off,
                Behavior: k.LockNumLock ? LockKeyBehavior.Ignore : LockKeyBehavior.Toggle),
            ScrollLock: new LockKeySetting(
                Initial: k.ScrollLockInitial == LockKeyInitialState.On ? LockKeyInitial.On : LockKeyInitial.Off,
                Behavior: k.LockScrollLock ? LockKeyBehavior.Ignore : LockKeyBehavior.Toggle)),
        _ => new SkipLockKeySettings(),
    };

    private static IStickyKeysSettings MapStickyKeys(StickyKeysSettings s) => s.Mode switch
    {
        StickyKeysMode.Default => new DefaultStickyKeysSettings(),
        StickyKeysMode.Disable => new DisabledStickyKeysSettings(),
        StickyKeysMode.Configure => new CustomStickyKeysSettings(BuildStickyKeyFlags(s)),
        _ => new DefaultStickyKeysSettings(),
    };

    private static HashSet<SchneegansStickyKeys> BuildStickyKeyFlags(StickyKeysSettings s)
    {
        var flags = new HashSet<SchneegansStickyKeys>();
        if (s.HotKeyActive) flags.Add(SchneegansStickyKeys.HotKeyActive);
        if (s.Indicator) flags.Add(SchneegansStickyKeys.Indicator);
        if (s.TriState) flags.Add(SchneegansStickyKeys.TriState);
        if (s.TwoKeysOff) flags.Add(SchneegansStickyKeys.TwoKeysOff);
        if (s.AudibleFeedback) flags.Add(SchneegansStickyKeys.AudibleFeedback);
        if (s.HotKeySound) flags.Add(SchneegansStickyKeys.HotKeySound);
        return flags;
    }

    // ─── Lookup helper ──────────────────────────────────────────────

    private T? TryLookup<T>(string? key) where T : class, IKeyed
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        try { return _generator.Lookup<T>(key); }
        catch (ConfigurationException) { return null; }
    }
}

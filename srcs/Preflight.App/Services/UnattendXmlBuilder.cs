using System.Collections.Immutable;
using System.Text;
using Preflight.App.Models;
using Schneegans.Unattend;
// Our UnattendConfig and Schneegans.Unattend both declare WindowsEdition - alias
// the library type so the adapter can reference both without ambiguity.
using SchneegansEdition = Schneegans.Unattend.WindowsEdition;
using SchneegansRecoveryMode = Schneegans.Unattend.RecoveryMode;
using UiRecoveryMode = Preflight.App.Models.RecoveryMode;

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

    private Configuration MapToConfiguration(UnattendConfig ui) => Configuration.Default with
    {
        LanguageSettings = MapLanguage(ui.Region),
        AccountSettings = MapAccounts(ui.Users, ui.FirstLogon),
        EditionSettings = MapEdition(ui.Edition),
        PartitionSettings = MapDisk(ui.Disk),
        InstallFromSettings = MapSourceImage(ui.SourceImage),
        PESettings = MapPe(ui.Pe),
        Bloatwares = MapBloatware(ui.Bloatware),
    };

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

    // Ids must match srcs/Preflight.Unattend/resource/WindowsEdition.json verbatim -
    // the generator's Lookup&lt;WindowsEdition&gt; is case-sensitive.
    private static string MapEditionId(Preflight.App.Models.WindowsEdition edition) => edition switch
    {
        Preflight.App.Models.WindowsEdition.Home => "home",
        Preflight.App.Models.WindowsEdition.HomeN => "home_n",
        Preflight.App.Models.WindowsEdition.HomeSingleLanguage => "home_single",
        Preflight.App.Models.WindowsEdition.Pro => "pro",
        Preflight.App.Models.WindowsEdition.ProN => "pro_n",
        Preflight.App.Models.WindowsEdition.ProEducation => "pro_education",
        Preflight.App.Models.WindowsEdition.ProEducationN => "pro_education_n",
        Preflight.App.Models.WindowsEdition.ProForWorkstations => "pro_workstations",
        Preflight.App.Models.WindowsEdition.ProForWorkstationsN => "pro_workstations_n",
        Preflight.App.Models.WindowsEdition.Education => "education",
        Preflight.App.Models.WindowsEdition.EducationN => "education_n",
        Preflight.App.Models.WindowsEdition.Enterprise => "enterprise",
        Preflight.App.Models.WindowsEdition.EnterpriseN => "enterprise_n",
        _ => "pro",
    };

    // ─── Disk / partition mapping ──────────────────────────────────

    private static IPartitionSettings MapDisk(DiskSettings disk) => disk.Mode switch
    {
        DiskMode.Interactive => new InteractivePartitionSettings(),
        DiskMode.AutoWipe => new UnattendedPartitionSettings(
            PartitionLayout: disk.PartitionStyle == PartitionStyle.Gpt ? PartitionLayout.GPT : PartitionLayout.MBR,
            RecoveryMode: MapRecoveryMode(disk.Recovery),
            EspSize: ClampEsp(disk.EspSizeMb),
            RecoverySize: ClampRecovery(disk.RecoverySizeMb)),
        // A blank custom script would throw inside the schneegans modifier; fall
        // back to the interactive path so the preview keeps rendering while the
        // user is still typing their diskpart script.
        DiskMode.CustomScript when string.IsNullOrWhiteSpace(disk.CustomScript)
            => new InteractivePartitionSettings(),
        DiskMode.CustomScript => new CustomPartitionSettings(
            Script: disk.CustomScript!,
            InstallTo: MapInstallTo(disk)),
        _ => new InteractivePartitionSettings(),
    };

    private static SchneegansRecoveryMode MapRecoveryMode(UiRecoveryMode mode) => mode switch
    {
        UiRecoveryMode.OnRecoveryPartition => SchneegansRecoveryMode.Partition,
        UiRecoveryMode.OnWindowsPartition => SchneegansRecoveryMode.Folder,
        UiRecoveryMode.Remove => SchneegansRecoveryMode.None,
        _ => SchneegansRecoveryMode.Partition,
    };

    private static IInstallToSettings MapInstallTo(DiskSettings disk)
    {
        if (disk.InstallDiskIndex is int d && disk.InstallPartitionIndex is int p)
        {
            try { return new CustomInstallToSettings(d, p); }
            catch (ConfigurationException) { return new AvailableInstallToSettings(); }
        }
        return new AvailableInstallToSettings();
    }

    private static int ClampEsp(int mb) => mb is < 100 or > 2048 ? Constants.EspDefaultSize : mb;
    private static int ClampRecovery(int mb) => mb is < 300 or > 4096 ? Constants.RecoveryPartitionSize : mb;

    // ─── Source image mapping ──────────────────────────────────────

    private static IInstallFromSettings MapSourceImage(SourceImageSettings src) => src.Mode switch
    {
        SourceImageMode.Automatic => new AutomaticInstallFromSettings(),
        SourceImageMode.ByIndex => src.ImageIndex > 0
            ? new IndexInstallFromSettings(src.ImageIndex)
            : new AutomaticInstallFromSettings(),
        SourceImageMode.ByName when !string.IsNullOrWhiteSpace(src.ImageName)
            => new NameInstallFromSettings(src.ImageName!),
        _ => new AutomaticInstallFromSettings(),
    };

    // ─── Windows PE mapping ────────────────────────────────────────

    private static IPESettings MapPe(WindowsPeSettings pe) => pe.Mode switch
    {
        PeMode.Default => new DefaultPESettings(),
        PeMode.Generated => new GeneratePESettings(
            Disable8Dot3Names: pe.Disable8Dot3Names,
            PauseBeforeFormatting: pe.PauseBeforePartition,
            PauseBeforeReboot: pe.PauseBeforeReboot),
        PeMode.Custom when !string.IsNullOrWhiteSpace(pe.CustomCmd)
            => new ScriptPESetttings(pe.CustomCmd!),
        _ => new DefaultPESettings(),
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

    // ─── Lookup helper ──────────────────────────────────────────────

    private T? TryLookup<T>(string? key) where T : class, IKeyed
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        try { return _generator.Lookup<T>(key); }
        catch (ConfigurationException) { return null; }
    }
}

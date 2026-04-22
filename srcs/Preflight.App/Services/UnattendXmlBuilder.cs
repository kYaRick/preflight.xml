using System.Collections.Immutable;
using System.Text;
using Preflight.App.Models;
using Schneegans.Unattend;
// Our UnattendConfig and Schneegans.Unattend both declare WindowsEdition - alias
// the library type so the adapter can reference both without ambiguity.
using SchneegansEdition = Schneegans.Unattend.WindowsEdition;

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
        LockoutSettings = MapLockout(ui.AccountSecurity),
        PasswordExpirationSettings = MapPasswordExpiration(ui.AccountSecurity),
        EditionSettings = MapEdition(ui.Edition),
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
        var named = users.Where(u => !string.IsNullOrWhiteSpace(u.Name)).ToList();

        // Built-in administrator auto-logon is a special case: schneegans allows zero
        // declared accounts because Windows Setup will log straight into the built-in
        // Administrator user using the supplied password.
        var usingBuiltin = firstLogon.Mode == FirstLogonMode.BuiltInAdministrator;

        // Empty list / no admin → the UnattendedAccountSettings constructor enforces
        // "must have at least one administrator" (unless the auto-logon is builtin).
        // Instead of throwing, fall back to an interactive account-setup path so the
        // preview still renders. Which interactive flavor depends on the user's
        // explicit "prompt" checkboxes - local wins if both are set.
        if (named.Count == 0 && !usingBuiltin)
        {
            return PickInteractive(firstLogon);
        }
        if (named.Count > 0 && !named.Any(u => u.Group == AccountGroup.Administrators) && !usingBuiltin)
        {
            return PickInteractive(firstLogon);
        }

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
            FirstLogonMode.BuiltInAdministrator => new BuiltinAutoLogonSettings(firstLogon.BuiltInAdminPassword ?? string.Empty),
            FirstLogonMode.DoNotLogon => new NoneAutoLogonSettings(),
            _ => new NoneAutoLogonSettings(),
        };

        return new UnattendedAccountSettings(accounts, autoLogon, firstLogon.ObscurePasswordsWithBase64);
    }

    /// <summary>
    /// When the UI has no admin / no accounts, pick an interactive flavor. Local-prompt
    /// takes precedence (it hides the MSA screens); MSA-prompt is the plain interactive
    /// flow; if neither box is checked we still default to local-account since that's the
    /// safer assumption for a debloat-oriented tool.
    /// </summary>
    private static IAccountSettings PickInteractive(FirstLogonSettings firstLogon)
    {
        if (firstLogon.PromptForLocalAccount) return new InteractiveLocalAccountSettings();
        if (firstLogon.PromptForMicrosoftAccount) return new InteractiveMicrosoftAccountSettings();
        return new InteractiveLocalAccountSettings();
    }

    private static ILockoutSettings MapLockout(AccountSecuritySettings sec) => sec.Lockout switch
    {
        LockoutMode.Disabled => new DisableLockoutSettings(),
        LockoutMode.Custom => SafeCustomLockout(sec),
        _ => new DefaultLockoutSettings(),
    };

    private static ILockoutSettings SafeCustomLockout(AccountSecuritySettings sec)
    {
        // Schneegans' CustomLockoutSettings throws on out-of-range or window>duration.
        // Swallow here and fall back to defaults so the preview explains what's wrong via
        // the outer Build()'s try/catch rather than blanking the whole document.
        try
        {
            return new CustomLockoutSettings(
                lockoutThreshold: sec.LockoutAttempts,
                lockoutDuration: sec.LockoutUnlockMinutes,
                lockoutWindow: sec.LockoutWindowMinutes);
        }
        catch (ConfigurationException)
        {
            return new DefaultLockoutSettings();
        }
    }

    private static IPasswordExpirationSettings MapPasswordExpiration(AccountSecuritySettings sec) => sec.PasswordExpiration switch
    {
        PasswordExpirationMode.Never => new UnlimitedPasswordExpirationSettings(),
        PasswordExpirationMode.Custom => SafeCustomExpiration(sec.PasswordExpirationDays),
        _ => new DefaultPasswordExpirationSettings(),
    };

    private static IPasswordExpirationSettings SafeCustomExpiration(int days)
    {
        try { return new CustomPasswordExpirationSettings(days); }
        catch (ConfigurationException) { return new DefaultPasswordExpirationSettings(); }
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

    // ─── Lookup helper ──────────────────────────────────────────────

    private T? TryLookup<T>(string? key) where T : class, IKeyed
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        try { return _generator.Lookup<T>(key); }
        catch (ConfigurationException) { return null; }
    }
}

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
    /// Read-only view over the embedded bloatware catalog for UI consumers
    /// (see <see cref="BloatwareCatalog"/>). Returned as plain
    /// <see cref="Bloatware"/> instances so callers can pick off
    /// <c>Id</c> / <c>DisplayName</c> without a deeper schneegans dependency.
    /// </summary>
    public IEnumerable<Bloatware> GetBloatwareCatalog() => _generator.Bloatwares.Values;

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
        Bloatwares = MapBloatware(ui.Bloatware),
        WifiSettings = MapWifi(ui.Network),
        ColorSettings = MapColors(ui.Personalization.Colors),
        WallpaperSettings = MapWallpaper(ui.Personalization.Wallpaper),
        LockScreenSettings = MapLockScreen(ui.Personalization.LockScreen),
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

    private static IWifiSettings MapWifi(NetworkSettings net)
    {
        switch (net.WifiMode)
        {
            case Preflight.App.Models.WifiMode.Skip:
                return new SkipWifiSettings();
            case Preflight.App.Models.WifiMode.Configure:
                // An SSID is mandatory; fall back to Interactive until the user types one so
                // the preview keeps rendering instead of blanking on ConfigurationException.
                if (string.IsNullOrWhiteSpace(net.Ssid))
                    return new InteractiveWifiSettings();
                var auth = net.Auth switch
                {
                    Preflight.App.Models.WifiAuth.Open         => WifiAuthentications.Open,
                    Preflight.App.Models.WifiAuth.Wpa3Personal => WifiAuthentications.WPA3SAE,
                    _                                          => WifiAuthentications.WPA2PSK,
                };
                // Open nets get an empty password; the schneegans builder ignores it.
                var pwd = auth == WifiAuthentications.Open ? string.Empty : (net.Password ?? string.Empty);
                return new ParameterizedWifiSettings(
                    Name: net.Ssid!,
                    Password: pwd,
                    ConnectAutomatically: true,
                    Authentication: auth,
                    NonBroadcast: net.SsidHidden);
            case Preflight.App.Models.WifiMode.ProfileXml:
                if (string.IsNullOrWhiteSpace(net.ProfileXml))
                    return new InteractiveWifiSettings();
                // XmlWifiSettings throws ConfigurationException on bad XML — catch so a
                // half-typed profile doesn't blow up the whole preview.
                try { return new XmlWifiSettings(net.ProfileXml!); }
                catch (ConfigurationException) { return new InteractiveWifiSettings(); }
            default:
                return new InteractiveWifiSettings();
        }
    }

    private static IColorSettings MapColors(ColorSettings c)
    {
        if (c.Mode != ColorMode.Custom) return new DefaultColorSettings();

        // The custom path needs a parseable hex string; on garbage input fall back to default.
        if (!TryParseHtmlColor(c.AccentColor, out var accent))
            return new DefaultColorSettings();

        return new CustomColorSettings(
            SystemTheme: c.TaskbarAndStartTheme == Preflight.App.Models.ColorTheme.Light
                ? Schneegans.Unattend.ColorTheme.Light
                : Schneegans.Unattend.ColorTheme.Dark,
            AppsTheme: c.AppsTheme == Preflight.App.Models.ColorTheme.Light
                ? Schneegans.Unattend.ColorTheme.Light
                : Schneegans.Unattend.ColorTheme.Dark,
            EnableTransparency: c.Translucent,
            AccentColorOnStart: c.AccentOnStartAndTaskbar,
            AccentColorOnBorders: c.AccentOnTitleBars,
            AccentColor: accent);
    }

    private static IWallpaperSettings MapWallpaper(WallpaperSettings w) => w.Mode switch
    {
        WallpaperMode.SolidColor when TryParseHtmlColor(w.SolidColor, out var c)
            => new SolidWallpaperSettings(c),
        WallpaperMode.Script when !string.IsNullOrWhiteSpace(w.Script)
            => new ScriptWallpaperSettings(w.Script!),
        _ => new DefaultWallpaperSettings(),
    };

    private static ILockScreenSettings MapLockScreen(LockScreenSettings l) => l.Mode switch
    {
        LockScreenMode.Script when !string.IsNullOrWhiteSpace(l.Script)
            => new ScriptLockScreenSettings(l.Script!),
        _ => new DefaultLockScreenSettings(),
    };

    private static bool TryParseHtmlColor(string? html, out System.Drawing.Color color)
    {
        color = System.Drawing.Color.Empty;
        if (string.IsNullOrWhiteSpace(html)) return false;
        try { color = System.Drawing.ColorTranslator.FromHtml(html.Trim()); return true; }
        catch (Exception) { return false; }
    }

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

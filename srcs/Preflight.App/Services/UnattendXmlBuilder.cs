using System.Collections.Immutable;
using System.Text;
using Preflight.App.Models;
using Schneegans.Unattend;
// Our UnattendConfig and Schneegans.Unattend both declare WindowsEdition - alias
// the library type so the adapter can reference both without ambiguity.
using SchneegansEdition = Schneegans.Unattend.WindowsEdition;
// Same trick for the shell-cluster models: the UI enum names shadow schneegans' canonical
// types (TaskbarSearchMode, Effect, DesktopIcon, StartFolder, HideModes). Aliasing the UI
// side keeps the mapper readable without forcing the UI code into awkward full-qualification.
using UiExplorerHide = Preflight.App.Models.ExplorerHideFiles;
using UiTaskbarSearch = Preflight.App.Models.TaskbarSearchMode;
using UiVisualEffect = Preflight.App.Models.VisualEffect;

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
        var baseCfg = Configuration.Default with
        {
            LanguageSettings = MapLanguage(ui.Region),
            AccountSettings = MapAccounts(ui.Users, ui.FirstLogon),
            EditionSettings = MapEdition(ui.Edition),
            Bloatwares = MapBloatware(ui.Bloatware),
        };

        // Shell cluster (Phase B). Each Map* helper returns a Configuration mutated with the
        // relevant fields; applied in order so later ones see the earlier values (none of
        // them currently overlap, but ordering is cheap insurance).
        baseCfg = MapExplorer(baseCfg, ui.Explorer);
        baseCfg = MapStartMenu(baseCfg, ui.StartMenu);
        baseCfg = MapVisualEffects(baseCfg, ui.VisualEffects);
        baseCfg = MapDesktopIcons(baseCfg, ui.DesktopIcons);
        baseCfg = MapStartFolders(baseCfg, ui.StartFolders);
        return baseCfg;
    }

    // ─── Shell cluster mappers ──────────────────────────────────────

    private static Configuration MapExplorer(Configuration cfg, ExplorerSettings ui) => cfg with
    {
        ClassicContextMenu = ui.ClassicContextMenu,
        ShowFileExtensions = ui.ShowFileExtensions,
        HideInfoTip = ui.HideInfoTip,
        LaunchToThisPC = ui.LaunchToThisPC,
        ShowEndTask = ui.ShowEndTask,
        HideFiles = ui.HideFiles switch
        {
            UiExplorerHide.ShowAll => HideModes.None,
            UiExplorerHide.OsOnly => HideModes.HiddenSystem,
            _ => HideModes.Hidden,
        },
    };

    private static Configuration MapStartMenu(Configuration cfg, StartMenuSettings ui) => cfg with
    {
        TaskbarSearch = ui.TaskbarSearch switch
        {
            UiTaskbarSearch.Hide => Schneegans.Unattend.TaskbarSearchMode.Hide,
            UiTaskbarSearch.Icon => Schneegans.Unattend.TaskbarSearchMode.Icon,
            UiTaskbarSearch.Label => Schneegans.Unattend.TaskbarSearchMode.Label,
            _ => Schneegans.Unattend.TaskbarSearchMode.Box,
        },
        DisableWidgets = ui.DisableWidgets,
        LeftTaskbar = ui.LeftTaskbar,
        HideTaskViewButton = ui.HideTaskViewButton,
        ShowAllTrayIcons = ui.ShowAllTrayIcons,
        DisableBingResults = ui.DisableBingResults,
        TaskbarIcons = ui.TaskbarIcons switch
        {
            TaskbarIconsMode.RemoveAll => new EmptyTaskbarIcons(),
            // An empty / null payload falls back to Default: the Custom variant's ctor does
            // schema validation inside schneegans and we'd rather defer the error until the
            // user actually types something, rather than error the whole preview immediately.
            TaskbarIconsMode.CustomXml when !string.IsNullOrWhiteSpace(ui.TaskbarIconsXml)
                => new CustomTaskbarIcons(ui.TaskbarIconsXml!),
            _ => new DefaultTaskbarIcons(),
        },
        StartTilesSettings = ui.StartTiles switch
        {
            StartTilesMode.RemoveAll => new EmptyStartTilesSettings(),
            StartTilesMode.CustomXml when !string.IsNullOrWhiteSpace(ui.StartTilesXml)
                => new CustomStartTilesSettings(ui.StartTilesXml!),
            _ => new DefaultStartTilesSettings(),
        },
        StartPinsSettings = ui.StartPins switch
        {
            StartPinsMode.RemoveAll => new EmptyStartPinsSettings(),
            StartPinsMode.CustomJson when !string.IsNullOrWhiteSpace(ui.StartPinsJson)
                => new CustomStartPinsSettings(ui.StartPinsJson!),
            _ => new DefaultStartPinsSettings(),
        },
    };

    private static Configuration MapVisualEffects(Configuration cfg, VisualEffectsSettings ui) => cfg with
    {
        Effects = ui.Preset switch
        {
            VisualEffectsPreset.BestAppearance => new BestAppearanceEffects(),
            VisualEffectsPreset.BestPerformance => new BestPerformanceEffects(),
            VisualEffectsPreset.Custom => BuildCustomEffects(ui.CustomEffects),
            _ => new DefaultEffects(),
        },
    };

    // Materialize the UI dictionary into the schneegans ImmutableDictionary<Effect, bool>.
    // Any effect missing from the UI dictionary defaults to false - the mapping is a direct
    // parse because UI and schneegans enums share identical names. Concrete Dictionary
    // parameter (not IDictionary) to satisfy CA1859 - the settings object always uses it.
    private static CustomEffects BuildCustomEffects(Dictionary<UiVisualEffect, bool> ui)
    {
        var builder = ImmutableDictionary.CreateBuilder<Effect, bool>();
        foreach (var effect in Enum.GetValues<Effect>())
        {
            if (Enum.TryParse<UiVisualEffect>(effect.ToString(), out var uiEffect)
                && ui.TryGetValue(uiEffect, out var v))
            {
                builder[effect] = v;
            }
            else
            {
                builder[effect] = false;
            }
        }
        return new CustomEffects(builder.ToImmutable());
    }

    private Configuration MapDesktopIcons(Configuration cfg, DesktopIconSettings ui) => cfg with
    {
        DeleteEdgeDesktopIcon = ui.DeleteEdgeDesktopIcon,
        DesktopIcons = ui.Mode switch
        {
            DesktopIconMode.Specific => BuildCustomDesktopIcons(ui.VisibleIcons),
            _ => new DefaultDesktopIconSettings(),
        },
    };

    // Concrete return type keeps CA1859 happy; callers assign into an IDesktopIconSettings slot
    // on the Configuration record, so covariance is unaffected.
    private CustomDesktopIconSettings BuildCustomDesktopIcons(HashSet<string> visible)
    {
        // Load every known icon; mark the ones the user picked as true, the rest false.
        // Unknown ids in the config are skipped - matches the bloatware pattern so stale
        // presets don't blow up the preview.
        var dict = new Dictionary<DesktopIcon, bool>();
        foreach (var icon in _generator.DesktopIcons.Values)
        {
            dict[icon] = visible.Contains(icon.Id);
        }
        return new CustomDesktopIconSettings(dict);
    }

    private Configuration MapStartFolders(Configuration cfg, StartFolderSettings ui) => cfg with
    {
        StartFolderSettings = ui.Mode switch
        {
            StartFolderMode.Specific => BuildCustomStartFolders(ui.VisibleFolders),
            _ => new DefaultStartFolderSettings(),
        },
    };

    private CustomStartFolderSettings BuildCustomStartFolders(HashSet<string> visible)
    {
        var dict = new Dictionary<StartFolder, bool>();
        foreach (var folder in _generator.StartFolders.Values)
        {
            dict[folder] = visible.Contains(folder.Id);
        }
        return new CustomStartFolderSettings(dict);
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

    // ─── Lookup helper ──────────────────────────────────────────────

    private T? TryLookup<T>(string? key) where T : class, IKeyed
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        try { return _generator.Lookup<T>(key); }
        catch (ConfigurationException) { return null; }
    }
}

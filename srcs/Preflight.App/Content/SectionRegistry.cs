using Preflight.App.Content.Sections;
using Preflight.App.Models;

namespace Preflight.App.Content;

/// <summary>
/// Single source of truth for every section the Advanced / Docs / Wizard surfaces know about.
/// Data-driven sections expose a <see cref="SectionDefinition"/>; sections with bespoke UI
/// skip the definition (or publish it only for header metadata) and render via a dedicated
/// <c>.razor</c> that AdvancedShell switches on by id.
///
/// Keys are the URL-safe slugs used by <c>/advanced/{id}</c> and <c>/docs/{id}</c>.
/// </summary>
public static class SectionRegistry
{
    /// <summary>All registered section ids in display order (used by nav menus).</summary>
    public static readonly IReadOnlyList<string> AllSectionIds =
    [
        "region",
        "tweaks",
        "vm-support",
        "express-settings",
        "lock-keys",
        "sticky-keys",
        "wdac",
        "applocker",
        "scripts",
        "components",
    ];

    /// <summary>Section ids that render via a bespoke <c>.razor</c> component instead of the generic <see cref="Layout.SectionView"/>.</summary>
    public static readonly IReadOnlySet<string> CustomSectionIds =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "lock-keys",
            "sticky-keys",
            "scripts",
            "components",
        };

    /// <summary>Public dictionary exposed for pages that want to enumerate data-driven defs (e.g. Docs).</summary>
    public static IReadOnlyDictionary<string, SectionDefinition> Definitions => _defs;

    private static readonly Dictionary<string, SectionDefinition> _defs = new(StringComparer.Ordinal)
    {
        ["region"] = RegionSection.Definition,
        ["tweaks"] = SystemTweaksSection.Definition,
        ["vm-support"] = VmSupportSection.Definition,
        ["express-settings"] = ExpressSettingsSection.Definition,
        // lock-keys / sticky-keys render via bespoke .razor components, but we still register
        // their metadata so header / nav labels have a single source of truth.
        ["lock-keys"] = LockKeysSection.Definition,
        ["sticky-keys"] = StickyKeysSection.Definition,
        ["wdac"] = WdacSection.Definition,
        ["applocker"] = AppLockerSection.Definition,
    };

    public static SectionDefinition? TryGet(string? id) =>
        id is not null && _defs.TryGetValue(id, out var def) ? def : null;

    public static bool IsCustom(string? id) =>
        id is not null && CustomSectionIds.Contains(id);
}

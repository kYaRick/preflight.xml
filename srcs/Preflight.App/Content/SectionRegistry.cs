using Preflight.App.Content.Sections;
using Preflight.App.Models;

namespace Preflight.App.Content;

/// <summary>
/// Central lookup of every data-driven <see cref="SectionDefinition"/>. Rather than asking
/// each page (Docs / Advanced / Wizard) to import every section class by name, callers go
/// through here so adding a section is a one-line registry edit.
///
/// Sections that need a bespoke layout (lock-keys, sticky-keys) are identified by id here
/// but rendered by a dedicated <c>.razor</c> component the calling page switches on.
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
    ];

    /// <summary>Section ids that render via a custom .razor component instead of the data-driven SectionView.</summary>
    public static readonly IReadOnlySet<string> CustomSectionIds =
        new HashSet<string>(StringComparer.Ordinal) { "lock-keys", "sticky-keys" };

    private static readonly Dictionary<string, SectionDefinition> _defs = new(StringComparer.Ordinal)
    {
        ["region"] = RegionSection.Definition,
        ["tweaks"] = SystemTweaksSection.Definition,
        ["vm-support"] = VmSupportSection.Definition,
        ["express-settings"] = ExpressSettingsSection.Definition,
        // lock-keys / sticky-keys render via bespoke .razor components, but we still register
        // their section metadata so header / nav labels can come from a single source of truth.
        ["lock-keys"] = LockKeysSection.Definition,
        ["sticky-keys"] = StickyKeysSection.Definition,
    };

    public static SectionDefinition? TryGet(string id) =>
        _defs.TryGetValue(id, out var def) ? def : null;

    public static bool IsCustom(string id) => CustomSectionIds.Contains(id);
}

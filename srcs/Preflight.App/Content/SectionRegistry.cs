using Preflight.App.Content.Sections;
using Preflight.App.Models;

namespace Preflight.App.Content;

/// <summary>
/// Single source of truth for every section the Advanced / Docs / Wizard surfaces know about.
/// Sections are registered with their definitions (or null for fully custom runtime sections).
/// The registry maintains ordering via <see cref="AllSectionIds"/>, which is the authoritative
/// list for nav menus and other enumeration surfaces.
/// </summary>
public static class SectionRegistry
{
    /// <summary>Top-level groupings shown as collapsible headers in the Advanced / Docs nav.</summary>
    public static readonly IReadOnlyList<SectionGroup> Groups =
    [
        new SectionGroup("setup-id", "Advanced.Group.SetupId", "🧭",
            ["region", "time-zone", "computer-name", "setup-settings"]),
        new SectionGroup("install", "Advanced.Group.Install", "💽",
            ["source-image", "edition", "windows-pe", "disk"]),
        new SectionGroup("system", "Advanced.Group.System", "🔧",
            ["tweaks", "vm-support", "express-settings", "wdac", "applocker", "processor-archs", "compact-os"]),
        new SectionGroup("desktop", "Advanced.Group.Desktop", "🎨",
            ["explorer", "visual-effects", "desktop-icons", "start-menu", "start-folders", "personalization", "network"]),
        new SectionGroup("advanced-features", "Advanced.Group.Advanced", "⚡",
            ["lock-keys", "sticky-keys", "scripts", "components", "bloatware"]),
        new SectionGroup("users", "Advanced.Group.Users", "👤",
            ["users"]),
    ];

    /// <summary>All registered section ids in display order (used by nav menus and enumeration).</summary>
    public static readonly IReadOnlyList<string> AllSectionIds =
        Groups.SelectMany(g => g.SectionIds).ToArray();

    /// <summary>Section ids that render via a bespoke <c>.razor</c> component instead of the generic <see cref="Layout.SectionView"/>.</summary>
    public static readonly IReadOnlySet<string> CustomSectionIds =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "lock-keys",
            "sticky-keys",
            "scripts",
            "components",
            "network",
            "personalization",
            "bloatware",
            "start-folders",
            "start-menu",
            "computer-name",
            "time-zone",
            "users",
            "visual-effects",
            "desktop-icons",
        };

    /// <summary>Public dictionary exposed for pages that want to enumerate data-driven defs (e.g. Docs).</summary>
    public static IReadOnlyDictionary<string, SectionDefinition?> Definitions => _defs;

    private static readonly Dictionary<string, SectionDefinition?> _defs = new(StringComparer.Ordinal)
    {
        // Setup & Identification
        ["region"] = RegionSection.Definition,
        ["time-zone"] = TimeZoneSectionDefinition.Definition,
        ["computer-name"] = ComputerNameSectionDefinition.Definition,
        ["setup-settings"] = SetupSettingsSection.Definition,

        // Installation & System Image
        ["source-image"] = SourceImageSectionDefinition.Definition,
        ["edition"] = EditionSectionDefinition.Definition,
        ["windows-pe"] = WindowsPeSectionDefinition.Definition,
        ["disk"] = DiskSectionDefinition.Definition,

        // System Configuration
        ["tweaks"] = SystemTweaksSection.Definition,
        ["vm-support"] = VmSupportSection.Definition,
        ["express-settings"] = ExpressSettingsSection.Definition,
        ["wdac"] = WdacSection.Definition,
        ["applocker"] = AppLockerSection.Definition,
        ["processor-archs"] = ProcessorArchsSection.Definition,
        ["compact-os"] = CompactOsSection.Definition,

        // Desktop & Personalization
        ["explorer"] = ExplorerSection.Definition,
        ["visual-effects"] = VisualEffectsSection.Definition,
        ["desktop-icons"] = DesktopIconsSection.Definition,
        // Custom sections with bespoke components; definitions stored for nav metadata
        ["start-menu"] = StartMenuSection.Definition,
        ["start-folders"] = StartFoldersSection.Definition,
        ["personalization"] = null,  // Renders via PersonalizationSectionView
        ["network"] = null,           // Renders via NetworkSectionView

        // Advanced Features
        ["lock-keys"] = LockKeysSection.Definition,
        ["sticky-keys"] = StickyKeysSection.Definition,
        ["scripts"] = null,           // Custom .razor (Scripts category)
        ["components"] = null,        // Custom .razor (Components category)
        ["bloatware"] = null,         // Renders via BloatwareSectionView with injected catalog

        // User Management
        ["users"] = null,             // Renders via UsersSection.razor
    };

    public static SectionDefinition? TryGet(string? id) =>
        id is not null && _defs.TryGetValue(id, out var def) ? def : null;

    public static bool IsCustom(string? id) =>
        id is not null && CustomSectionIds.Contains(id);
}

/// <summary>One collapsible group in the Advanced / Docs side navigation.</summary>
public sealed record SectionGroup(
    string Id,
    string TitleKey,
    string Emoji,
    IReadOnlyList<string> SectionIds);

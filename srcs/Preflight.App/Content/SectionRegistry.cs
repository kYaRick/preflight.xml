using Preflight.App.Content.Sections;
using Preflight.App.Models;

namespace Preflight.App.Content;

/// <summary>
/// Central list of section definitions used by the Advanced shell and the Docs index.
/// One entry per section id; the id doubles as the URL segment (<c>/advanced/{id}</c>)
/// and as the resx key suffix (<c>Advanced.Section.{id}</c>).
///
/// Phase B foundation added <c>region</c>; the Phase B shell cluster appends
/// <c>explorer</c>, <c>start-menu</c>, <c>visual-effects</c>, <c>desktop-icons</c>
/// and <c>folders-start</c>. Sections still stubbed in <c>AdvancedShell</c>'s
/// "coming in Phase 3" branch stay here as ids only (no definition yet) so the
/// navigation menu is still exhaustive.
/// </summary>
public static class SectionRegistry
{
    /// <summary>Sections that have a concrete <see cref="SectionDefinition"/> wired up.</summary>
    public static readonly IReadOnlyDictionary<string, SectionDefinition> Definitions =
        new Dictionary<string, SectionDefinition>(StringComparer.Ordinal)
        {
            [RegionSection.Definition.Id]        = RegionSection.Definition,
            [ExplorerSection.Definition.Id]      = ExplorerSection.Definition,
            [StartMenuSection.Definition.Id]     = StartMenuSection.Definition,
            [VisualEffectsSection.Definition.Id] = VisualEffectsSection.Definition,
            [DesktopIconsSection.Definition.Id]  = DesktopIconsSection.Definition,
            [StartFoldersSection.Definition.Id]  = StartFoldersSection.Definition,
        };

    /// <summary>Ordered list of section ids the Advanced nav should render (includes stubs).</summary>
    public static readonly IReadOnlyList<string> AllSectionIds =
    [
        // Phase B foundation + shell cluster (complete).
        "region",
        "explorer",
        "start-menu",
        "visual-effects",
        "desktop-icons",
        "folders-start",
        // Remaining Phase 3 stubs - shown disabled in the shell until wired.
        "disk", "users", "edition", "privacy",
        "security", "bloatware", "tweaks", "vm", "network",
    ];

    public static SectionDefinition? TryGet(string id) =>
        Definitions.TryGetValue(id, out var def) ? def : null;
}

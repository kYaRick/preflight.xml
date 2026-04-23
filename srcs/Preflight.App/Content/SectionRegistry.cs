using Preflight.App.Content.Sections;
using Preflight.App.Models;

namespace Preflight.App.Content;

/// <summary>
/// Single source of truth for every section the Advanced/Docs/Wizard surfaces know about.
/// Data-driven sections expose an <see cref="SectionDefinition"/>; sections with bespoke UI
/// (e.g. Scripts' four-phase grid, Components' dropdown-per-slot) skip the definition and
/// have their <c>.razor</c> rendered by AdvancedShell's custom switch.
///
/// Keys are the same URL-safe slugs used for routing (/advanced/{id}, /docs/{id}).
/// </summary>
public static class SectionRegistry
{
    public static readonly IReadOnlyDictionary<string, SectionDefinition> Definitions =
        new Dictionary<string, SectionDefinition>(StringComparer.Ordinal)
        {
            ["region"] = RegionSection.Definition,
            ["wdac"] = WdacSection.Definition,
            ["applocker"] = AppLockerSection.Definition,
        };

    /// <summary>Slugs of sections that have a custom <c>.razor</c> rather than a <see cref="SectionDefinition"/>.</summary>
    public static readonly IReadOnlySet<string> CustomRazorSections =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "scripts",
            "components",
        };

    public static SectionDefinition? TryGet(string? id) =>
        id is not null && Definitions.TryGetValue(id, out var def) ? def : null;

    public static bool IsCustom(string? id) =>
        id is not null && CustomRazorSections.Contains(id);
}

using Preflight.App.Content.Sections;
using Preflight.App.Models;

namespace Preflight.App.Content;

/// <summary>
/// Central lookup for <see cref="SectionDefinition"/>s so <c>AdvancedShell</c> and the Docs
/// pages don't hand-roll their own switch statements. Sections that have a dedicated
/// <c>.razor</c> component (Network, Personalization, Bloatware ship bespoke layouts
/// beyond what <see cref="Layout.SectionView"/> can express) register <c>null</c> here;
/// the shell then dispatches to the companion Razor file.
/// </summary>
public static class SectionRegistry
{
    /// <summary>
    /// Section id → <see cref="SectionDefinition"/>. A missing entry (or a <c>null</c> value)
    /// means the section has a bespoke <c>.razor</c> file that the Advanced shell renders directly.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, SectionDefinition?> All =
        new Dictionary<string, SectionDefinition?>(StringComparer.Ordinal)
        {
            ["region"] = RegionSection.Definition,
            // Sections below are rendered by custom .razor files — see AdvancedShell's switch.
            ["network"] = null,
            ["personalization"] = null,
            ["bloatware"] = null,
        };

    public static SectionDefinition? Get(string id) =>
        All.TryGetValue(id, out var def) ? def : null;
}

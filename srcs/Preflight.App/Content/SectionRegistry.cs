using Preflight.App.Content.Sections;
using Preflight.App.Models;

namespace Preflight.App.Content;

/// <summary>
/// Central lookup of <see cref="SectionDefinition"/>s keyed by their URL id.
/// AdvancedShell and DocsEntry both consult this registry so a new section only
/// needs one entry here to show up on every surface it touches.
/// </summary>
/// <remarks>
/// Sections that require custom interaction beyond the built-in <see cref="OptionKind"/>s
/// (dependent controls, multi-step flows) register their id here with a minimal
/// <see cref="SectionDefinition"/>; AdvancedShell then renders a dedicated razor
/// component via <see cref="GetCustomComponent"/>. Plain data-driven sections
/// ignore that path and flow through <see cref="Layout.SectionView"/>.
/// </remarks>
public static class SectionRegistry
{
    private static readonly Dictionary<string, SectionDefinition> _all = new(StringComparer.Ordinal)
    {
        ["region"] = RegionSection.Definition,
        ["processor-archs"] = ProcessorArchsSection.Definition,
        ["setup-settings"] = SetupSettingsSection.Definition,
        ["computer-name"] = ComputerNameSectionDefinition.Definition,
        ["compact-os"] = CompactOsSection.Definition,
        ["time-zone"] = TimeZoneSectionDefinition.Definition,
    };

    public static SectionDefinition? TryGet(string id) =>
        _all.TryGetValue(id, out var def) ? def : null;

    public static IReadOnlyDictionary<string, SectionDefinition> All => _all;

    /// <summary>Section ids whose Advanced-view rendering is not driven by SectionView.</summary>
    public static bool IsCustom(string id) => id is "computer-name" or "time-zone";
}

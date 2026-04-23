using Preflight.App.Content.Sections;
using Preflight.App.Models;

namespace Preflight.App.Content;

/// <summary>
/// Central lookup of every <see cref="SectionDefinition"/> exposed by the app.
/// <see cref="AdvancedShell"/> and the Docs router both hit this dictionary so
/// adding a new section is a one-line edit here instead of a fresh branch in
/// every view.
///
/// Sections whose Advanced form needs conditional layout (disk, edition, ...) have
/// their own <c>.razor</c> component; see <see cref="HasCustomComponent"/> for the
/// dispatch switch. The metadata here still covers the Docs rendering and the
/// sidebar labels.
/// </summary>
public static class SectionRegistry
{
    public static readonly IReadOnlyDictionary<string, SectionDefinition> All =
        new Dictionary<string, SectionDefinition>(StringComparer.Ordinal)
        {
            [RegionSection.Definition.Id] = RegionSection.Definition,
            [DiskSectionDefinition.Definition.Id] = DiskSectionDefinition.Definition,
            [EditionSectionDefinition.Definition.Id] = EditionSectionDefinition.Definition,
            [SourceImageSectionDefinition.Definition.Id] = SourceImageSectionDefinition.Definition,
            [WindowsPeSectionDefinition.Definition.Id] = WindowsPeSectionDefinition.Definition,
        };

    /// <summary>Stable render order used by the Advanced sidebar.</summary>
    public static readonly IReadOnlyList<string> Order =
    [
        RegionSection.Definition.Id,
        DiskSectionDefinition.Definition.Id,
        EditionSectionDefinition.Definition.Id,
        SourceImageSectionDefinition.Definition.Id,
        WindowsPeSectionDefinition.Definition.Id,
    ];

    public static SectionDefinition? TryGet(string id) =>
        id is null ? null : (All.TryGetValue(id, out var s) ? s : null);

    /// <summary>
    /// Section ids that render a custom <c>.razor</c> component instead of the
    /// generic flat <see cref="Layout.SectionView"/>. AdvancedShell uses this to
    /// pick the right renderer.
    /// </summary>
    public static bool HasCustomComponent(string id) => id is "disk" or "edition" or "source-image" or "windows-pe";
}

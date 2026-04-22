using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Desktop-icon visibility. Schneegans only surfaces Default vs. Custom-with-dictionary, so
/// we expose a radio for that plus the independent <c>DeleteEdgeDesktopIcon</c> checkbox.
/// The list of icons for the Specific mode is rendered by <c>DesktopIconsAdvanced.razor</c>,
/// which fetches <c>data/desktop-icons.json</c> at runtime.
/// </summary>
public static class DesktopIconsSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "desktop-icons",
        TitleKey = "Advanced.Section.desktop-icons",
        SubtitleKey = "Section.desktop-icons.Subtitle",
        IntroMarkdownPath = "content/sections/desktop-icons.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "mode",
                LabelKey = "DesktopIcons.Mode.Label",
                DescriptionKey = "DesktopIcons.Mode.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new OptionValue(nameof(DesktopIconMode.Default),  "DesktopIcons.Mode.Default"),
                    new OptionValue(nameof(DesktopIconMode.Specific), "DesktopIcons.Mode.Specific"),
                ],
                GetString = c => c.DesktopIcons.Mode.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<DesktopIconMode>(v, out var parsed))
                        c.DesktopIcons.Mode = parsed;
                },
            },
            new OptionDefinition
            {
                Id = "delete-edge-desktop-icon",
                LabelKey = "DesktopIcons.DeleteEdge.Label",
                DescriptionKey = "DesktopIcons.DeleteEdge.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.DesktopIcons.DeleteEdgeDesktopIcon,
                SetBool = (c, v) => c.DesktopIcons.DeleteEdgeDesktopIcon = v,
            },
        ],
    };
}

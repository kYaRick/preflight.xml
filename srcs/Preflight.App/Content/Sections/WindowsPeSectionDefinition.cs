using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Static metadata for the <c>windows-pe</c> section (PE-stage scripting behaviour).
/// Live layout is <see cref="WindowsPeSection"/>.
/// </summary>
public static class WindowsPeSectionDefinition
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "windows-pe",
        TitleKey = "Advanced.Section.windows-pe",
        SubtitleKey = "Section.windows-pe.Subtitle",
        IntroMarkdownPath = "content/sections/windows-pe.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "mode",
                LabelKey = "Pe.Mode.Label",
                DescriptionKey = "Pe.Mode.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new("Default", "Pe.Mode.Default"),
                    new("Generated", "Pe.Mode.Generated"),
                    new("Custom", "Pe.Mode.Custom"),
                ],
                GetString = c => c.Pe.Mode.ToString(),
                SetString = (c, v) => c.Pe.Mode = Enum.TryParse<PeMode>(v, out var m) ? m : PeMode.Default,
            },
            new OptionDefinition
            {
                Id = "disable-8dot3",
                LabelKey = "Pe.Disable8Dot3.Label",
                DescriptionKey = "Pe.Disable8Dot3.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Pe.Disable8Dot3Names,
                SetBool = (c, v) => c.Pe.Disable8Dot3Names = v,
            },
            new OptionDefinition
            {
                Id = "pause-before-partition",
                LabelKey = "Pe.PauseBeforePartition.Label",
                DescriptionKey = "Pe.PauseBeforePartition.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Pe.PauseBeforePartition,
                SetBool = (c, v) => c.Pe.PauseBeforePartition = v,
            },
            new OptionDefinition
            {
                Id = "pause-before-reboot",
                LabelKey = "Pe.PauseBeforeReboot.Label",
                DescriptionKey = "Pe.PauseBeforeReboot.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Pe.PauseBeforeReboot,
                SetBool = (c, v) => c.Pe.PauseBeforeReboot = v,
            },
            new OptionDefinition
            {
                Id = "custom-cmd",
                LabelKey = "Pe.CustomCmd.Label",
                DescriptionKey = "Pe.CustomCmd.Description",
                Kind = OptionKind.Textarea,
                Language = "batch",
                Rows = 12,
                PlaceholderKey = "Pe.CustomCmd.Placeholder",
                GetString = c => c.Pe.CustomCmd,
                SetString = (c, v) => c.Pe.CustomCmd = string.IsNullOrWhiteSpace(v) ? null : v,
            },
        ],
    };
}

using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Compact OS mode selects whether Windows keeps its files in the compressed WIMBOOT layout
/// after installation. Default = follow Schneegans' heuristic, Enabled = always compress,
/// Disabled = never compress.
/// </summary>
public static class CompactOsSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "compact-os",
        TitleKey = "Advanced.Section.compact-os",
        SubtitleKey = "Section.compact-os.Subtitle",
        IntroMarkdownPath = "content/sections/compact-os.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "mode",
                LabelKey = "CompactOs.Mode.Label",
                DescriptionKey = "CompactOs.Mode.Description",
                ShowDescriptionInAdvanced = true,
                LearnMoreUrl = "https://learn.microsoft.com/windows-hardware/manufacture/desktop/compact-os",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new OptionValue("Default", "CompactOs.Mode.Default"),
                    new OptionValue("Enabled", "CompactOs.Mode.Enabled"),
                    new OptionValue("Disabled", "CompactOs.Mode.Disabled"),
                ],
                GetString = c => c.CompactOs.Mode.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<CompactOsMode>(v, out var m))
                        c.CompactOs.Mode = m;
                },
            },
        ],
    };
}

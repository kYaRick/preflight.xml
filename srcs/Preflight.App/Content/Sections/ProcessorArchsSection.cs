using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Which processor architectures the generated autounattend.xml targets. Exposed as a single
/// CheckboxGroup so the user can pick any non-empty subset of x86 / amd64 / arm64.
/// </summary>
public static class ProcessorArchsSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "processor-archs",
        TitleKey = "Advanced.Section.processor-archs",
        SubtitleKey = "Section.processor-archs.Subtitle",
        IntroMarkdownPath = "content/sections/processor-archs.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "architectures",
                LabelKey = "ProcessorArchs.Architectures.Label",
                DescriptionKey = "ProcessorArchs.Architectures.Description",
                ShowDescriptionInAdvanced = true,
                LearnMoreUrl = "/docs/processor-archs",
                Kind = OptionKind.CheckboxGroup,
                CheckboxItems =
                [
                    new CheckboxItem(
                        "x86",
                        "ProcessorArchs.X86.Label",
                        c => c.ProcessorArchs.X86,
                        (c, v) => c.ProcessorArchs.X86 = v),
                    new CheckboxItem(
                        "amd64",
                        "ProcessorArchs.Amd64.Label",
                        c => c.ProcessorArchs.Amd64,
                        (c, v) => c.ProcessorArchs.Amd64 = v),
                    new CheckboxItem(
                        "arm64",
                        "ProcessorArchs.Arm64.Label",
                        c => c.ProcessorArchs.Arm64,
                        (c, v) => c.ProcessorArchs.Arm64 = v),
                ],
            },
        ],
    };
}

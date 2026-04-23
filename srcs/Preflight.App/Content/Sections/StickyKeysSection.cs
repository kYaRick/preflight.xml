using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Section metadata for the Accessibility → Sticky Keys configuration. The actual form
/// renders via <see cref="Preflight.App.Content.Sections.StickyKeysForm"/> because the six
/// flag checkboxes only apply in the "Configure" mode.
/// </summary>
public static class StickyKeysSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "sticky-keys",
        TitleKey = "Advanced.Section.sticky-keys",
        SubtitleKey = "Section.sticky-keys.Subtitle",
        IntroMarkdownPath = "content/sections/sticky-keys.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "mode",
                LabelKey = "StickyKeys.Mode.Label",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new OptionValue("Default", "StickyKeys.Mode.Default"),
                    new OptionValue("Disable", "StickyKeys.Mode.Disable"),
                    new OptionValue("Configure", "StickyKeys.Mode.Configure"),
                ],
                GetString = c => c.StickyKeys.Mode.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<StickyKeysMode>(v, out var mode))
                        c.StickyKeys.Mode = mode;
                },
            },
        ],
    };
}

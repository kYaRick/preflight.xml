using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Section metadata for Caps-Lock / Num-Lock / Scroll-Lock configuration. The actual form
/// renders via <see cref="Preflight.App.Content.Sections.LockKeysForm"/> because the visible
/// controls are conditional on the selected mode - too much dynamic shape for the plain
/// data-driven SectionView.
/// </summary>
public static class LockKeysSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "lock-keys",
        TitleKey = "Advanced.Section.lock-keys",
        SubtitleKey = "Section.lock-keys.Subtitle",
        IntroMarkdownPath = "content/sections/lock-keys.{locale}.md",
        // Even though the form is custom, we keep an Options list so wizard steps /
        // future export tooling can still enumerate the fields.
        Options =
        [
            new OptionDefinition
            {
                Id = "mode",
                LabelKey = "LockKeys.Mode.Label",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new OptionValue("Default", "LockKeys.Mode.Default"),
                    new OptionValue("Configure", "LockKeys.Mode.Configure"),
                ],
                GetString = c => c.LockKeys.Mode.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<LockKeysMode>(v, out var mode))
                        c.LockKeys.Mode = mode;
                },
            },
        ],
    };
}

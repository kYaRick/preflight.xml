using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// OOBE "Express settings" mode - the screen where Windows asks to enable several privacy
/// knobs in one click. Schneegans supports three outcomes: show the screen (Interactive),
/// preselect "Accept" (EnableAll), preselect "Customize / No" (DisableAll).
/// Uses the same <see cref="Preflight.App.Models.ExpressSettingsMode"/> already stored on
/// <see cref="Preflight.App.Models.PrivacySettings.ExpressSettings"/>.
/// </summary>
public static class ExpressSettingsSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "express-settings",
        TitleKey = "Advanced.Section.express-settings",
        SubtitleKey = "Section.express-settings.Subtitle",
        IntroMarkdownPath = "content/sections/express-settings.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "mode",
                LabelKey = "ExpressSettings.Mode.Label",
                DescriptionKey = "ExpressSettings.Mode.Description",
                ShowDescriptionInAdvanced = true,
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new OptionValue("Interactive", "ExpressSettings.Mode.Interactive"),
                    new OptionValue("EnableAll", "ExpressSettings.Mode.EnableAll"),
                    new OptionValue("DisableAll", "ExpressSettings.Mode.DisableAll"),
                ],
                GetString = c => c.Privacy.ExpressSettings.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<ExpressSettingsMode>(v, out var mode))
                        c.Privacy.ExpressSettings = mode;
                },
            },
        ],
    };
}

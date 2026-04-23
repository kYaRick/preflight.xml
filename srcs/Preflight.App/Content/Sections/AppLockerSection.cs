using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// AppLocker - either skip entirely (default) or paste a custom policy XML that
/// Schneegans will validate against its embedded <c>AppLocker.xsd</c>.
///
/// The textarea is always rendered; the adapter only forwards it to Schneegans when
/// Mode=CustomXml and the content is non-empty, so an invalid paste in Mode=NotConfigured
/// doesn't blow up the preview.
/// </summary>
public static class AppLockerSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "applocker",
        TitleKey = "Advanced.Section.applocker",
        SubtitleKey = "Section.applocker.Subtitle",
        IntroMarkdownPath = "content/sections/applocker.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "mode",
                LabelKey = "AppLocker.Mode.Label",
                DescriptionKey = "AppLocker.Mode.Description",
                ShowDescriptionInAdvanced = true,
                LearnMoreUrl = "https://learn.microsoft.com/windows/security/application-security/application-control/windows-defender-application-control/applocker/applocker-overview",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new(nameof(AppLockerMode.NotConfigured), "AppLocker.Mode.NotConfigured"),
                    new(nameof(AppLockerMode.CustomXml), "AppLocker.Mode.CustomXml"),
                ],
                GetString = c => c.AppLocker.Mode.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<AppLockerMode>(v, out var m)) c.AppLocker.Mode = m;
                },
            },
            new OptionDefinition
            {
                Id = "policy-xml",
                LabelKey = "AppLocker.PolicyXml.Label",
                DescriptionKey = "AppLocker.PolicyXml.Description",
                ShowDescriptionInAdvanced = true,
                Kind = OptionKind.Textarea,
                TextareaRows = 12,
                Monospace = true,
                Placeholder = "<AppLockerPolicy Version=\"1\"> ... </AppLockerPolicy>",
                GetString = c => c.AppLocker.PolicyXml,
                SetString = (c, v) => c.AppLocker.PolicyXml = v,
            },
        ],
    };
}

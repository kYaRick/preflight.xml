using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Region & language - the first vertical slice. Defines which options exist, where their
/// values come from, and how they're read/written on <see cref="UnattendConfig"/>.
/// Rendered identically by <c>/docs/region</c> (read-only) and <c>/advanced/region</c> (editable);
/// wizard steps pick subsets of <see cref="Definition.Options"/>.
/// </summary>
public static class RegionSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "region",
        TitleKey = "Advanced.Section.region",
        SubtitleKey = "Section.region.Subtitle",
        IntroMarkdownPath = "content/sections/region.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "display-lang",
                LabelKey = "Region.DisplayLanguage.Label",
                DescriptionKey = "Region.DisplayLanguage.Description",
                LearnMoreUrl = "/docs/region",
                Kind = OptionKind.Dropdown,
                JsonSource = "data/locales.json",
                GetString = c => c.Region.DisplayLanguage,
                SetString = (c, v) => c.Region.DisplayLanguage = v ?? "en-US",
            },
            new OptionDefinition
            {
                Id = "input-lang",
                LabelKey = "Region.InputLanguage.Label",
                DescriptionKey = "Region.InputLanguage.Description",
                LearnMoreUrl = "/docs/region",
                Kind = OptionKind.Dropdown,
                JsonSource = "data/locales.json",
                GetString = c => c.Region.InputLanguage,
                SetString = (c, v) => c.Region.InputLanguage = v ?? "en-US",
            },
            new OptionDefinition
            {
                Id = "home-location",
                LabelKey = "Region.HomeLocation.Label",
                DescriptionKey = "Region.HomeLocation.Description",
                LearnMoreUrl = "/docs/region",
                Kind = OptionKind.Dropdown,
                JsonSource = "data/geo-ids.json",
                GetString = c => c.Region.HomeLocation,
                SetString = (c, v) => c.Region.HomeLocation = v ?? "US",
            },
        ],
    };
}

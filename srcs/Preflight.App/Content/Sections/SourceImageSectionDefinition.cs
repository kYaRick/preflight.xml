using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Static metadata for the <c>source-image</c> section (which install.wim /
/// index / name Setup picks). Live layout is <see cref="SourceImageSection"/>.
/// </summary>
public static class SourceImageSectionDefinition
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "source-image",
        TitleKey = "Advanced.Section.source-image",
        SubtitleKey = "Section.source-image.Subtitle",
        IntroMarkdownPath = "content/sections/source-image.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "mode",
                LabelKey = "SourceImage.Mode.Label",
                DescriptionKey = "SourceImage.Mode.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new("Automatic", "SourceImage.Mode.Automatic"),
                    new("ByIndex", "SourceImage.Mode.ByIndex"),
                    new("ByName", "SourceImage.Mode.ByName"),
                ],
                GetString = c => c.SourceImage.Mode.ToString(),
                SetString = (c, v) => c.SourceImage.Mode = Enum.TryParse<SourceImageMode>(v, out var m) ? m : SourceImageMode.Automatic,
            },
            new OptionDefinition
            {
                Id = "image-index",
                LabelKey = "SourceImage.ImageIndex.Label",
                DescriptionKey = "SourceImage.ImageIndex.Description",
                Kind = OptionKind.Number,
                Min = 1,
                Max = 64,
                GetInt = c => c.SourceImage.ImageIndex,
                SetInt = (c, v) => c.SourceImage.ImageIndex = v,
            },
            new OptionDefinition
            {
                Id = "image-name",
                LabelKey = "SourceImage.ImageName.Label",
                DescriptionKey = "SourceImage.ImageName.Description",
                Kind = OptionKind.Text,
                PlaceholderKey = "SourceImage.ImageName.Placeholder",
                GetString = c => c.SourceImage.ImageName,
                SetString = (c, v) => c.SourceImage.ImageName = string.IsNullOrWhiteSpace(v) ? null : v,
            },
        ],
    };
}

using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Start-menu folder visibility (Windows 11 feature). Pair of states just like desktop icons:
/// Default defers to Windows; Specific pins the set in <see cref="StartFolderSettings.VisibleFolders"/>.
/// The checkbox grid lives in <c>StartFoldersAdvanced.razor</c>, populated from
/// <c>data/start-folders.json</c>.
/// </summary>
public static class StartFoldersSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "folders-start",
        TitleKey = "Advanced.Section.folders-start",
        SubtitleKey = "Section.folders-start.Subtitle",
        IntroMarkdownPath = "content/sections/folders-start.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "mode",
                LabelKey = "StartFolders.Mode.Label",
                DescriptionKey = "StartFolders.Mode.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new OptionValue(nameof(StartFolderMode.Default),  "StartFolders.Mode.Default"),
                    new OptionValue(nameof(StartFolderMode.Specific), "StartFolders.Mode.Specific"),
                ],
                GetString = c => c.StartFolders.Mode.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<StartFolderMode>(v, out var parsed))
                        c.StartFolders.Mode = parsed;
                },
            },
        ],
    };
}

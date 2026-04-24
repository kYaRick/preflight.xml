using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// File Explorer tweaks - the data-driven subset of the shell cluster. Mirrors the
/// handful of <see cref="ExplorerSettings"/> flags that map straight onto schneegans
/// <c>Configuration</c> booleans plus the <c>HideModes</c> tri-state.
/// </summary>
public static class ExplorerSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "explorer",
        TitleKey = "Advanced.Section.explorer",
        SubtitleKey = "Section.explorer.Subtitle",
        IntroMarkdownPath = "content/sections/explorer.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "hide-files",
                LabelKey = "Explorer.HideFiles.Label",
                DescriptionKey = "Explorer.HideFiles.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new OptionValue(nameof(ExplorerHideFiles.Hidden), "Explorer.HideFiles.Hidden"),
                    new OptionValue(nameof(ExplorerHideFiles.OsOnly), "Explorer.HideFiles.OsOnly"),
                    new OptionValue(nameof(ExplorerHideFiles.ShowAll), "Explorer.HideFiles.ShowAll"),
                ],
                GetString = c => c.Explorer.HideFiles.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<ExplorerHideFiles>(v, out var parsed))
                        c.Explorer.HideFiles = parsed;
                },
            },
            new OptionDefinition
            {
                Id = "classic-context-menu",
                LabelKey = "Explorer.ClassicContextMenu.Label",
                DescriptionKey = "Explorer.ClassicContextMenu.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Explorer.ClassicContextMenu,
                SetBool = (c, v) => c.Explorer.ClassicContextMenu = v,
            },
            new OptionDefinition
            {
                Id = "show-file-extensions",
                LabelKey = "Explorer.ShowFileExtensions.Label",
                DescriptionKey = "Explorer.ShowFileExtensions.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Explorer.ShowFileExtensions,
                SetBool = (c, v) => c.Explorer.ShowFileExtensions = v,
            },
            new OptionDefinition
            {
                Id = "hide-info-tip",
                LabelKey = "Explorer.HideInfoTip.Label",
                DescriptionKey = "Explorer.HideInfoTip.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Explorer.HideInfoTip,
                SetBool = (c, v) => c.Explorer.HideInfoTip = v,
            },
            new OptionDefinition
            {
                Id = "launch-to-this-pc",
                LabelKey = "Explorer.LaunchToThisPC.Label",
                DescriptionKey = "Explorer.LaunchToThisPC.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Explorer.LaunchToThisPC,
                SetBool = (c, v) => c.Explorer.LaunchToThisPC = v,
            },
            new OptionDefinition
            {
                Id = "show-end-task",
                LabelKey = "Explorer.ShowEndTask.Label",
                DescriptionKey = "Explorer.ShowEndTask.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Explorer.ShowEndTask,
                SetBool = (c, v) => c.Explorer.ShowEndTask = v,
            },
        ],
    };
}

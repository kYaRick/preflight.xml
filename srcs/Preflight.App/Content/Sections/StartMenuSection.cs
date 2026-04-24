using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Start menu + taskbar controls. Most options map 1:1 onto schneegans booleans, but three -
/// TaskbarIcons / StartTiles / StartPins - use an interface with Default / RemoveAll / Custom*
/// variants. Those three are rendered by the companion <c>StartMenuAdvanced.razor</c> component
/// (radio + conditional textarea reveal); everything else is plain enough for the data-driven
/// <see cref="SectionView"/> path.
/// </summary>
public static class StartMenuSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "start-menu",
        TitleKey = "Advanced.Section.start-menu",
        SubtitleKey = "Section.start-menu.Subtitle",
        IntroMarkdownPath = "content/sections/start-menu.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "taskbar-search",
                LabelKey = "StartMenu.TaskbarSearch.Label",
                DescriptionKey = "StartMenu.TaskbarSearch.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new OptionValue(nameof(TaskbarSearchMode.Hide),  "StartMenu.TaskbarSearch.Hide")
                        { ImagePath = "content/images/taskbar-search-hide.png" },
                    new OptionValue(nameof(TaskbarSearchMode.Icon),  "StartMenu.TaskbarSearch.Icon")
                        { ImagePath = "content/images/taskbar-search-icon.png" },
                    new OptionValue(nameof(TaskbarSearchMode.Box),   "StartMenu.TaskbarSearch.Box")
                        { ImagePath = "content/images/taskbar-search-box.png" },
                    new OptionValue(nameof(TaskbarSearchMode.Label), "StartMenu.TaskbarSearch.Label_")
                        { ImagePath = "content/images/taskbar-search-label.png" },
                ],
                GetString = c => c.StartMenu.TaskbarSearch.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<TaskbarSearchMode>(v, out var parsed))
                        c.StartMenu.TaskbarSearch = parsed;
                },
            },
            new OptionDefinition
            {
                Id = "taskbar-icons",
                LabelKey = "StartMenu.TaskbarIcons.Label",
                DescriptionKey = "StartMenu.TaskbarIcons.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new OptionValue(nameof(TaskbarIconsMode.Default),   "StartMenu.TaskbarIcons.Default"),
                    new OptionValue(nameof(TaskbarIconsMode.RemoveAll), "StartMenu.TaskbarIcons.RemoveAll"),
                    new OptionValue(nameof(TaskbarIconsMode.CustomXml), "StartMenu.TaskbarIcons.CustomXml"),
                ],
                GetString = c => c.StartMenu.TaskbarIcons.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<TaskbarIconsMode>(v, out var parsed))
                        c.StartMenu.TaskbarIcons = parsed;
                },
            },
            new OptionDefinition
            {
                Id = "taskbar-icons-xml",
                LabelKey = "StartMenu.TaskbarIconsXml.Label",
                DescriptionKey = "StartMenu.TaskbarIconsXml.Description",
                Kind = OptionKind.Textarea,
                Language = "xml",
                TextareaRows = 10,
                GetString = c => c.StartMenu.TaskbarIconsXml,
                SetString = (c, v) => c.StartMenu.TaskbarIconsXml = v,
            },
            new OptionDefinition
            {
                Id = "disable-widgets",
                LabelKey = "StartMenu.DisableWidgets.Label",
                DescriptionKey = "StartMenu.DisableWidgets.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.StartMenu.DisableWidgets,
                SetBool = (c, v) => c.StartMenu.DisableWidgets = v,
            },
            new OptionDefinition
            {
                Id = "left-taskbar",
                LabelKey = "StartMenu.LeftTaskbar.Label",
                DescriptionKey = "StartMenu.LeftTaskbar.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.StartMenu.LeftTaskbar,
                SetBool = (c, v) => c.StartMenu.LeftTaskbar = v,
            },
            new OptionDefinition
            {
                Id = "hide-task-view-button",
                LabelKey = "StartMenu.HideTaskViewButton.Label",
                DescriptionKey = "StartMenu.HideTaskViewButton.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.StartMenu.HideTaskViewButton,
                SetBool = (c, v) => c.StartMenu.HideTaskViewButton = v,
            },
            new OptionDefinition
            {
                Id = "show-all-tray-icons",
                LabelKey = "StartMenu.ShowAllTrayIcons.Label",
                DescriptionKey = "StartMenu.ShowAllTrayIcons.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.StartMenu.ShowAllTrayIcons,
                SetBool = (c, v) => c.StartMenu.ShowAllTrayIcons = v,
            },
            new OptionDefinition
            {
                Id = "disable-bing-results",
                LabelKey = "StartMenu.DisableBingResults.Label",
                DescriptionKey = "StartMenu.DisableBingResults.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.StartMenu.DisableBingResults,
                SetBool = (c, v) => c.StartMenu.DisableBingResults = v,
            },
            new OptionDefinition
            {
                Id = "start-tiles",
                LabelKey = "StartMenu.StartTiles.Label",
                DescriptionKey = "StartMenu.StartTiles.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new OptionValue(nameof(StartTilesMode.Default),   "StartMenu.StartTiles.Default"),
                    new OptionValue(nameof(StartTilesMode.RemoveAll), "StartMenu.StartTiles.RemoveAll"),
                    new OptionValue(nameof(StartTilesMode.CustomXml), "StartMenu.StartTiles.CustomXml"),
                ],
                GetString = c => c.StartMenu.StartTiles.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<StartTilesMode>(v, out var parsed))
                        c.StartMenu.StartTiles = parsed;
                },
            },
            new OptionDefinition
            {
                Id = "start-tiles-xml",
                LabelKey = "StartMenu.StartTilesXml.Label",
                DescriptionKey = "StartMenu.StartTilesXml.Description",
                Kind = OptionKind.Textarea,
                Language = "xml",
                TextareaRows = 10,
                GetString = c => c.StartMenu.StartTilesXml,
                SetString = (c, v) => c.StartMenu.StartTilesXml = v,
            },
            new OptionDefinition
            {
                Id = "start-pins",
                LabelKey = "StartMenu.StartPins.Label",
                DescriptionKey = "StartMenu.StartPins.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new OptionValue(nameof(StartPinsMode.Default),    "StartMenu.StartPins.Default"),
                    new OptionValue(nameof(StartPinsMode.RemoveAll),  "StartMenu.StartPins.RemoveAll"),
                    new OptionValue(nameof(StartPinsMode.CustomJson), "StartMenu.StartPins.CustomJson"),
                ],
                GetString = c => c.StartMenu.StartPins.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<StartPinsMode>(v, out var parsed))
                        c.StartMenu.StartPins = parsed;
                },
            },
            new OptionDefinition
            {
                Id = "start-pins-json",
                LabelKey = "StartMenu.StartPinsJson.Label",
                DescriptionKey = "StartMenu.StartPinsJson.Description",
                Kind = OptionKind.Textarea,
                Language = "json",
                TextareaRows = 10,
                GetString = c => c.StartMenu.StartPinsJson,
                SetString = (c, v) => c.StartMenu.StartPinsJson = v,
            },
        ],
    };
}

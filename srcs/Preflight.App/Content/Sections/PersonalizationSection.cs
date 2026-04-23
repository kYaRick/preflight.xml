using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Personalization has three logically independent sub-sections (Colors, Wallpaper, LockScreen),
/// each with its own mode-switched reveal. Rather than cram all three into one flat
/// <see cref="SectionDefinition"/>, the <c>.razor</c> companion mounts three separate
/// <see cref="Layout.SectionView"/> instances against the three definitions exposed here.
/// </summary>
public static class PersonalizationSection
{
    public static readonly SectionDefinition Colors = new()
    {
        Id = "personalization-colors",
        TitleKey = "Personalization.Colors.Title",
        SubtitleKey = "Personalization.Colors.Subtitle",
        IntroMarkdownPath = "content/sections/personalization-colors.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "color-mode",
                LabelKey = "Personalization.Colors.Mode.Label",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new(nameof(ColorMode.Default), "Personalization.Colors.Mode.Default"),
                    new(nameof(ColorMode.Custom),  "Personalization.Colors.Mode.Custom"),
                ],
                GetString = c => c.Personalization.Colors.Mode.ToString(),
                SetString = (c, v) => c.Personalization.Colors.Mode = Enum.TryParse<ColorMode>(v, out var m) ? m : ColorMode.Default,
            },
            new OptionDefinition
            {
                Id = "taskbar-start-theme",
                LabelKey = "Personalization.Colors.TaskbarAndStartTheme.Label",
                Kind = OptionKind.Radio,
                VisibleWhen = c => c.Personalization.Colors.Mode == ColorMode.Custom,
                InlineValues =
                [
                    new(nameof(Preflight.App.Models.ColorTheme.Dark),  "Personalization.Colors.Theme.Dark"),
                    new(nameof(Preflight.App.Models.ColorTheme.Light), "Personalization.Colors.Theme.Light"),
                ],
                GetString = c => c.Personalization.Colors.TaskbarAndStartTheme.ToString(),
                SetString = (c, v) => c.Personalization.Colors.TaskbarAndStartTheme = Enum.TryParse<Preflight.App.Models.ColorTheme>(v, out var t) ? t : Preflight.App.Models.ColorTheme.Dark,
            },
            new OptionDefinition
            {
                Id = "apps-theme",
                LabelKey = "Personalization.Colors.AppsTheme.Label",
                Kind = OptionKind.Radio,
                VisibleWhen = c => c.Personalization.Colors.Mode == ColorMode.Custom,
                InlineValues =
                [
                    new(nameof(Preflight.App.Models.ColorTheme.Dark),  "Personalization.Colors.Theme.Dark"),
                    new(nameof(Preflight.App.Models.ColorTheme.Light), "Personalization.Colors.Theme.Light"),
                ],
                GetString = c => c.Personalization.Colors.AppsTheme.ToString(),
                SetString = (c, v) => c.Personalization.Colors.AppsTheme = Enum.TryParse<Preflight.App.Models.ColorTheme>(v, out var t) ? t : Preflight.App.Models.ColorTheme.Dark,
            },
            new OptionDefinition
            {
                Id = "accent-color",
                LabelKey = "Personalization.Colors.AccentColor.Label",
                DescriptionKey = "Personalization.Colors.AccentColor.Description",
                ShowDescriptionInAdvanced = true,
                Kind = OptionKind.Text,
                VisibleWhen = c => c.Personalization.Colors.Mode == ColorMode.Custom,
                GetString = c => c.Personalization.Colors.AccentColor,
                SetString = (c, v) => c.Personalization.Colors.AccentColor = v ?? "#0078D4",
            },
            new OptionDefinition
            {
                Id = "accent-on-start",
                LabelKey = "Personalization.Colors.AccentOnStartAndTaskbar.Label",
                Kind = OptionKind.Checkbox,
                VisibleWhen = c => c.Personalization.Colors.Mode == ColorMode.Custom,
                GetBool = c => c.Personalization.Colors.AccentOnStartAndTaskbar,
                SetBool = (c, v) => c.Personalization.Colors.AccentOnStartAndTaskbar = v,
            },
            new OptionDefinition
            {
                Id = "accent-on-titlebars",
                LabelKey = "Personalization.Colors.AccentOnTitleBars.Label",
                Kind = OptionKind.Checkbox,
                VisibleWhen = c => c.Personalization.Colors.Mode == ColorMode.Custom,
                GetBool = c => c.Personalization.Colors.AccentOnTitleBars,
                SetBool = (c, v) => c.Personalization.Colors.AccentOnTitleBars = v,
            },
            new OptionDefinition
            {
                Id = "translucent",
                LabelKey = "Personalization.Colors.Translucent.Label",
                Kind = OptionKind.Checkbox,
                VisibleWhen = c => c.Personalization.Colors.Mode == ColorMode.Custom,
                GetBool = c => c.Personalization.Colors.Translucent,
                SetBool = (c, v) => c.Personalization.Colors.Translucent = v,
            },
        ],
    };

    public static readonly SectionDefinition Wallpaper = new()
    {
        Id = "personalization-wallpaper",
        TitleKey = "Personalization.Wallpaper.Title",
        SubtitleKey = "Personalization.Wallpaper.Subtitle",
        IntroMarkdownPath = "content/sections/personalization-wallpaper.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "wallpaper-mode",
                LabelKey = "Personalization.Wallpaper.Mode.Label",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new(nameof(WallpaperMode.Default),    "Personalization.Wallpaper.Mode.Default"),
                    new(nameof(WallpaperMode.SolidColor), "Personalization.Wallpaper.Mode.SolidColor"),
                    new(nameof(WallpaperMode.Script),     "Personalization.Wallpaper.Mode.Script"),
                ],
                GetString = c => c.Personalization.Wallpaper.Mode.ToString(),
                SetString = (c, v) => c.Personalization.Wallpaper.Mode = Enum.TryParse<WallpaperMode>(v, out var m) ? m : WallpaperMode.Default,
            },
            new OptionDefinition
            {
                Id = "wallpaper-solid",
                LabelKey = "Personalization.Wallpaper.SolidColor.Label",
                DescriptionKey = "Personalization.Wallpaper.SolidColor.Description",
                ShowDescriptionInAdvanced = true,
                Kind = OptionKind.Text,
                VisibleWhen = c => c.Personalization.Wallpaper.Mode == WallpaperMode.SolidColor,
                GetString = c => c.Personalization.Wallpaper.SolidColor,
                SetString = (c, v) => c.Personalization.Wallpaper.SolidColor = v ?? "#0078D4",
            },
            new OptionDefinition
            {
                Id = "wallpaper-script",
                LabelKey = "Personalization.Wallpaper.Script.Label",
                DescriptionKey = "Personalization.Wallpaper.Script.Description",
                ShowDescriptionInAdvanced = true,
                Kind = OptionKind.Textarea,
                VisibleWhen = c => c.Personalization.Wallpaper.Mode == WallpaperMode.Script,
                GetString = c => c.Personalization.Wallpaper.Script,
                SetString = (c, v) => c.Personalization.Wallpaper.Script = v,
            },
        ],
    };

    public static readonly SectionDefinition LockScreen = new()
    {
        Id = "personalization-lockscreen",
        TitleKey = "Personalization.LockScreen.Title",
        SubtitleKey = "Personalization.LockScreen.Subtitle",
        IntroMarkdownPath = "content/sections/personalization-lockscreen.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "lockscreen-mode",
                LabelKey = "Personalization.LockScreen.Mode.Label",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new(nameof(LockScreenMode.Default), "Personalization.LockScreen.Mode.Default"),
                    new(nameof(LockScreenMode.Script),  "Personalization.LockScreen.Mode.Script"),
                ],
                GetString = c => c.Personalization.LockScreen.Mode.ToString(),
                SetString = (c, v) => c.Personalization.LockScreen.Mode = Enum.TryParse<LockScreenMode>(v, out var m) ? m : LockScreenMode.Default,
            },
            new OptionDefinition
            {
                Id = "lockscreen-script",
                LabelKey = "Personalization.LockScreen.Script.Label",
                DescriptionKey = "Personalization.LockScreen.Script.Description",
                ShowDescriptionInAdvanced = true,
                Kind = OptionKind.Textarea,
                VisibleWhen = c => c.Personalization.LockScreen.Mode == LockScreenMode.Script,
                GetString = c => c.Personalization.LockScreen.Script,
                SetString = (c, v) => c.Personalization.LockScreen.Script = v,
            },
        ],
    };
}

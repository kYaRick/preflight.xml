using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// System tweaks - the long list of bool flags exposed by schneegans' Configuration record.
/// Organised via <see cref="OptionDefinition.GroupHeadingKey"/> so the ~30 checkboxes read as
/// three shorter lists (explorer + shell, performance + system, Edge).
/// </summary>
public static class SystemTweaksSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "tweaks",
        TitleKey = "Advanced.Section.tweaks",
        SubtitleKey = "Section.tweaks.Subtitle",
        IntroMarkdownPath = "content/sections/tweaks.{locale}.md",
        Options =
        [
            // ─── Explorer & shell ─────────────────────────────────
            Checkbox("show-file-extensions", "Tweaks.ShowFileExtensions.Label",
                c => c.Tweaks.ShowFileExtensions, (c, v) => c.Tweaks.ShowFileExtensions = v,
                group: "Tweaks.Group.Shell"),
            Checkbox("show-all-tray-icons", "Tweaks.ShowAllTrayIcons.Label",
                c => c.Tweaks.ShowAllTrayIcons, (c, v) => c.Tweaks.ShowAllTrayIcons = v,
                group: "Tweaks.Group.Shell"),
            Checkbox("hide-task-view-button", "Tweaks.HideTaskViewButton.Label",
                c => c.Tweaks.HideTaskViewButton, (c, v) => c.Tweaks.HideTaskViewButton = v,
                group: "Tweaks.Group.Shell"),
            Checkbox("classic-context-menu", "Tweaks.ClassicContextMenu.Label",
                c => c.Tweaks.ClassicContextMenu, (c, v) => c.Tweaks.ClassicContextMenu = v,
                group: "Tweaks.Group.Shell"),
            Checkbox("left-taskbar", "Tweaks.LeftTaskbar.Label",
                c => c.Tweaks.LeftTaskbar, (c, v) => c.Tweaks.LeftTaskbar = v,
                group: "Tweaks.Group.Shell"),
            Checkbox("launch-to-this-pc", "Tweaks.LaunchToThisPC.Label",
                c => c.Tweaks.LaunchToThisPC, (c, v) => c.Tweaks.LaunchToThisPC = v,
                group: "Tweaks.Group.Shell"),
            Checkbox("show-end-task", "Tweaks.ShowEndTask.Label",
                c => c.Tweaks.ShowEndTask, (c, v) => c.Tweaks.ShowEndTask = v,
                group: "Tweaks.Group.Shell"),
            Checkbox("hide-info-tip", "Tweaks.HideInfoTip.Label",
                c => c.Tweaks.HideInfoTip, (c, v) => c.Tweaks.HideInfoTip = v,
                group: "Tweaks.Group.Shell"),

            // ─── System & performance ─────────────────────────────
            Checkbox("enable-long-paths", "Tweaks.EnableLongPaths.Label",
                c => c.Tweaks.EnableLongPaths, (c, v) => c.Tweaks.EnableLongPaths = v,
                group: "Tweaks.Group.System"),
            Checkbox("harden-system-drive-acl", "Tweaks.HardenSystemDriveAcl.Label",
                c => c.Tweaks.HardenSystemDriveAcl, (c, v) => c.Tweaks.HardenSystemDriveAcl = v,
                group: "Tweaks.Group.System"),
            Checkbox("delete-junctions", "Tweaks.DeleteJunctions.Label",
                c => c.Tweaks.DeleteJunctions, (c, v) => c.Tweaks.DeleteJunctions = v,
                group: "Tweaks.Group.System"),
            Checkbox("allow-powershell-scripts", "Tweaks.AllowPowerShellScripts.Label",
                c => c.Tweaks.AllowPowerShellScripts, (c, v) => c.Tweaks.AllowPowerShellScripts = v,
                group: "Tweaks.Group.System"),
            Checkbox("disable-last-access", "Tweaks.DisableLastAccess.Label",
                c => c.Tweaks.DisableLastAccess, (c, v) => c.Tweaks.DisableLastAccess = v,
                group: "Tweaks.Group.System"),
            Checkbox("prevent-automatic-reboot", "Tweaks.PreventAutomaticReboot.Label",
                c => c.Tweaks.PreventAutomaticReboot, (c, v) => c.Tweaks.PreventAutomaticReboot = v,
                group: "Tweaks.Group.System"),
            Checkbox("disable-fast-startup", "Tweaks.DisableFastStartup.Label",
                c => c.Tweaks.DisableFastStartup, (c, v) => c.Tweaks.DisableFastStartup = v,
                group: "Tweaks.Group.System"),
            Checkbox("disable-system-restore", "Tweaks.DisableSystemRestore.Label",
                c => c.Tweaks.DisableSystemRestore, (c, v) => c.Tweaks.DisableSystemRestore = v,
                group: "Tweaks.Group.System"),
            Checkbox("turn-off-system-sounds", "Tweaks.TurnOffSystemSounds.Label",
                c => c.Tweaks.TurnOffSystemSounds, (c, v) => c.Tweaks.TurnOffSystemSounds = v,
                group: "Tweaks.Group.System"),
            Checkbox("disable-app-suggestions", "Tweaks.DisableAppSuggestions.Label",
                c => c.Tweaks.DisableAppSuggestions, (c, v) => c.Tweaks.DisableAppSuggestions = v,
                group: "Tweaks.Group.System"),
            Checkbox("disable-widgets", "Tweaks.DisableWidgets.Label",
                c => c.Tweaks.DisableWidgets, (c, v) => c.Tweaks.DisableWidgets = v,
                group: "Tweaks.Group.System"),
            Checkbox("disable-windows-update", "Tweaks.DisableWindowsUpdate.Label",
                c => c.Tweaks.DisableWindowsUpdate, (c, v) => c.Tweaks.DisableWindowsUpdate = v,
                group: "Tweaks.Group.System"),
            Checkbox("disable-pointer-precision", "Tweaks.DisablePointerPrecision.Label",
                c => c.Tweaks.DisablePointerPrecision, (c, v) => c.Tweaks.DisablePointerPrecision = v,
                group: "Tweaks.Group.System"),
            Checkbox("delete-windows-old", "Tweaks.DeleteWindowsOld.Label",
                c => c.Tweaks.DeleteWindowsOld, (c, v) => c.Tweaks.DeleteWindowsOld = v,
                group: "Tweaks.Group.System"),
            Checkbox("disable-bing-results", "Tweaks.DisableBingResults.Label",
                c => c.Tweaks.DisableBingResults, (c, v) => c.Tweaks.DisableBingResults = v,
                group: "Tweaks.Group.System"),
            Checkbox("prevent-device-encryption", "Tweaks.PreventDeviceEncryption.Label",
                c => c.Tweaks.PreventDeviceEncryption, (c, v) => c.Tweaks.PreventDeviceEncryption = v,
                group: "Tweaks.Group.System"),
            Checkbox("disable-core-isolation", "Tweaks.DisableCoreIsolation.Label",
                c => c.Tweaks.DisableCoreIsolation, (c, v) => c.Tweaks.DisableCoreIsolation = v,
                group: "Tweaks.Group.System"),
            Checkbox("disable-automatic-restart-sign-on", "Tweaks.DisableAutomaticRestartSignOn.Label",
                c => c.Tweaks.DisableAutomaticRestartSignOn, (c, v) => c.Tweaks.DisableAutomaticRestartSignOn = v,
                group: "Tweaks.Group.System"),
            Checkbox("disable-wpbt", "Tweaks.DisableWpbt.Label",
                c => c.Tweaks.DisableWpbt, (c, v) => c.Tweaks.DisableWpbt = v,
                group: "Tweaks.Group.System"),

            // ─── Microsoft Edge ───────────────────────────────────
            Checkbox("hide-edge-fre", "Tweaks.HideEdgeFre.Label",
                c => c.Tweaks.HideEdgeFre, (c, v) => c.Tweaks.HideEdgeFre = v,
                group: "Tweaks.Group.Edge"),
            Checkbox("disable-edge-startup-boost", "Tweaks.DisableEdgeStartupBoost.Label",
                c => c.Tweaks.DisableEdgeStartupBoost, (c, v) => c.Tweaks.DisableEdgeStartupBoost = v,
                group: "Tweaks.Group.Edge"),
            Checkbox("make-edge-uninstallable", "Tweaks.MakeEdgeUninstallable.Label",
                c => c.Tweaks.MakeEdgeUninstallable, (c, v) => c.Tweaks.MakeEdgeUninstallable = v,
                group: "Tweaks.Group.Edge"),
            Checkbox("delete-edge-desktop-icon", "Tweaks.DeleteEdgeDesktopIcon.Label",
                c => c.Tweaks.DeleteEdgeDesktopIcon, (c, v) => c.Tweaks.DeleteEdgeDesktopIcon = v,
                group: "Tweaks.Group.Edge"),
        ],
    };

    private static OptionDefinition Checkbox(
        string id,
        string labelKey,
        Func<UnattendConfig, bool> getter,
        Action<UnattendConfig, bool> setter,
        string? group = null) => new()
        {
            Id = id,
            LabelKey = labelKey,
            Kind = OptionKind.Checkbox,
            GroupHeadingKey = group,
            GetBool = getter,
            SetBool = setter,
        };
}

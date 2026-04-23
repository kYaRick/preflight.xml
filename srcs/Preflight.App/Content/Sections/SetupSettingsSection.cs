using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Setup-time toggles that don't fit any single other section - Windows 11 requirement
/// bypass, OOBE network-check skip, PowerShell window visibility, and so on. Rendered as six
/// plain checkboxes bound to <see cref="UnattendConfig.Setup"/>.
/// </summary>
public static class SetupSettingsSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "setup-settings",
        TitleKey = "Advanced.Section.setup-settings",
        SubtitleKey = "Section.setup-settings.Subtitle",
        IntroMarkdownPath = "content/sections/setup-settings.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "bypass-requirements-check",
                LabelKey = "Setup.BypassRequirementsCheck.Label",
                DescriptionKey = "Setup.BypassRequirementsCheck.Description",
                ShowDescriptionInAdvanced = true,
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Setup.BypassRequirementsCheck,
                SetBool = (c, v) => c.Setup.BypassRequirementsCheck = v,
            },
            new OptionDefinition
            {
                Id = "bypass-network-check",
                LabelKey = "Setup.BypassNetworkCheck.Label",
                DescriptionKey = "Setup.BypassNetworkCheck.Description",
                ShowDescriptionInAdvanced = true,
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Setup.BypassNetworkCheck,
                SetBool = (c, v) => c.Setup.BypassNetworkCheck = v,
            },
            new OptionDefinition
            {
                Id = "use-configuration-set",
                LabelKey = "Setup.UseConfigurationSet.Label",
                DescriptionKey = "Setup.UseConfigurationSet.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Setup.UseConfigurationSet,
                SetBool = (c, v) => c.Setup.UseConfigurationSet = v,
            },
            new OptionDefinition
            {
                Id = "hide-powershell-windows",
                LabelKey = "Setup.HidePowerShellWindows.Label",
                DescriptionKey = "Setup.HidePowerShellWindows.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Setup.HidePowerShellWindows,
                SetBool = (c, v) => c.Setup.HidePowerShellWindows = v,
            },
            new OptionDefinition
            {
                Id = "keep-sensitive-files",
                LabelKey = "Setup.KeepSensitiveFiles.Label",
                DescriptionKey = "Setup.KeepSensitiveFiles.Description",
                ShowDescriptionInAdvanced = true,
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Setup.KeepSensitiveFiles,
                SetBool = (c, v) => c.Setup.KeepSensitiveFiles = v,
            },
            new OptionDefinition
            {
                Id = "use-narrator",
                LabelKey = "Setup.UseNarrator.Label",
                DescriptionKey = "Setup.UseNarrator.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.Setup.UseNarrator,
                SetBool = (c, v) => c.Setup.UseNarrator = v,
            },
        ],
    };
}

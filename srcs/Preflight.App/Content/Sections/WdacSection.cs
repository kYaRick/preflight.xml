using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Windows Defender Application Control. Three radio groups:
///  - Mode (NotConfigured / Basic)
///  - Enforcement (Audit / AuditOnBootFail / Enforce) - only meaningful when Mode=Basic
///  - ScriptEnforcement (Restricted / Unrestricted) - only meaningful when Mode=Basic
/// The two dependent controls are rendered unconditionally for simplicity; the adapter
/// ignores them unless Mode=Basic, and descriptions explain the relationship.
/// </summary>
public static class WdacSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "wdac",
        TitleKey = "Advanced.Section.wdac",
        SubtitleKey = "Section.wdac.Subtitle",
        IntroMarkdownPath = "content/sections/wdac.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "mode",
                LabelKey = "Wdac.Mode.Label",
                DescriptionKey = "Wdac.Mode.Description",
                ShowDescriptionInAdvanced = true,
                LearnMoreUrl = "https://learn.microsoft.com/windows/security/threat-protection/windows-defender-application-control/windows-defender-application-control",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new(nameof(WdacMode.NotConfigured), "Wdac.Mode.NotConfigured"),
                    new(nameof(WdacMode.Basic), "Wdac.Mode.Basic"),
                ],
                GetString = c => c.Wdac.Mode.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<WdacMode>(v, out var mode)) c.Wdac.Mode = mode;
                },
            },
            new OptionDefinition
            {
                Id = "enforcement",
                LabelKey = "Wdac.Enforcement.Label",
                DescriptionKey = "Wdac.Enforcement.Description",
                ShowDescriptionInAdvanced = true,
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new(nameof(WdacEnforcement.Audit), "Wdac.Enforcement.Audit"),
                    new(nameof(WdacEnforcement.AuditOnBootFail), "Wdac.Enforcement.AuditOnBootFail"),
                    new(nameof(WdacEnforcement.Enforce), "Wdac.Enforcement.Enforce"),
                ],
                GetString = c => c.Wdac.Enforcement.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<WdacEnforcement>(v, out var e)) c.Wdac.Enforcement = e;
                },
            },
            new OptionDefinition
            {
                Id = "script-enforcement",
                LabelKey = "Wdac.ScriptEnforcement.Label",
                DescriptionKey = "Wdac.ScriptEnforcement.Description",
                ShowDescriptionInAdvanced = true,
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new(nameof(WdacScriptEnforcement.Restricted), "Wdac.ScriptEnforcement.Restricted"),
                    new(nameof(WdacScriptEnforcement.Unrestricted), "Wdac.ScriptEnforcement.Unrestricted"),
                ],
                GetString = c => c.Wdac.ScriptEnforcement.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<WdacScriptEnforcement>(v, out var s)) c.Wdac.ScriptEnforcement = s;
                },
            },
        ],
    };
}

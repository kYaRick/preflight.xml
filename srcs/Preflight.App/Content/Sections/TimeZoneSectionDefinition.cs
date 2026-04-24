using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Time zone has a dependent Dropdown (visible only when Mode=Explicit). The live form lives in
/// <see cref="TimeZoneSection"/> (the sibling .razor component). The registry entry keeps the
/// section visible in the nav and links to the docs intro; AdvancedShell swaps in the custom
/// form on the Advanced surface.
/// </summary>
public static class TimeZoneSectionDefinition
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "time-zone",
        TitleKey = "Advanced.Section.time-zone",
        SubtitleKey = "Section.time-zone.Subtitle",
        IntroMarkdownPath = "content/sections/time-zone.{locale}.md",
        Options = [],
    };
}

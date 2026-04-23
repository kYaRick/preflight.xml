using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Computer name has a conditional shape (Random / Manual-with-text / Script-with-textarea) that
/// doesn't fit a single <see cref="OptionKind"/>; the live form lives in
/// <see cref="ComputerNameSection"/> (the sibling .razor component).
/// The registry still needs a <see cref="SectionDefinition"/> for the nav label, docs link, and
/// subtitle, but its <see cref="SectionDefinition.Options"/> stays empty so
/// <see cref="Layout.SectionView"/> renders just the header / intro while AdvancedShell swaps in
/// the custom form.
/// </summary>
public static class ComputerNameSectionDefinition
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "computer-name",
        TitleKey = "Advanced.Section.computer-name",
        SubtitleKey = "Section.computer-name.Subtitle",
        IntroMarkdownPath = "content/sections/computer-name.{locale}.md",
        Options = [],
    };
}

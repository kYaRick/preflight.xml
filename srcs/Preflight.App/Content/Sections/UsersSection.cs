using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Static registry-side metadata for the user-accounts section. The editable
/// surface is a hand-rolled Razor component (<c>UsersSection.razor</c>) because
/// the controls (dynamic account table + conditional sub-forms) don't fit the
/// one-control-per-option shape used by <see cref="OptionDefinition"/>.
///
/// This class sits next to the component rather than inside it so the compiler
/// doesn't try to merge a static class with the generated component partial.
/// </summary>
public static class UsersSectionDefinition
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "users",
        TitleKey = "Advanced.Section.users",
        SubtitleKey = "Section.users.Subtitle",
        IntroMarkdownPath = "content/sections/users.{locale}.md",
        Options = [],
    };
}

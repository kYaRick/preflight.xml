using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// User accounts section - owns the list of local accounts, first-logon mode,
/// password expiration, and account-lockout policy. Unlike simple sections
/// driven by <see cref="OptionDefinition"/> lists, the Advanced view for users
/// is a hand-rolled Razor component (<c>UsersSection.razor</c>) because it needs
/// a table-with-add/remove + several conditional sub-forms that don't fit the
/// one-control-per-option shape.
///
/// The <see cref="Definition"/> still exists so the section can be discovered by
/// <c>SectionRegistry</c> and rendered in read-only Docs mode; in Docs the empty
/// <see cref="SectionDefinition.Options"/> list means only the title + intro
/// markdown are shown.
/// </summary>
public static class UsersSection
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

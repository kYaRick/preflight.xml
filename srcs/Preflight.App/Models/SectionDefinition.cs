namespace Preflight.App.Models;

/// <summary>
/// The static metadata that describes a single <c>autounattend.xml</c> configuration section.
/// One <see cref="SectionDefinition"/> drives three renderings:
///   • <c>/docs/{id}</c> — read-only — title + intro + option descriptions.
///   • <c>/advanced/{id}</c> — editable — same content plus live controls bound to <see cref="UnattendConfig"/>.
///   • <c>/wizard/guided/{step}</c> — narrative — hand-crafted step that picks a subset of the options.
/// Keeping the definition centralized means adding a new option or changing an enum value only ever
/// touches one file; all three views pick up the change automatically.
/// </summary>
public sealed record SectionDefinition
{
    /// <summary>URL-safe id that matches the route segment: <c>region</c>, <c>disk</c>, etc.</summary>
    public required string Id { get; init; }

    /// <summary>Resource key for the section title — kept in <c>SharedResources.resx</c>.</summary>
    public required string TitleKey { get; init; }

    /// <summary>Resource key for the one-line subtitle shown under the title.</summary>
    public required string SubtitleKey { get; init; }

    /// <summary>
    /// Relative path template for the Markdown intro, e.g. <c>content/sections/region.{locale}.md</c>.
    /// The view swaps <c>{locale}</c> for <c>en</c> / <c>uk</c> at render time.
    /// </summary>
    public required string IntroMarkdownPath { get; init; }

    /// <summary>Ordered list of configurable options the section exposes.</summary>
    public required IReadOnlyList<OptionDefinition> Options { get; init; }
}

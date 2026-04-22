using Preflight.App.Models;

namespace Preflight.App.Content;

/// <summary>
/// Single source of truth for every <see cref="SectionDefinition"/> known to the app.
/// Routed pages, Docs, and the Advanced shell all resolve sections by id against this
/// registry rather than holding their own lookup tables - so adding a new section is
/// "implement + add one entry".
///
/// Sections whose Advanced view is a custom .razor component (e.g. Users) still have
/// a definition registered here so Docs mode and the nav menu can discover them; the
/// <see cref="SectionDefinition.Options"/> list is simply empty in that case.
/// </summary>
public static class SectionRegistry
{
    private static readonly Dictionary<string, SectionDefinition> _byId = new(StringComparer.Ordinal)
    {
        [Sections.RegionSection.Definition.Id] = Sections.RegionSection.Definition,
        [Sections.UsersSection.Definition.Id] = Sections.UsersSection.Definition,
    };

    public static IReadOnlyCollection<SectionDefinition> All => _byId.Values;

    public static SectionDefinition? Find(string? id) =>
        id is not null && _byId.TryGetValue(id, out var s) ? s : null;
}

using Preflight.App.Models;
using Preflight.App.Services;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Bloatware removal - a single <see cref="OptionKind.CheckboxGroup"/> backed by the 40+
/// entries from <see cref="BloatwareCatalog"/>. The catalog instance is injected by the
/// <c>.razor</c> companion (which is the only DI-aware surface here); it calls
/// <see cref="Build"/> once per section load to bake the <see cref="SectionDefinition"/>.
/// </summary>
public static class BloatwareSection
{
    public static SectionDefinition Build(BloatwareCatalog catalog) => new()
    {
        Id = "bloatware",
        TitleKey = "Advanced.Section.bloatware",
        SubtitleKey = "Section.bloatware.Subtitle",
        IntroMarkdownPath = "content/sections/bloatware.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "apps-to-remove",
                LabelKey = "Bloatware.RemoveCount.Label",
                Kind = OptionKind.CheckboxGroup,
                ItemsProvider = () => catalog.Items,
                IsItemSelected = (c, id) => c.Bloatware.AppsToRemove.Contains(id),
                SetItemSelected = (c, id, on) =>
                {
                    if (on) c.Bloatware.AppsToRemove.Add(id);
                    else    c.Bloatware.AppsToRemove.Remove(id);
                },
            },
        ],
    };
}

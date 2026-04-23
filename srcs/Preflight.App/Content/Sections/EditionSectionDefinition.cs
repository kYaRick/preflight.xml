using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Static metadata for the <c>edition</c> section. Drives the Docs view and provides
/// the individual option definitions that <see cref="EditionSection"/> pieces together
/// under conditional visibility.
/// </summary>
public static class EditionSectionDefinition
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "edition",
        TitleKey = "Advanced.Section.edition",
        SubtitleKey = "Section.edition.Subtitle",
        IntroMarkdownPath = "content/sections/edition.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "key-mode",
                LabelKey = "Edition.KeyMode.Label",
                DescriptionKey = "Edition.KeyMode.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new("Generic", "Edition.KeyMode.Generic"),
                    new("Custom", "Edition.KeyMode.Custom"),
                    new("Interactive", "Edition.KeyMode.Interactive"),
                    new("FromBios", "Edition.KeyMode.FromBios"),
                ],
                GetString = c => c.Edition.KeyMode.ToString(),
                SetString = (c, v) => c.Edition.KeyMode = Enum.TryParse<ProductKeyMode>(v, out var k) ? k : ProductKeyMode.Generic,
            },
            new OptionDefinition
            {
                Id = "product-key",
                LabelKey = "Edition.ProductKey.Label",
                DescriptionKey = "Edition.ProductKey.Description",
                Kind = OptionKind.Text,
                // Schneegans' validator pattern, mirrored client-side for an early signal.
                Pattern = "[A-Za-z0-9]{5}-[A-Za-z0-9]{5}-[A-Za-z0-9]{5}-[A-Za-z0-9]{5}-[A-Za-z0-9]{5}",
                PlaceholderKey = "Edition.ProductKey.Placeholder",
                GetString = c => c.Edition.ProductKey,
                SetString = (c, v) => c.Edition.ProductKey = string.IsNullOrWhiteSpace(v) ? null : v!.Trim().ToUpperInvariant(),
            },
            new OptionDefinition
            {
                Id = "edition-id",
                LabelKey = "Edition.Edition.Label",
                DescriptionKey = "Edition.Edition.Description",
                Kind = OptionKind.Dropdown,
                JsonSource = "data/editions.json",
                GetString = c => MapEditionOut(c.Edition.Edition),
                SetString = (c, v) => c.Edition.Edition = MapEditionIn(v),
            },
        ],
    };

    // Adapter: UI dropdown values use the schneegans JSON "Id" (lowercase + underscore)
    // while our own enum uses PascalCase - keep the two in sync in one place.
    private static string MapEditionOut(WindowsEdition e) => e switch
    {
        WindowsEdition.Home => "home",
        WindowsEdition.HomeN => "home_n",
        WindowsEdition.HomeSingleLanguage => "home_single",
        WindowsEdition.Pro => "pro",
        WindowsEdition.ProN => "pro_n",
        WindowsEdition.ProEducation => "pro_education",
        WindowsEdition.ProEducationN => "pro_education_n",
        WindowsEdition.ProForWorkstations => "pro_workstations",
        WindowsEdition.ProForWorkstationsN => "pro_workstations_n",
        WindowsEdition.Education => "education",
        WindowsEdition.EducationN => "education_n",
        WindowsEdition.Enterprise => "enterprise",
        WindowsEdition.EnterpriseN => "enterprise_n",
        _ => "pro",
    };

    private static WindowsEdition MapEditionIn(string? id) => id switch
    {
        "home" => WindowsEdition.Home,
        "home_n" => WindowsEdition.HomeN,
        "home_single" => WindowsEdition.HomeSingleLanguage,
        "pro" => WindowsEdition.Pro,
        "pro_n" => WindowsEdition.ProN,
        "pro_education" => WindowsEdition.ProEducation,
        "pro_education_n" => WindowsEdition.ProEducationN,
        "pro_workstations" => WindowsEdition.ProForWorkstations,
        "pro_workstations_n" => WindowsEdition.ProForWorkstationsN,
        "education" => WindowsEdition.Education,
        "education_n" => WindowsEdition.EducationN,
        "enterprise" => WindowsEdition.Enterprise,
        "enterprise_n" => WindowsEdition.EnterpriseN,
        _ => WindowsEdition.Pro,
    };
}

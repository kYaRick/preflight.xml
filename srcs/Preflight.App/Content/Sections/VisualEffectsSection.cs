using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Visual-effect preset picker. The preset radio maps to schneegans' <c>IEffects</c> variants
/// (<c>DefaultEffects</c>, <c>BestAppearanceEffects</c>, <c>BestPerformanceEffects</c>,
/// <c>CustomEffects</c>). The 17 per-effect checkboxes for <see cref="VisualEffectsPreset.Custom"/>
/// are rendered by <c>VisualEffectsAdvanced.razor</c>: one checkbox per entry in
/// <see cref="VisualEffect"/>, each one named via a <c>VisualEffects.Effect.*</c> resx key.
/// </summary>
public static class VisualEffectsSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "visual-effects",
        TitleKey = "Advanced.Section.visual-effects",
        SubtitleKey = "Section.visual-effects.Subtitle",
        IntroMarkdownPath = "content/sections/visual-effects.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "preset",
                LabelKey = "VisualEffects.Preset.Label",
                DescriptionKey = "VisualEffects.Preset.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new OptionValue(nameof(VisualEffectsPreset.Default),         "VisualEffects.Preset.Default"),
                    new OptionValue(nameof(VisualEffectsPreset.BestAppearance),  "VisualEffects.Preset.BestAppearance"),
                    new OptionValue(nameof(VisualEffectsPreset.BestPerformance), "VisualEffects.Preset.BestPerformance"),
                    new OptionValue(nameof(VisualEffectsPreset.Custom),          "VisualEffects.Preset.Custom"),
                ],
                GetString = c => c.VisualEffects.Preset.ToString(),
                SetString = (c, v) =>
                {
                    if (Enum.TryParse<VisualEffectsPreset>(v, out var parsed))
                        c.VisualEffects.Preset = parsed;
                },
            },
        ],
    };
}

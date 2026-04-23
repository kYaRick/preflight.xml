namespace Preflight.App.Models;

public enum OptionKind
{
    /// <summary>Single-select dropdown; values come from <see cref="OptionDefinition.InlineValues"/> or <see cref="OptionDefinition.JsonSource"/>.</summary>
    Dropdown,
    /// <summary>Plain text input bound to a <see cref="string"/>.</summary>
    Text,
    /// <summary>Boolean checkbox. <see cref="OptionDefinition.GetString"/> / <see cref="OptionDefinition.SetString"/> are ignored; use <see cref="OptionDefinition.GetBool"/> / <see cref="OptionDefinition.SetBool"/>.</summary>
    Checkbox,
    /// <summary>Horizontal radio-button group. Values come from <see cref="OptionDefinition.InlineValues"/>; bound via <see cref="OptionDefinition.GetString"/>/<see cref="OptionDefinition.SetString"/>.</summary>
    Radio,
    /// <summary>Set of independent checkboxes driven by <see cref="OptionDefinition.InlineValues"/>; bound via <see cref="OptionDefinition.GetStringSet"/>/<see cref="OptionDefinition.SetStringSet"/>.</summary>
    CheckboxGroup,
    /// <summary>Numeric input bound via <see cref="OptionDefinition.GetInt"/>/<see cref="OptionDefinition.SetInt"/>.</summary>
    Number,
    /// <summary>Multi-line text area bound via <see cref="OptionDefinition.GetString"/>/<see cref="OptionDefinition.SetString"/>.</summary>
    Textarea,
}

/// <summary>
/// Describes one user-editable option inside a <see cref="SectionDefinition"/>.
/// </summary>
public sealed record OptionDefinition
{
    /// <summary>Stable within a section - e.g. <c>display-lang</c>. Used for anchor links from Advanced → Docs.</summary>
    public required string Id { get; init; }

    /// <summary>Resource key for the option label shown next to the control.</summary>
    public required string LabelKey { get; init; }

    /// <summary>Resource key for the inline help block. Shown in Docs; shown in Advanced only when <see cref="ShowDescriptionInAdvanced"/> is true.</summary>
    public string? DescriptionKey { get; init; }

    /// <summary>Opt-in flag for options whose tradeoff is genuinely non-obvious (e.g. a risky checkbox). Default: Advanced shows label + control only.</summary>
    public bool ShowDescriptionInAdvanced { get; init; }

    /// <summary>External reference (e.g. Microsoft Learn URL). Rendered as "Learn more ↗" inline in both views.</summary>
    public string? LearnMoreUrl { get; init; }

    public required OptionKind Kind { get; init; }

    // ─── Value source (Dropdown / Radio / CheckboxGroup) ─────────

    /// <summary>When non-null, the view fetches this JSON path from <c>wwwroot/</c> and maps <c>{Id, DisplayName}</c> into options.</summary>
    public string? JsonSource { get; init; }

    /// <summary>Inline value list for small dropdowns / radios / checkbox groups.</summary>
    public IReadOnlyList<OptionValue>? InlineValues { get; init; }

    // ─── Textarea tuning ──────────────────────────────────────────

    /// <summary>Row count hint for the <see cref="OptionKind.Textarea"/> control.</summary>
    public int TextareaRows { get; init; } = 6;

    /// <summary>Monospace rendering hint for the <see cref="OptionKind.Textarea"/> control.</summary>
    public bool Monospace { get; init; }

    /// <summary>Placeholder text for text-like inputs.</summary>
    public string? Placeholder { get; init; }

    // ─── Binding ──────────────────────────────────────────────────

    public Func<UnattendConfig, string?>? GetString { get; init; }
    public Action<UnattendConfig, string?>? SetString { get; init; }

    public Func<UnattendConfig, bool>? GetBool { get; init; }
    public Action<UnattendConfig, bool>? SetBool { get; init; }

    public Func<UnattendConfig, int>? GetInt { get; init; }
    public Action<UnattendConfig, int>? SetInt { get; init; }

    public Func<UnattendConfig, ISet<string>>? GetStringSet { get; init; }
    public Action<UnattendConfig, string, bool>? SetStringSetItem { get; init; }
}

/// <summary>A single entry in an inline dropdown / radio / checkbox-group option list.</summary>
public sealed record OptionValue(string Value, string DisplayKey);

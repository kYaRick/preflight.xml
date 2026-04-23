namespace Preflight.App.Models;

public enum OptionKind
{
    /// <summary>Single-select dropdown; values come from <see cref="OptionDefinition.InlineValues"/> or <see cref="OptionDefinition.JsonSource"/>.</summary>
    Dropdown,
    /// <summary>Plain text input bound to a <see cref="string"/>.</summary>
    Text,
    /// <summary>Boolean checkbox. <see cref="OptionDefinition.GetString"/> / <see cref="OptionDefinition.SetString"/> are ignored; use <see cref="OptionDefinition.GetBool"/> / <see cref="OptionDefinition.SetBool"/>.</summary>
    Checkbox,
    /// <summary>Radio group - exactly one of <see cref="OptionDefinition.InlineValues"/> is selected.
    /// Uses <see cref="OptionDefinition.GetString"/> / <see cref="OptionDefinition.SetString"/>.</summary>
    Radio,
    /// <summary>Group of independent checkboxes (each item can be toggled independently).
    /// Uses <see cref="OptionDefinition.IsItemSelected"/> / <see cref="OptionDefinition.SetItemSelected"/>.</summary>
    CheckboxGroup,
    /// <summary>Numeric input bound to an <see cref="int"/>. Uses <see cref="OptionDefinition.GetInt"/> / <see cref="OptionDefinition.SetInt"/>.</summary>
    Number,
    /// <summary>Multi-line text input bound to a <see cref="string"/>. Uses <see cref="OptionDefinition.GetString"/> / <see cref="OptionDefinition.SetString"/>.</summary>
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

    /// <summary>
    /// Optional sub-heading grouping marker. When set, this option appears under an
    /// &lt;h4&gt; sub-heading rendered from this resource key. Consecutive options sharing
    /// the same value render once under one heading.
    /// </summary>
    public string? GroupHeadingKey { get; init; }

    // ─── Value source (Dropdown / Radio / CheckboxGroup) ──────────

    /// <summary>When non-null, the view fetches this JSON path from <c>wwwroot/</c> and maps <c>{Id, DisplayName}</c> into dropdown options.</summary>
    public string? JsonSource { get; init; }

    /// <summary>Inline value list for small dropdowns / radios / checkbox groups where a JSON file would be overkill.</summary>
    public IReadOnlyList<OptionValue>? InlineValues { get; init; }

    // ─── Number-input bounds (Number only) ────────────────────────

    public int? Min { get; init; }
    public int? Max { get; init; }

    // ─── Textarea size (Textarea only) ────────────────────────────

    public int? Rows { get; init; }

    // ─── Binding ──────────────────────────────────────────────────

    public Func<UnattendConfig, string?>? GetString { get; init; }
    public Action<UnattendConfig, string?>? SetString { get; init; }

    public Func<UnattendConfig, bool>? GetBool { get; init; }
    public Action<UnattendConfig, bool>? SetBool { get; init; }

    public Func<UnattendConfig, int>? GetInt { get; init; }
    public Action<UnattendConfig, int>? SetInt { get; init; }

    /// <summary>For <see cref="OptionKind.CheckboxGroup"/>: is this value-id currently selected?</summary>
    public Func<UnattendConfig, string, bool>? IsItemSelected { get; init; }
    /// <summary>For <see cref="OptionKind.CheckboxGroup"/>: set selected state for this value-id.</summary>
    public Action<UnattendConfig, string, bool>? SetItemSelected { get; init; }

    // ─── Conditional visibility ───────────────────────────────────

    /// <summary>Predicate controlling whether this option renders at all. Returns true to show.
    /// Used for master/child patterns (e.g. "when Mode == Configure, show detail checkboxes").</summary>
    public Func<UnattendConfig, bool>? VisibleWhen { get; init; }
}

/// <summary>A single entry in an inline dropdown / radio / checkbox-group option list.</summary>
public sealed record OptionValue(string Value, string DisplayKey);

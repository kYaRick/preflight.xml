namespace Preflight.App.Models;

public enum OptionKind
{
    /// <summary>Single-select dropdown; values come from <see cref="OptionDefinition.InlineValues"/> or <see cref="OptionDefinition.JsonSource"/>.</summary>
    Dropdown,
    /// <summary>Plain text input bound to a <see cref="string"/>.</summary>
    Text,
    /// <summary>Boolean checkbox. Uses <see cref="OptionDefinition.GetBool"/> / <see cref="OptionDefinition.SetBool"/>.</summary>
    Checkbox,
    /// <summary>Exclusive radio group. Items from <see cref="OptionDefinition.InlineValues"/>; bound via <see cref="OptionDefinition.GetString"/>/<see cref="OptionDefinition.SetString"/>.</summary>
    Radio,
    /// <summary>Multi-select group of independent checkboxes.
    /// Membership is read/written via <see cref="OptionDefinition.IsItemSelected"/>/<see cref="OptionDefinition.SetItemSelected"/>
    /// (preferred), or legacy <see cref="OptionDefinition.GetStringSet"/>/<see cref="OptionDefinition.SetStringSetItem"/>.</summary>
    CheckboxGroup,
    /// <summary>Numeric input bound via <see cref="OptionDefinition.GetInt"/>/<see cref="OptionDefinition.SetInt"/>.</summary>
    Number,
    /// <summary>Multi-line text input bound via <see cref="OptionDefinition.GetString"/>/<see cref="OptionDefinition.SetString"/>.</summary>
    Textarea,
}

/// <summary>
/// Describes one user-editable option inside a <see cref="SectionDefinition"/>.
/// Bindings are strongly typed by kind; each kind only reads the delegates it needs.
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
    /// Optional sub-heading grouping marker. Consecutive options sharing the same value
    /// render once under one &lt;h4&gt; from this resource key.
    /// </summary>
    public string? GroupHeadingKey { get; init; }

    // ─── Value source (Dropdown / Radio / CheckboxGroup) ─────────

    /// <summary>When non-null, the view fetches this JSON path from <c>wwwroot/</c> and maps <c>{Id, DisplayName}</c> into options.</summary>
    public string? JsonSource { get; init; }

    /// <summary>Inline value list for small dropdowns / radios / checkbox groups.</summary>
    public IReadOnlyList<OptionValue>? InlineValues { get; init; }

    // ─── Number-input bounds (Number only) ────────────────────────

    public int? Min { get; init; }
    public int? Max { get; init; }

    // ─── Textarea tuning ──────────────────────────────────────────

    /// <summary>Row count hint for the <see cref="OptionKind.Textarea"/> control. Legacy alias honoured by <see cref="SectionView"/>.</summary>
    public int? Rows { get; init; }

    /// <summary>Row count hint for the <see cref="OptionKind.Textarea"/> control.</summary>
    public int TextareaRows { get; init; } = 6;

    /// <summary>Monospace rendering hint.</summary>
    public bool Monospace { get; init; }

    /// <summary>Placeholder text for text-like inputs (raw string).</summary>
    public string? Placeholder { get; init; }

    /// <summary>Resource-keyed placeholder (alternative to raw <see cref="Placeholder"/>).</summary>
    public string? PlaceholderKey { get; init; }

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

    /// <summary>Legacy CheckboxGroup binding: whole-set read. Prefer <see cref="IsItemSelected"/>.</summary>
    public Func<UnattendConfig, ISet<string>>? GetStringSet { get; init; }
    /// <summary>Legacy CheckboxGroup binding: per-item write. Prefer <see cref="SetItemSelected"/>.</summary>
    public Action<UnattendConfig, string, bool>? SetStringSetItem { get; init; }

    // ─── Conditional visibility ───────────────────────────────────

    /// <summary>Predicate controlling whether this option renders at all. Returns true to show.
    /// Used for master/child patterns (e.g. "when Mode == Configure, show detail checkboxes").</summary>
    public Func<UnattendConfig, bool>? VisibleWhen { get; init; }
}

/// <summary>A single entry in an inline dropdown / radio / checkbox-group option list.</summary>
public sealed record OptionValue(string Value, string DisplayKey);

namespace Preflight.App.Models;

/// <summary>
/// Rendering type for each user-editable option. Determines which control is rendered
/// and which binding delegates are used.
/// </summary>
public enum OptionKind
{
    /// <summary>Single-select dropdown; values come from <see cref="OptionDefinition.InlineValues"/> or <see cref="OptionDefinition.JsonSource"/>.</summary>   
    Dropdown,

    /// <summary>Plain text input bound to a <see cref="string"/> via <see cref="OptionDefinition.GetString"/>/<see cref="OptionDefinition.SetString"/>.</summary>    
    Text,

    /// <summary>Boolean checkbox. Uses <see cref="OptionDefinition.GetBool"/> / <see cref="OptionDefinition.SetBool"/>.</summary>
    Checkbox,

    /// <summary>Mutually-exclusive radio group. Items from <see cref="OptionDefinition.InlineValues"/> (required); bound via <see cref="OptionDefinition.GetString"/>/<see cref="OptionDefinition.SetString"/>.</summary>
    Radio,

    /// <summary>Multi-select group of independent checkboxes. Items come from <see cref="OptionDefinition.InlineValues"/>, <see cref="OptionDefinition.CheckboxItems"/>, or <see cref="OptionDefinition.ItemsProvider"/>. 
    /// Membership is read/written via <see cref="OptionDefinition.IsItemSelected"/>/<see cref="OptionDefinition.SetItemSelected"/> (preferred), 
    /// or legacy <see cref="OptionDefinition.GetStringSet"/>/<see cref="OptionDefinition.SetStringSetItem"/>.</summary>
    CheckboxGroup,

    /// <summary>Numeric integer input bound via <see cref="OptionDefinition.GetInt"/>/<see cref="OptionDefinition.SetInt"/>. 
    /// Respects <see cref="OptionDefinition.Min"/> and <see cref="OptionDefinition.Max"/> for validation.</summary>
    Number,

    /// <summary>Multi-line text input bound via <see cref="OptionDefinition.GetString"/>/<see cref="OptionDefinition.SetString"/>. 
    /// Row count controlled by <see cref="OptionDefinition.Rows"/> or <see cref="OptionDefinition.TextareaRows"/>.</summary>
    Textarea,
}

/// <summary>
/// Describes one user-editable option inside a <see cref="SectionDefinition"/>.
/// Bindings are strongly typed by kind; each kind only reads the delegates it needs.
/// 
/// This is the consolidated schema supporting all UI patterns across phases:
/// - Phase 3a: Dropdown, Text, Checkbox (full wiring)
/// - Phase 3b: Radio, CheckboxGroup, Number, Textarea (added)
/// - Phase 3c+: Extensions and edge cases
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

    /// <summary>Static list of checkbox items for a <see cref="OptionKind.CheckboxGroup"/> - label + id. 
    /// Membership is read/written via <see cref="IsItemSelected"/> / <see cref="SetItemSelected"/>.</summary>
    public IReadOnlyList<OptionValue>? CheckboxItems { get; init; }

    /// <summary>Dynamic item provider for <see cref="OptionKind.CheckboxGroup"/> (e.g. from a service). 
    /// Supplies the list of {Id, DisplayName} entries at render time.</summary>       
    public Func<IReadOnlyList<OptionValue>>? ItemsProvider { get; init; }       

    // ─── Number-input bounds (Number only) ────────────────────────

    public int? Min { get; init; }
    public int? Max { get; init; }

    // ─── Textarea tuning ──────────────────────────────────────────

    /// <summary>Row count hint for the <see cref="OptionKind.Textarea"/> control. Legacy alias; use <see cref="TextareaRows"/> in new code.</summary>
    public int? Rows { get; init; }

    /// <summary>Row count hint for the <see cref="OptionKind.Textarea"/> control. Default: 6.</summary>
    public int TextareaRows { get; init; } = 6;

    /// <summary>Monospace rendering hint for <see cref="OptionKind.Text"/> or <see cref="OptionKind.Textarea"/>.</summary>
    public bool Monospace { get; init; }

    /// <summary>Placeholder text for text-like inputs (raw string).</summary>  
    public string? Placeholder { get; init; }

    /// <summary>Resource-keyed placeholder (alternative to raw <see cref="Placeholder"/>).</summary>
    public string? PlaceholderKey { get; init; }

    /// <summary>Client-side HTML pattern for <see cref="OptionKind.Text"/> - e.g. the product-key group of 5×5 alphanumerics.</summary>
    public string? Pattern { get; init; }

    // ─── Binding (strongly-typed per kind) ────────────────────────

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

/// <summary>
/// One boolean toggle inside a <see cref="OptionKind.CheckboxGroup"/>. Rendered as a child
/// checkbox under the parent option's label with its own getter/setter.
/// </summary>
public sealed record CheckboxItem(
    string Id,
    string LabelKey,
    Func<UnattendConfig, bool> Get,
    Action<UnattendConfig, bool> Set);

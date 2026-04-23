namespace Preflight.App.Models;

public enum OptionKind
{
    /// <summary>Single-select dropdown; values come from <see cref="OptionDefinition.InlineValues"/> or <see cref="OptionDefinition.JsonSource"/>.</summary>
    Dropdown,
    /// <summary>Plain text input bound to a <see cref="string"/>.</summary>
    Text,
    /// <summary>Boolean checkbox. <see cref="OptionDefinition.GetString"/> / <see cref="OptionDefinition.SetString"/> are ignored; use <see cref="OptionDefinition.GetBool"/> / <see cref="OptionDefinition.SetBool"/>.</summary>
    Checkbox,
    /// <summary>Mutually-exclusive radio group. Value list comes from <see cref="OptionDefinition.InlineValues"/>; string value bound via <see cref="OptionDefinition.GetString"/> / <see cref="OptionDefinition.SetString"/>.</summary>
    Radio,
    /// <summary>Fan-out of independent checkboxes (one row per entry). Items come from <see cref="OptionDefinition.CheckboxItems"/>; membership read/written via <see cref="OptionDefinition.IsItemSelected"/> / <see cref="OptionDefinition.SetItemSelected"/>.</summary>
    CheckboxGroup,
    /// <summary>Integer input with optional <see cref="OptionDefinition.Min"/> / <see cref="OptionDefinition.Max"/>. Bound via <see cref="OptionDefinition.GetInt"/> / <see cref="OptionDefinition.SetInt"/>.</summary>
    Number,
    /// <summary>Multi-line text input (<see cref="OptionDefinition.Rows"/> visible rows). Bound via <see cref="OptionDefinition.GetString"/> / <see cref="OptionDefinition.SetString"/>.</summary>
    Textarea,
}

/// <summary>
/// Describes one user-editable option inside a <see cref="SectionDefinition"/>.
/// Simple sections are fully data-driven via this type; complex sections (disk, edition, ...)
/// use it as a building block plus a custom .razor component for conditional layout.
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

    // ─── Value source (Dropdown / Radio / CheckboxGroup) ──────────

    /// <summary>When non-null, the view fetches this JSON path from <c>wwwroot/</c> and maps <c>{Id, DisplayName}</c> into dropdown options.</summary>
    public string? JsonSource { get; init; }

    /// <summary>Inline value list for small dropdowns / radios where a JSON file would be overkill.</summary>
    public IReadOnlyList<OptionValue>? InlineValues { get; init; }

    /// <summary>Entries for a <see cref="OptionKind.CheckboxGroup"/> - label + id. Membership is read/written via <see cref="IsItemSelected"/> / <see cref="SetItemSelected"/>.</summary>
    public IReadOnlyList<OptionValue>? CheckboxItems { get; init; }

    // ─── Number bounds + textarea rows ────────────────────────────

    public int? Min { get; init; }
    public int? Max { get; init; }
    public int? Rows { get; init; }

    /// <summary>Placeholder text (resource key) shown in an empty Text / Textarea / Number input.</summary>
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

    /// <summary>For <see cref="OptionKind.CheckboxGroup"/>: is the item (identified by <see cref="OptionValue.Value"/>) currently selected?</summary>
    public Func<UnattendConfig, string, bool>? IsItemSelected { get; init; }
    /// <summary>For <see cref="OptionKind.CheckboxGroup"/>: toggle the item's membership.</summary>
    public Action<UnattendConfig, string, bool>? SetItemSelected { get; init; }
}

/// <summary>A single entry in an inline dropdown / radio / checkbox-group option list.</summary>
public sealed record OptionValue(string Value, string DisplayKey);

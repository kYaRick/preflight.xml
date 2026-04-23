namespace Preflight.App.Models;

public enum OptionKind
{
    /// <summary>Single-select dropdown; values come from <see cref="OptionDefinition.InlineValues"/> or <see cref="OptionDefinition.JsonSource"/>.</summary>
    Dropdown,
    /// <summary>Plain text input bound to a <see cref="string"/>.</summary>
    Text,
    /// <summary>Boolean checkbox. <see cref="OptionDefinition.GetString"/> / <see cref="OptionDefinition.SetString"/> are ignored; use <see cref="OptionDefinition.GetBool"/> / <see cref="OptionDefinition.SetBool"/>.</summary>
    Checkbox,
    /// <summary>Radio-group — one of several string values. Uses <see cref="OptionDefinition.InlineValues"/> (required) and <see cref="OptionDefinition.GetString"/> / <see cref="OptionDefinition.SetString"/>.</summary>
    Radio,
    /// <summary>Multi-select checkbox list — uses <see cref="OptionDefinition.IsItemSelected"/> / <see cref="OptionDefinition.SetItemSelected"/> plus a dynamic item source.</summary>
    CheckboxGroup,
    /// <summary>Numeric input (integer). Uses <see cref="OptionDefinition.GetInt"/> / <see cref="OptionDefinition.SetInt"/>.</summary>
    Number,
    /// <summary>Multi-line text area for free-form XML / script content. Uses <see cref="OptionDefinition.GetString"/> / <see cref="OptionDefinition.SetString"/>.</summary>
    Textarea,
}

/// <summary>
/// Describes one user-editable option inside a <see cref="SectionDefinition"/>.
/// For Phase 3a, only <see cref="OptionKind.Dropdown"/> (string values) and <see cref="OptionKind.Checkbox"/> are fully wired.
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

    /// <summary>Inline value list for small dropdowns / radio groups where a JSON file would be overkill.</summary>
    public IReadOnlyList<OptionValue>? InlineValues { get; init; }

    /// <summary>For <see cref="OptionKind.CheckboxGroup"/> — supplies the list of {Id, DisplayName} entries dynamically (e.g. from a service).</summary>
    public Func<IReadOnlyList<OptionValue>>? ItemsProvider { get; init; }

    // ─── Visibility ───────────────────────────────────────────────

    /// <summary>If set, the option is rendered only when the predicate returns true for the current config — used for conditional reveals (e.g. a Wi-Fi password field appearing only when "Configure" is selected).</summary>
    public Func<UnattendConfig, bool>? VisibleWhen { get; init; }

    // ─── Binding (strongly-typed for the two kinds we ship in 3a) ─

    public Func<UnattendConfig, string?>? GetString { get; init; }
    public Action<UnattendConfig, string?>? SetString { get; init; }

    public Func<UnattendConfig, bool>? GetBool { get; init; }
    public Action<UnattendConfig, bool>? SetBool { get; init; }

    public Func<UnattendConfig, int>? GetInt { get; init; }
    public Action<UnattendConfig, int>? SetInt { get; init; }

    // ─── CheckboxGroup item binding ──────────────────────────────

    /// <summary>For <see cref="OptionKind.CheckboxGroup"/> — predicate returning whether a given item id is currently selected.</summary>
    public Func<UnattendConfig, string, bool>? IsItemSelected { get; init; }
    /// <summary>For <see cref="OptionKind.CheckboxGroup"/> — mutator that adds/removes an item id from the underlying collection.</summary>
    public Action<UnattendConfig, string, bool>? SetItemSelected { get; init; }
}

/// <summary>A single entry in an inline dropdown / radio option list.</summary>
/// <param name="Value">Stored value (persisted on the model).</param>
/// <param name="DisplayKey">Resource key for the human-readable label. Code that creates <see cref="OptionValue"/> from a non-localized source (e.g. a bloatware catalog) may pass the literal display string here; the view treats the key as a label if no resource matches.</param>
public sealed record OptionValue(string Value, string DisplayKey);

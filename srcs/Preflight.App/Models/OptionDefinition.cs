namespace Preflight.App.Models;

public enum OptionKind
{
    /// <summary>Single-select dropdown; values come from <see cref="OptionDefinition.InlineValues"/> or <see cref="OptionDefinition.JsonSource"/>.</summary>
    Dropdown,
    /// <summary>Plain text input bound to a <see cref="string"/>.</summary>
    Text,
    /// <summary>Boolean checkbox. <see cref="OptionDefinition.GetString"/> / <see cref="OptionDefinition.SetString"/> are ignored; use <see cref="OptionDefinition.GetBool"/> / <see cref="OptionDefinition.SetBool"/>.</summary>
    Checkbox,
    /// <summary>Single-select radio group; values come from <see cref="OptionDefinition.InlineValues"/>. Uses GetString/SetString.</summary>
    Radio,
    /// <summary>Multi-line text input bound to a <see cref="string"/>. Uses GetString/SetString.</summary>
    Textarea,
    /// <summary>
    /// Set of independent boolean checkboxes sharing a single label. Each child item is listed in
    /// <see cref="OptionDefinition.CheckboxItems"/> with its own getter/setter. Parent GetBool/SetBool
    /// are ignored.
    /// </summary>
    CheckboxGroup,
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

    // ─── Value source (Dropdown / Radio) ──────────────────────────

    /// <summary>When non-null, the view fetches this JSON path from <c>wwwroot/</c> and maps <c>{Id, DisplayName}</c> into dropdown options.</summary>
    public string? JsonSource { get; init; }

    /// <summary>Inline value list for small dropdowns or radio groups.</summary>
    public IReadOnlyList<OptionValue>? InlineValues { get; init; }

    /// <summary>Children for <see cref="OptionKind.CheckboxGroup"/>: each one is an independent bool toggle.</summary>
    public IReadOnlyList<CheckboxItem>? CheckboxItems { get; init; }

    // ─── Binding (strongly-typed for the two kinds we ship in 3a) ─

    public Func<UnattendConfig, string?>? GetString { get; init; }
    public Action<UnattendConfig, string?>? SetString { get; init; }

    public Func<UnattendConfig, bool>? GetBool { get; init; }
    public Action<UnattendConfig, bool>? SetBool { get; init; }
}

/// <summary>A single entry in an inline dropdown / radio option list.</summary>
public sealed record OptionValue(string Value, string DisplayKey);

/// <summary>
/// One boolean toggle inside a <see cref="OptionKind.CheckboxGroup"/>. Rendered as a child
/// checkbox under the parent option's label.
/// </summary>
public sealed record CheckboxItem(
    string Id,
    string LabelKey,
    Func<UnattendConfig, bool> Get,
    Action<UnattendConfig, bool> Set);

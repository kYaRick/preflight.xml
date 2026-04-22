namespace Preflight.App.Models;

public enum OptionKind
{
    /// <summary>Single-select dropdown; values come from <see cref="OptionDefinition.InlineValues"/> or <see cref="OptionDefinition.JsonSource"/>.</summary>
    Dropdown,
    /// <summary>Plain text input bound to a <see cref="string"/>.</summary>
    Text,
    /// <summary>Boolean checkbox. <see cref="OptionDefinition.GetString"/> / <see cref="OptionDefinition.SetString"/> are ignored; use <see cref="OptionDefinition.GetBool"/> / <see cref="OptionDefinition.SetBool"/>.</summary>
    Checkbox,
}

/// <summary>
/// Describes one user-editable option inside a <see cref="SectionDefinition"/>.
/// For Phase 3a, only <see cref="OptionKind.Dropdown"/> (string values) and <see cref="OptionKind.Checkbox"/> are fully wired.
/// </summary>
public sealed record OptionDefinition
{
    /// <summary>Stable within a section — e.g. <c>display-lang</c>. Used for anchor links from Advanced → Docs.</summary>
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

    // ─── Value source (Dropdown only) ─────────────────────────────

    /// <summary>When non-null, the view fetches this JSON path from <c>wwwroot/</c> and maps <c>{Id, DisplayName}</c> into dropdown options.</summary>
    public string? JsonSource { get; init; }

    /// <summary>Inline value list for small dropdowns where a JSON file would be overkill.</summary>
    public IReadOnlyList<OptionValue>? InlineValues { get; init; }

    // ─── Binding (strongly-typed for the two kinds we ship in 3a) ─

    public Func<UnattendConfig, string?>? GetString { get; init; }
    public Action<UnattendConfig, string?>? SetString { get; init; }

    public Func<UnattendConfig, bool>? GetBool { get; init; }
    public Action<UnattendConfig, bool>? SetBool { get; init; }
}

/// <summary>A single entry in an inline dropdown option list.</summary>
public sealed record OptionValue(string Value, string DisplayKey);

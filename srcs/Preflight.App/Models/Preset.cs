namespace Preflight.App.Models;

/// <summary>
/// A named factory that produces a fully-populated <see cref="UnattendConfig"/> for common scenarios.
/// Presets are registered once in <see cref="Services.PresetService"/> and exposed on the preset picker.
/// </summary>
public sealed class Preset
{
    public required string Id { get; init; }
    public required string NameKey { get; init; }
    public required string DescriptionKey { get; init; }
    public required string Icon { get; init; }
    public required Func<UnattendConfig> Factory { get; init; }
}

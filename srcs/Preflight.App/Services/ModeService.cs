using Preflight.App.Models;

namespace Preflight.App.Services;

/// <summary>
/// Holds the shared state for all three UX modes.
/// Registered as a singleton - one <see cref="UnattendConfig"/> persists across mode switches.
/// </summary>
public sealed class ModeService
{
    public AppMode CurrentMode { get; private set; } = AppMode.Wizard;

    public UnattendConfig Config { get; private set; } = new();

    public string? ActivePresetId { get; private set; }

    /// <summary>Raised whenever <see cref="CurrentMode"/> or <see cref="Config"/> changes.</summary>
    public event Action? StateChanged;

    public void SwitchMode(AppMode mode)
    {
        if (CurrentMode == mode) return;
        CurrentMode = mode;
        StateChanged?.Invoke();
    }

    public void ApplyPreset(Preset preset)
    {
        ArgumentNullException.ThrowIfNull(preset);
        Config = preset.Factory();
        ActivePresetId = preset.Id;
        StateChanged?.Invoke();
    }

    public void ResetConfig()
    {
        Config = new UnattendConfig();
        ActivePresetId = null;
        StateChanged?.Invoke();
    }

    /// <summary>Notify subscribers after mutating <see cref="Config"/> in place.</summary>
    public void NotifyConfigChanged() => StateChanged?.Invoke();
}

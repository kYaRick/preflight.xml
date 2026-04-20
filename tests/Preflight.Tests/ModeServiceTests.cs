using Preflight.App.Models;
using Preflight.App.Services;

namespace Preflight.Tests;

public sealed class ModeServiceTests
{
    [Fact]
    public void Defaults_to_Wizard_mode()
    {
        var svc = new ModeService();
        Assert.Equal(AppMode.Wizard, svc.CurrentMode);
        Assert.NotNull(svc.Config);
        Assert.Null(svc.ActivePresetId);
    }

    [Fact]
    public void SwitchMode_raises_StateChanged_and_updates()
    {
        var svc = new ModeService();
        var raised = 0;
        svc.StateChanged += () => raised++;

        svc.SwitchMode(AppMode.Advanced);

        Assert.Equal(AppMode.Advanced, svc.CurrentMode);
        Assert.Equal(1, raised);
    }

    [Fact]
    public void SwitchMode_is_noop_when_already_in_target_mode()
    {
        var svc = new ModeService();
        var raised = 0;
        svc.StateChanged += () => raised++;

        svc.SwitchMode(AppMode.Wizard);

        Assert.Equal(0, raised);
    }

    [Fact]
    public void ApplyPreset_replaces_config_and_stores_preset_id()
    {
        var svc = new ModeService();
        var presets = new PresetService();
        var preset = presets.ById("clean-home-pc");
        Assert.NotNull(preset);

        svc.ApplyPreset(preset!);

        Assert.Equal("clean-home-pc", svc.ActivePresetId);
        Assert.NotEmpty(svc.Config.Users);
        Assert.True(svc.Config.Privacy.DisableTelemetry);
    }

    [Fact]
    public void SwitchMode_preserves_config_state()
    {
        var svc = new ModeService();
        var presets = new PresetService();
        svc.ApplyPreset(presets.ById("vm-proxmox")!);
        var configBefore = svc.Config;

        svc.SwitchMode(AppMode.Advanced);
        svc.SwitchMode(AppMode.Docs);
        svc.SwitchMode(AppMode.Wizard);

        Assert.Same(configBefore, svc.Config);
        Assert.Equal("vm-proxmox", svc.ActivePresetId);
    }

    [Fact]
    public void ResetConfig_clears_active_preset()
    {
        var svc = new ModeService();
        var presets = new PresetService();
        svc.ApplyPreset(presets.ById("minimal-fast")!);

        svc.ResetConfig();

        Assert.Null(svc.ActivePresetId);
        Assert.Empty(svc.Config.Users);
    }
}

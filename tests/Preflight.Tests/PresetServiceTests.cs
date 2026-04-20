using Preflight.App.Models;
using Preflight.App.Services;

namespace Preflight.Tests;

public sealed class PresetServiceTests
{
    [Fact]
    public void Exposes_all_four_built_in_presets()
    {
        var svc = new PresetService();

        Assert.Equal(4, svc.All.Count);
        Assert.Contains(svc.All, p => p.Id == "clean-home-pc");
        Assert.Contains(svc.All, p => p.Id == "vm-proxmox");
        Assert.Contains(svc.All, p => p.Id == "corporate-baseline");
        Assert.Contains(svc.All, p => p.Id == "minimal-fast");
    }

    [Fact]
    public void ById_returns_matching_preset()
    {
        var svc = new PresetService();

        Assert.Equal("clean-home-pc", svc.ById("clean-home-pc")?.Id);
        Assert.Null(svc.ById("does-not-exist"));
    }

    [Theory]
    [InlineData("clean-home-pc")]
    [InlineData("vm-proxmox")]
    [InlineData("corporate-baseline")]
    [InlineData("minimal-fast")]
    public void Factory_produces_non_null_config_for_every_preset(string id)
    {
        var svc = new PresetService();
        var preset = svc.ById(id);
        Assert.NotNull(preset);

        var config = preset!.Factory();

        Assert.NotNull(config);
        Assert.NotNull(config.Region);
        Assert.NotNull(config.Disk);
        Assert.NotNull(config.Edition);
        Assert.False(string.IsNullOrEmpty(config.Region.DisplayLanguage));
    }

    [Fact]
    public void CleanHomePc_removes_Copilot_and_disables_telemetry()
    {
        var config = new PresetService().ById("clean-home-pc")!.Factory();

        Assert.Contains("Copilot", config.Bloatware.AppsToRemove);
        Assert.True(config.Privacy.DisableTelemetry);
        Assert.Equal(ExpressSettingsMode.DisableAll, config.Privacy.ExpressSettings);
    }

    [Fact]
    public void VmProxmox_enables_virtio_and_rdp()
    {
        var config = new PresetService().ById("vm-proxmox")!.Factory();

        Assert.True(config.VmSupport.VirtIoAndQemuAgent);
        Assert.True(config.Security.EnableRdp);
    }

    [Fact]
    public void Minimal_stays_interactive_with_no_tweaks()
    {
        var config = new PresetService().ById("minimal-fast")!.Factory();

        Assert.Equal(DiskMode.Interactive, config.Disk.Mode);
        Assert.Equal(ExpressSettingsMode.Interactive, config.Privacy.ExpressSettings);
        Assert.Empty(config.Bloatware.AppsToRemove);
    }
}

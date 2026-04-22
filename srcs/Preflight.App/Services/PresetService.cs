using Preflight.App.Models;

namespace Preflight.App.Services;

/// <summary>
/// Registry of built-in <see cref="Preset"/> templates. Each returns a fully populated
/// <see cref="UnattendConfig"/> and is surfaced on the preset picker screen.
/// </summary>
public sealed class PresetService
{
    public IReadOnlyList<Preset> All { get; }

    public PresetService()
    {
        All =
        [
            new Preset
            {
                Id = "clean-home-pc",
                NameKey = "Preset.CleanHomePc.Name",
                DescriptionKey = "Preset.CleanHomePc.Description",
                Icon = "🏠",
                Factory = BuildCleanHomePc,
            },
            new Preset
            {
                Id = "vm-proxmox",
                NameKey = "Preset.VmProxmox.Name",
                DescriptionKey = "Preset.VmProxmox.Description",
                Icon = "🖥️",
                Factory = BuildVmProxmox,
            },
            new Preset
            {
                Id = "corporate-baseline",
                NameKey = "Preset.Corporate.Name",
                DescriptionKey = "Preset.Corporate.Description",
                Icon = "🏢",
                Factory = BuildCorporateBaseline,
            },
            new Preset
            {
                Id = "minimal-fast",
                NameKey = "Preset.Minimal.Name",
                DescriptionKey = "Preset.Minimal.Description",
                Icon = "⚡",
                Factory = BuildMinimalFast,
            },
        ];
    }

    public Preset? ById(string id) => All.FirstOrDefault(p => p.Id == id);

    // ─── Factories ───────────────────────────────────────────────

    private static UnattendConfig BuildCleanHomePc() => new()
    {
        Region = { DisplayLanguage = "en-US", InputLanguage = "en-US", HomeLocation = "244" },
        Disk = { Mode = DiskMode.AutoWipe, PartitionStyle = PartitionStyle.Gpt, EspSizeMb = 300 },
        Edition = { KeyMode = ProductKeyMode.Generic, Edition = WindowsEdition.Home },
        Users =
        {
            new UserAccount { Name = "user", DisplayName = "User", Group = AccountGroup.Administrators },
        },
        FirstLogon = { Mode = FirstLogonMode.FirstAdminAccount },
        Privacy =
        {
            ExpressSettings = ExpressSettingsMode.DisableAll,
            DisableTelemetry = true,
            DisableAdvertisingId = true,
        },
        Bloatware =
        {
            AppsToRemove =
            {
                "Copilot", "Cortana", "Bing Search", "Widgets",
                "Teams", "OneDrive", "Xbox", "Phone Link",
            },
        },
        Tweaks =
        {
            ShowFileExtensions = true,
            ClassicContextMenu = true,
            DisableWidgets = true,
            LeftAlignTaskbar = true,
            HideBingSearchResults = true,
        },
    };

    private static UnattendConfig BuildVmProxmox() => new()
    {
        Region = { DisplayLanguage = "en-US", HomeLocation = "244" },
        Disk = { Mode = DiskMode.AutoWipe, PartitionStyle = PartitionStyle.Gpt },
        Edition = { KeyMode = ProductKeyMode.Generic, Edition = WindowsEdition.Pro },
        Users =
        {
            new UserAccount { Name = "admin", DisplayName = "Administrator", Group = AccountGroup.Administrators },
        },
        FirstLogon = { Mode = FirstLogonMode.FirstAdminAccount },
        Privacy = { ExpressSettings = ExpressSettingsMode.DisableAll, DisableTelemetry = true },
        Security = { EnableRdp = true, BypassWin11RequirementsCheck = true },
        VmSupport = { VirtIoAndQemuAgent = true },
        Bloatware = { AppsToRemove = { "Copilot", "Cortana", "Widgets", "Teams", "Xbox" } },
        Tweaks = { ShowFileExtensions = true, DisableWidgets = true },
    };

    private static UnattendConfig BuildCorporateBaseline() => new()
    {
        Region = { DisplayLanguage = "en-US", HomeLocation = "244" },
        Disk = { Mode = DiskMode.AutoWipe, PartitionStyle = PartitionStyle.Gpt },
        Edition = { KeyMode = ProductKeyMode.Interactive, Edition = WindowsEdition.Pro },
        FirstLogon = { Mode = FirstLogonMode.DoNotLogon },
        Privacy = { ExpressSettings = ExpressSettingsMode.DisableAll, DisableTelemetry = true },
        Security = { EnableRdp = true, BypassWin11RequirementsCheck = false },
        Tweaks = { ShowFileExtensions = true },
    };

    private static UnattendConfig BuildMinimalFast() => new()
    {
        Region = { DisplayLanguage = "en-US", HomeLocation = "244" },
        Disk = { Mode = DiskMode.Interactive, PartitionStyle = PartitionStyle.Gpt },
        Edition = { KeyMode = ProductKeyMode.Generic, Edition = WindowsEdition.Home },
        FirstLogon = { Mode = FirstLogonMode.FirstAdminAccount },
        Privacy = { ExpressSettings = ExpressSettingsMode.Interactive },
    };
}

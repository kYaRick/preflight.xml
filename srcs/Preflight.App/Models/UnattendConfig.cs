namespace Preflight.App.Models;

/// <summary>
/// The full user configuration used by all three modes. This is a Phase 2b skeleton -
/// fields reflect the schneegans section structure but the XML generation logic lands in Phase 3.
/// </summary>
public sealed class UnattendConfig
{
    public TargetOs TargetOs { get; set; } = TargetOs.Windows11;
    public RegionSettings Region { get; init; } = new();
    public DiskSettings Disk { get; init; } = new();
    public WindowsEditionSettings Edition { get; init; } = new();
    public List<UserAccount> Users { get; init; } = [];
    public FirstLogonSettings FirstLogon { get; init; } = new();
    public PrivacySettings Privacy { get; init; } = new();
    public SecuritySettings Security { get; init; } = new();
    public BloatwareSettings Bloatware { get; init; } = new();
    public SystemTweaks Tweaks { get; init; } = new();
    public VmSupport VmSupport { get; init; } = new();
    public NetworkSettings Network { get; init; } = new();
    public PersonalizationSettings Personalization { get; init; } = new();
    public ProcessorArchSettings ProcessorArchs { get; init; } = new();
    public SetupSettings Setup { get; init; } = new();
    public ComputerNameSettings ComputerName { get; init; } = new();
    public CompactOsSettings CompactOs { get; init; } = new();
    public TimeZoneSettings TimeZoneSettings { get; init; } = new();
}

public enum TargetOs
{
    Windows10,
    Windows11,
    Both,
}

// ─── Region / language ──────────────────────────────────────────

public sealed class RegionSettings
{
    public string DisplayLanguage { get; set; } = "en-US";
    public string InputLanguage { get; set; } = "en-US";
    // Microsoft "geo ID" numeric string (see geo-ids.json). 244 = United States.
    public string HomeLocation { get; set; } = "244";
    public string? TimeZone { get; set; } // null = auto-detect
}

// ─── Disk ────────────────────────────────────────────────────────

public sealed class DiskSettings
{
    public DiskMode Mode { get; set; } = DiskMode.Interactive;
    public PartitionStyle PartitionStyle { get; set; } = PartitionStyle.Gpt;
    public int EspSizeMb { get; set; } = 300;
    public RecoveryMode Recovery { get; set; } = RecoveryMode.OnRecoveryPartition;
}

public enum DiskMode
{
    Interactive,
    AutoWipe,
    CustomScript,
}

public enum PartitionStyle
{
    Gpt,
    Mbr,
}

public enum RecoveryMode
{
    OnRecoveryPartition,
    OnWindowsPartition,
    Remove,
}

// ─── Windows edition ─────────────────────────────────────────────

public sealed class WindowsEditionSettings
{
    public ProductKeyMode KeyMode { get; set; } = ProductKeyMode.Generic;
    public string? ProductKey { get; set; }
    public WindowsEdition Edition { get; set; } = WindowsEdition.Pro;
}

public enum ProductKeyMode
{
    Generic,
    Custom,
    Interactive,
    FromBios,
}

public enum WindowsEdition
{
    Home,
    HomeN,
    HomeSingleLanguage,
    Pro,
    ProN,
    ProEducation,
    ProForWorkstations,
    Education,
    EducationN,
    Enterprise,
    EnterpriseN,
}

// ─── User accounts ───────────────────────────────────────────────

public sealed class UserAccount
{
    public string Name { get; set; } = "";
    public string? DisplayName { get; set; }
    public string? Password { get; set; }
    public AccountGroup Group { get; set; } = AccountGroup.Users;
}

public enum AccountGroup
{
    Administrators,
    Users,
}

public sealed class FirstLogonSettings
{
    public FirstLogonMode Mode { get; set; } = FirstLogonMode.FirstAdminAccount;
    public bool ObscurePasswordsWithBase64 { get; set; } = true;
}

public enum FirstLogonMode
{
    FirstAdminAccount,
    BuiltInAdministrator,
    DoNotLogon,
}

// ─── Privacy ─────────────────────────────────────────────────────

public sealed class PrivacySettings
{
    public ExpressSettingsMode ExpressSettings { get; set; } = ExpressSettingsMode.Interactive;
    public bool DisableTelemetry { get; set; }
    public bool DisableAdvertisingId { get; set; }
    public bool DisableLocationServices { get; set; }
}

public enum ExpressSettingsMode
{
    DisableAll,
    EnableAll,
    Interactive,
}

// ─── Security ────────────────────────────────────────────────────

public sealed class SecuritySettings
{
    public bool DisableUac { get; set; }
    public bool DisableDefender { get; set; }
    public bool DisableSmartScreen { get; set; }
    public bool EnableRdp { get; set; }
    public bool BypassWin11RequirementsCheck { get; set; }
}

// ─── Bloatware ───────────────────────────────────────────────────

public sealed class BloatwareSettings
{
    public HashSet<string> AppsToRemove { get; init; } = [];
}

// ─── System tweaks ───────────────────────────────────────────────

public sealed class SystemTweaks
{
    public bool EnableLongPaths { get; set; }
    public bool ShowFileExtensions { get; set; }
    public bool ClassicContextMenu { get; set; }
    public bool DisableFastStartup { get; set; }
    public bool DisableWidgets { get; set; }
    public bool LeftAlignTaskbar { get; set; }
    public bool HideBingSearchResults { get; set; }
}

// ─── Virtualization guest additions ──────────────────────────────

public sealed class VmSupport
{
    public bool VirtualBoxGuestAdditions { get; set; }
    public bool VmwareTools { get; set; }
    public bool VirtIoAndQemuAgent { get; set; }
    public bool ParallelsTools { get; set; }
}

// ─── Network ─────────────────────────────────────────────────────

public sealed class NetworkSettings
{
    public WifiMode WifiMode { get; set; } = WifiMode.Interactive;
    public string? WifiSsid { get; set; }
    public string? WifiPassword { get; set; }
    public WifiAuth WifiAuth { get; set; } = WifiAuth.Wpa2Personal;
}

public enum WifiMode
{
    Interactive,
    Skip,
    Configure,
}

public enum WifiAuth
{
    Open,
    Wpa2Personal,
    Wpa3Personal,
}

// ─── Personalization ─────────────────────────────────────────────

public sealed class PersonalizationSettings
{
    public PersonalizationTheme Theme { get; set; } = PersonalizationTheme.Default;
    public string? AccentColor { get; set; } // hex like "#0078d4"
}

public enum PersonalizationTheme
{
    Default,
    Dark,
    Light,
    Custom,
}

// ─── Processor architectures ─────────────────────────────────────

/// <summary>
/// Which processor architectures the generated autounattend.xml should target.
/// Schneegans duplicates components per selected architecture, so at least one must stay true
/// (the adapter falls back to amd64-only when all three are false).
/// </summary>
public sealed class ProcessorArchSettings
{
    public bool X86 { get; set; }
    public bool Amd64 { get; set; } = true;
    public bool Arm64 { get; set; }
}

// ─── Setup-level flags ───────────────────────────────────────────

/// <summary>
/// Misc Setup-time toggles that live as top-level bools on Schneegans' Configuration record.
/// </summary>
public sealed class SetupSettings
{
    public bool BypassRequirementsCheck { get; set; }
    public bool BypassNetworkCheck { get; set; }
    public bool UseConfigurationSet { get; set; }
    public bool HidePowerShellWindows { get; set; }
    public bool KeepSensitiveFiles { get; set; }
    public bool UseNarrator { get; set; }
}

// ─── Computer name ───────────────────────────────────────────────

public sealed class ComputerNameSettings
{
    public ComputerNameMode Mode { get; set; } = ComputerNameMode.Random;
    /// <summary>Used when <see cref="Mode"/> is <see cref="ComputerNameMode.Manual"/>. Max 15 chars, no whitespace / punctuation, not all-digits.</summary>
    public string? CustomName { get; set; }
    /// <summary>PowerShell script used when <see cref="Mode"/> is <see cref="ComputerNameMode.Script"/>. Must echo a valid NetBIOS name on stdout.</summary>
    public string? Script { get; set; }
}

public enum ComputerNameMode
{
    Random,
    Manual,
    Script,
}

// ─── Compact OS ──────────────────────────────────────────────────

public sealed class CompactOsSettings
{
    public CompactOsMode Mode { get; set; } = CompactOsMode.Default;
}

public enum CompactOsMode
{
    Default,
    Enabled,
    Disabled,
}

// ─── Time zone ───────────────────────────────────────────────────

public sealed class TimeZoneSettings
{
    public TimeZoneMode Mode { get; set; } = TimeZoneMode.Implicit;
    /// <summary>Windows time-zone id (e.g. <c>"Pacific Standard Time"</c>) used when <see cref="Mode"/> is <see cref="TimeZoneMode.Explicit"/>.</summary>
    public string? ExplicitId { get; set; }
}

public enum TimeZoneMode
{
    Implicit,
    Explicit,
}

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
    public AccountSecuritySettings AccountSecurity { get; init; } = new();
    public PrivacySettings Privacy { get; init; } = new();
    public SecuritySettings Security { get; init; } = new();
    public BloatwareSettings Bloatware { get; init; } = new();
    public SystemTweaks Tweaks { get; init; } = new();
    public VmSupport VmSupport { get; init; } = new();
    public NetworkSettings Network { get; init; } = new();
    public PersonalizationSettings Personalization { get; init; } = new();
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
    /// <summary>Password used for the built-in Administrator when <see cref="Mode"/> is <see cref="FirstLogonMode.BuiltInAdministrator"/>.</summary>
    public string? BuiltInAdminPassword { get; set; }
    public bool ObscurePasswordsWithBase64 { get; set; } = true;
    /// <summary>When true (and no local accounts are declared), the generator uses Microsoft-account interactive setup.</summary>
    public bool PromptForMicrosoftAccount { get; set; }
    /// <summary>When true (and no local accounts are declared), the generator uses local-account interactive setup (hiding MSA screens).</summary>
    public bool PromptForLocalAccount { get; set; }
}

public enum FirstLogonMode
{
    FirstAdminAccount,
    BuiltInAdministrator,
    DoNotLogon,
}

/// <summary>
/// Password expiration and account-lockout policy exposed in the Users section.
/// Maps one-to-one to schneegans' IPasswordExpirationSettings / ILockoutSettings.
/// </summary>
public sealed class AccountSecuritySettings
{
    public PasswordExpirationMode PasswordExpiration { get; set; } = PasswordExpirationMode.Default;
    /// <summary>Max password age in days when <see cref="PasswordExpiration"/> is <see cref="PasswordExpirationMode.Custom"/>. Valid range 1-999.</summary>
    public int PasswordExpirationDays { get; set; } = 42;

    public LockoutMode Lockout { get; set; } = LockoutMode.Default;
    /// <summary>Threshold (0-999) when <see cref="Lockout"/> is <see cref="LockoutMode.Custom"/>.</summary>
    public int LockoutAttempts { get; set; } = 10;
    /// <summary>Observation window in minutes (1-99999). Must be &lt;= <see cref="LockoutUnlockMinutes"/>.</summary>
    public int LockoutWindowMinutes { get; set; } = 10;
    /// <summary>Lockout duration in minutes (1-99999).</summary>
    public int LockoutUnlockMinutes { get; set; } = 10;
}

public enum PasswordExpirationMode
{
    Default,
    Never,
    Custom,
}

public enum LockoutMode
{
    Default,
    Disabled,
    Custom,
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

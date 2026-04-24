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
    public SourceImageSettings SourceImage { get; init; } = new();
    public WindowsPeSettings Pe { get; init; } = new();
    public List<UserAccount> Users { get; init; } = [];
    public FirstLogonSettings FirstLogon { get; init; } = new();
    public AccountSecuritySettings AccountSecurity { get; init; } = new();
    public PrivacySettings Privacy { get; init; } = new();
    public SecuritySettings Security { get; init; } = new();
    public BloatwareSettings Bloatware { get; init; } = new();
    public SystemTweaks Tweaks { get; init; } = new();
    public VmSupport VmSupport { get; init; } = new();
    public LockKeysSettings LockKeys { get; init; } = new();
    public StickyKeysSettings StickyKeys { get; init; } = new();
    public NetworkSettings Network { get; init; } = new();
    public PersonalizationSettings Personalization { get; init; } = new();
    public ExplorerSettings Explorer { get; init; } = new();
    public StartMenuSettings StartMenu { get; init; } = new();
    public DesktopIconSettings DesktopIcons { get; init; } = new();
    public StartFolderSettings StartFolders { get; init; } = new();
    public VisualEffectsSettings VisualEffects { get; init; } = new();
    public ProcessorArchSettings ProcessorArchs { get; init; } = new();
    public SetupSettings Setup { get; init; } = new();
    public ComputerNameSettings ComputerName { get; init; } = new();
    public CompactOsSettings CompactOs { get; init; } = new();
    public TimeZoneSettings TimeZoneSettings { get; init; } = new();
    public CustomScriptsSettings Scripts { get; init; } = new();
    public WdacSettings Wdac { get; init; } = new();
    public AppLockerSettings AppLocker { get; init; } = new();
    public XmlComponentsSettings Components { get; init; } = new();
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
    public int RecoverySizeMb { get; set; } = 1000;
    public string? CustomScript { get; set; }
    public int? InstallDiskIndex { get; set; }
    public int? InstallPartitionIndex { get; set; }
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

// ─── Source image (which WIM / index Setup picks) ────────────────

public sealed class SourceImageSettings
{
    public SourceImageMode Mode { get; set; } = SourceImageMode.Automatic;
    public int ImageIndex { get; set; } = 1;
    public string? ImageName { get; set; }
}

public enum SourceImageMode
{
    Automatic,
    ByIndex,
    ByName,
}

// ─── Windows PE stage ────────────────────────────────────────────

public sealed class WindowsPeSettings
{
    public PeMode Mode { get; set; } = PeMode.Default;
    public bool Disable8Dot3Names { get; set; }
    public bool PauseBeforePartition { get; set; }
    public bool PauseBeforeReboot { get; set; }
    public string? CustomCmd { get; set; }
}

public enum PeMode
{
    Default,
    Generated,
    Custom,
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
    ProEducationN,
    ProForWorkstations,
    ProForWorkstationsN,
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
    public string? BuiltInAdminPassword { get; set; }
    public bool ObscurePasswordsWithBase64 { get; set; } = true;
    public bool PromptForMicrosoftAccount { get; set; }
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
//
// Mirrors the bool-valued fields of Schneegans.Unattend.Configuration.
// Every property here is passed through in UnattendXmlBuilder.MapSystemTweaks
// so that a schema-level rename in Preflight.Unattend would surface as a
// build error rather than a silent no-op.

public sealed class SystemTweaks
{
    // File Explorer & shell
    public bool ShowFileExtensions { get; set; }
    public bool ShowAllTrayIcons { get; set; }
    public bool HideTaskViewButton { get; set; }
    public bool ClassicContextMenu { get; set; }
    public bool LeftTaskbar { get; set; }
    public bool LaunchToThisPC { get; set; }
    public bool ShowEndTask { get; set; }
    public bool HideInfoTip { get; set; }

    // System / performance
    public bool EnableLongPaths { get; set; }
    public bool HardenSystemDriveAcl { get; set; }
    public bool DeleteJunctions { get; set; }
    public bool AllowPowerShellScripts { get; set; }
    public bool DisableLastAccess { get; set; }
    public bool PreventAutomaticReboot { get; set; }
    public bool DisableFastStartup { get; set; }
    public bool DisableSystemRestore { get; set; }
    public bool TurnOffSystemSounds { get; set; }
    public bool DisableAppSuggestions { get; set; }
    public bool DisableWidgets { get; set; }
    public bool DisableWindowsUpdate { get; set; }
    public bool DisablePointerPrecision { get; set; }
    public bool DeleteWindowsOld { get; set; }
    public bool DisableBingResults { get; set; }
    public bool PreventDeviceEncryption { get; set; }
    public bool DisableCoreIsolation { get; set; }
    public bool DisableAutomaticRestartSignOn { get; set; }
    public bool DisableWpbt { get; set; }

    // Edge
    public bool HideEdgeFre { get; set; }
    public bool DisableEdgeStartupBoost { get; set; }
    public bool MakeEdgeUninstallable { get; set; }
    public bool DeleteEdgeDesktopIcon { get; set; }
}

// ─── Virtualization guest additions ──────────────────────────────

public sealed class VmSupport
{
    public bool VirtualBoxGuestAdditions { get; set; }
    public bool VmwareTools { get; set; }
    public bool VirtIoAndQemuAgent { get; set; }
    public bool ParallelsTools { get; set; }
}

// ─── Lock-key (Caps / Num / Scroll) ──────────────────────────────

public enum LockKeysMode
{
    Default,
    Configure,
}

public enum LockKeyInitialState
{
    Off,
    On,
}

public sealed class LockKeysSettings
{
    public LockKeysMode Mode { get; set; } = LockKeysMode.Default;

    public LockKeyInitialState CapsLockInitial { get; set; } = LockKeyInitialState.Off;
    public LockKeyInitialState NumLockInitial { get; set; } = LockKeyInitialState.On;
    public LockKeyInitialState ScrollLockInitial { get; set; } = LockKeyInitialState.Off;

    /// <summary>When true, the physical key is ignored (scancode mapped to nothing).</summary>
    public bool LockCapsLock { get; set; }
    public bool LockNumLock { get; set; }
    public bool LockScrollLock { get; set; }
}

// ─── Sticky keys accessibility ────────────────────────────────────

public enum StickyKeysMode
{
    Default,
    Disable,
    Configure,
}

public sealed class StickyKeysSettings
{
    public StickyKeysMode Mode { get; set; } = StickyKeysMode.Default;

    // Match Schneegans.Unattend.StickyKeys flag enum.
    public bool HotKeyActive { get; set; }
    public bool Indicator { get; set; }
    public bool TriState { get; set; }
    public bool TwoKeysOff { get; set; }
    public bool AudibleFeedback { get; set; }
    public bool HotKeySound { get; set; }
}

// ─── Network ─────────────────────────────────────────────────────

public sealed class NetworkSettings
{
    public WifiMode WifiMode { get; set; } = WifiMode.Interactive;
    public string? Ssid { get; set; }
    public bool SsidHidden { get; set; }
    public WifiAuth Auth { get; set; } = WifiAuth.Wpa2Personal;
    public string? Password { get; set; }
    public string? ProfileXml { get; set; }

    // Backward-compatible aliases used by older section variants.
    public string? WifiSsid { get => Ssid; set => Ssid = value; }
    public string? WifiPassword { get => Password; set => Password = value; }
    public WifiAuth WifiAuth { get => Auth; set => Auth = value; }

}

public enum WifiMode
{
    Interactive,
    Skip,
    Configure,
    ProfileXml,
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
    public ColorSettings Colors { get; init; } = new();
    public WallpaperSettings Wallpaper { get; init; } = new();
    public LockScreenSettings LockScreen { get; init; } = new();
}

public sealed class ColorSettings
{
    public ColorMode Mode { get; set; } = ColorMode.Default;
    public ColorTheme TaskbarAndStartTheme { get; set; } = ColorTheme.Dark;
    public ColorTheme AppsTheme { get; set; } = ColorTheme.Dark;
    public string AccentColor { get; set; } = "#0078D4";
    public bool AccentOnStartAndTaskbar { get; set; }
    public bool AccentOnTitleBars { get; set; }
    public bool Translucent { get; set; } = true;
}

public enum ColorMode
{
    Default,
    Custom,
}

public enum ColorTheme
{
    Dark,
    Light,
}

public sealed class WallpaperSettings
{
    public WallpaperMode Mode { get; set; } = WallpaperMode.Default;
    public string SolidColor { get; set; } = "#0078D4";
    public string? Script { get; set; }
}

public enum WallpaperMode
{
    Default,
    SolidColor,
    Script,
}

public sealed class LockScreenSettings
{
    public LockScreenMode Mode { get; set; } = LockScreenMode.Default;
    public string? Script { get; set; }
}

public enum LockScreenMode
{
    Default,
    Script,
}

// ─── Explorer (File Explorer) ───────────────────────────────────

public sealed class ExplorerSettings
{
    public ExplorerHideFiles HideFiles { get; set; } = ExplorerHideFiles.Hidden;
    public bool ClassicContextMenu { get; set; }
    public bool ShowFileExtensions { get; set; }
    public bool HideInfoTip { get; set; }
    public bool LaunchToThisPC { get; set; }
    public bool ShowEndTask { get; set; }
}

public enum ExplorerHideFiles
{
    ShowAll,
    OsOnly,
    Hidden,
}

// ─── Start menu & taskbar ───────────────────────────────────────

public sealed class StartMenuSettings
{
    public TaskbarSearchMode TaskbarSearch { get; set; } = TaskbarSearchMode.Box;
    public TaskbarIconsMode TaskbarIcons { get; set; } = TaskbarIconsMode.Default;
    public string? TaskbarIconsXml { get; set; }
    public bool DisableWidgets { get; set; }
    public bool LeftTaskbar { get; set; }
    public bool HideTaskViewButton { get; set; }
    public bool ShowAllTrayIcons { get; set; }
    public bool DisableBingResults { get; set; }
    public StartTilesMode StartTiles { get; set; } = StartTilesMode.Default;
    public string? StartTilesXml { get; set; }
    public StartPinsMode StartPins { get; set; } = StartPinsMode.Default;
    public string? StartPinsJson { get; set; }
}

public enum TaskbarSearchMode
{
    Hide = 0,
    Icon = 1,
    Box = 2,
    Label = 3,
}

public enum TaskbarIconsMode
{
    Default,
    RemoveAll,
    CustomXml,
}

public enum StartTilesMode
{
    Default,
    RemoveAll,
    CustomXml,
}

public enum StartPinsMode
{
    Default,
    RemoveAll,
    CustomJson,
}

// ─── Desktop icons ──────────────────────────────────────────────

public sealed class DesktopIconSettings
{
    public DesktopIconMode Mode { get; set; } = DesktopIconMode.Default;
    public HashSet<string> VisibleIcons { get; init; } = [];
    public bool DeleteEdgeDesktopIcon { get; set; }
}

public enum DesktopIconMode
{
    Default,
    Specific,
}

// ─── Start menu folders ─────────────────────────────────────────

public sealed class StartFolderSettings
{
    public StartFolderMode Mode { get; set; } = StartFolderMode.Default;
    public HashSet<string> VisibleFolders { get; init; } = [];
}

public enum StartFolderMode
{
    Default,
    Specific,
}

// ─── Visual effects ─────────────────────────────────────────────

public sealed class VisualEffectsSettings
{
    public VisualEffectsPreset Preset { get; set; } = VisualEffectsPreset.Default;
    /// <summary>Per-effect overrides. Only consulted when <see cref="Preset"/> is <see cref="VisualEffectsPreset.Custom"/>.</summary>
    public Dictionary<VisualEffect, bool> CustomEffects { get; init; } = [];
}

public enum VisualEffectsPreset
{
    Default,
    BestAppearance,
    BestPerformance,
    Custom,
}

/// <summary>Mirrors <c>Schneegans.Unattend.Effect</c>. Names match so mapping is a direct cast.</summary>
public enum VisualEffect
{
    ControlAnimations,
    AnimateMinMax,
    TaskbarAnimations,
    DWMAeroPeekEnabled,
    MenuAnimation,
    TooltipAnimation,
    SelectionFade,
    DWMSaveThumbnailEnabled,
    CursorShadow,
    ListviewShadow,
    ThumbnailsOrIcon,
    ListviewAlphaSelect,
    DragFullWindows,
    ComboBoxAnimation,
    FontSmoothing,
    ListBoxSmoothScrolling,
    DropShadow,
}

// ─── Processor architectures ─────────────────────────────────────

public sealed class ProcessorArchSettings
{
    public bool X86 { get; set; }
    public bool Amd64 { get; set; } = true;
    public bool Arm64 { get; set; }
}

// ─── Setup-level flags ───────────────────────────────────────────

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
    public string? CustomName { get; set; }
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
    public string? ExplicitId { get; set; }
}

public enum TimeZoneMode
{
    Implicit,
    Explicit,
}

// ─── Custom scripts ──────────────────────────────────────────────

/// <summary>
/// Schneegans' four script phases, rolled up into a single "Scripts" section on the UI.
/// Each list can hold up to <see cref="CustomScriptsSettings.MaxSlotsPerPhase"/> entries -
/// the UI renders empty slots as optional, and the adapter skips ones with empty Content.
/// </summary>
public sealed class CustomScriptsSettings
{
    public const int MaxSlotsPerPhase = 8;

    public List<CustomScript> System { get; init; } = [];
    public List<CustomScript> DefaultUser { get; init; } = [];
    public List<CustomScript> FirstLogon { get; init; } = [];
    public List<CustomScript> UserOnce { get; init; } = [];
    public bool RestartExplorer { get; set; }
}

public sealed class CustomScript
{
    public CustomScriptType Type { get; set; } = CustomScriptType.Cmd;
    public string Content { get; set; } = "";
}

public enum CustomScriptType
{
    Cmd,
    PowerShell,
    Registry,
    VbScript,
}

// ─── WDAC ────────────────────────────────────────────────────────

public sealed class WdacSettings
{
    public WdacMode Mode { get; set; } = WdacMode.NotConfigured;
    public WdacEnforcement Enforcement { get; set; } = WdacEnforcement.Audit;
    public WdacScriptEnforcement ScriptEnforcement { get; set; } = WdacScriptEnforcement.Restricted;
}

public enum WdacMode
{
    NotConfigured,
    Basic,
}

public enum WdacEnforcement
{
    Audit,
    AuditOnBootFail,
    Enforce,
}

public enum WdacScriptEnforcement
{
    Restricted,
    Unrestricted,
}

// ─── AppLocker ───────────────────────────────────────────────────

public sealed class AppLockerSettings
{
    public AppLockerMode Mode { get; set; } = AppLockerMode.NotConfigured;
    public string? PolicyXml { get; set; }
}

public enum AppLockerMode
{
    NotConfigured,
    CustomXml,
}

// ─── XML components (raw injection) ──────────────────────────────

public sealed class XmlComponentsSettings
{
    // Empty by default - rows are added on demand via the Add-component button in the UI.
    // The adapter ignores any row whose ComponentId or Xml is blank, so dropping the
    // three seeded placeholders just removes UI noise; it's not a behaviour change.
    public List<XmlComponentEntry> Entries { get; init; } = [];
}

public sealed class XmlComponentEntry
{
    /// <summary>Lookup key in <c>components.json</c>, e.g. <c>Microsoft-Windows-Shell-Setup|oobeSystem</c>.</summary>
    public string? ComponentId { get; set; }

    public string? Xml { get; set; }
}

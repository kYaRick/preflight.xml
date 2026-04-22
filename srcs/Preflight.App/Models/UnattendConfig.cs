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

    // ─── Shell / Explorer / Start menu cluster (Phase B) ──────────
    public ExplorerSettings Explorer { get; init; } = new();
    public StartMenuSettings StartMenu { get; init; } = new();
    public VisualEffectsSettings VisualEffects { get; init; } = new();
    public DesktopIconSettings DesktopIcons { get; init; } = new();
    public StartFolderSettings StartFolders { get; init; } = new();
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

// ─── Explorer (File Explorer) ───────────────────────────────────
//
// Maps to schneegans' Main.cs flags (ClassicContextMenu, ShowFileExtensions, HideInfoTip,
// LaunchToThisPC, ShowEndTask) plus the HideModes enum.

public sealed class ExplorerSettings
{
    public ExplorerHideFiles HideFiles { get; set; } = ExplorerHideFiles.Hidden;
    public bool ClassicContextMenu { get; set; }
    public bool ShowFileExtensions { get; set; }
    public bool HideInfoTip { get; set; }
    public bool LaunchToThisPC { get; set; }
    public bool ShowEndTask { get; set; }
}

/// <summary>Mirrors <c>Schneegans.Unattend.HideModes</c>. Defaults to <see cref="Hidden"/> (Windows default).</summary>
public enum ExplorerHideFiles
{
    /// <summary>Show all, including hidden + protected OS files.</summary>
    ShowAll,
    /// <summary>Show hidden, but keep protected OS files hidden. Maps to HiddenSystem.</summary>
    OsOnly,
    /// <summary>Default - hide hidden files and protected OS files.</summary>
    Hidden,
}

// ─── Start menu & taskbar ───────────────────────────────────────
//
// Schneegans exposes these via three IStart* interfaces (Default / Empty / Custom*) plus
// boolean flags and TaskbarSearchMode. Our UI flattens the interfaces to enums + a payload
// string that only has meaning for the Custom variant.

public sealed class StartMenuSettings
{
    public TaskbarSearchMode TaskbarSearch { get; set; } = TaskbarSearchMode.Box;

    public TaskbarIconsMode TaskbarIcons { get; set; } = TaskbarIconsMode.Default;
    /// <summary>Only used when <see cref="TaskbarIcons"/> is <see cref="TaskbarIconsMode.CustomXml"/>.</summary>
    public string? TaskbarIconsXml { get; set; }

    public bool DisableWidgets { get; set; }
    public bool LeftTaskbar { get; set; }
    public bool HideTaskViewButton { get; set; }
    public bool ShowAllTrayIcons { get; set; }
    public bool DisableBingResults { get; set; }

    public StartTilesMode StartTiles { get; set; } = StartTilesMode.Default;
    /// <summary>Only used when <see cref="StartTiles"/> is <see cref="StartTilesMode.CustomXml"/>.</summary>
    public string? StartTilesXml { get; set; }

    public StartPinsMode StartPins { get; set; } = StartPinsMode.Default;
    /// <summary>Only used when <see cref="StartPins"/> is <see cref="StartPinsMode.CustomJson"/>.</summary>
    public string? StartPinsJson { get; set; }
}

/// <summary>Mirrors <c>Schneegans.Unattend.TaskbarSearchMode</c>.</summary>
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

// ─── Visual effects ─────────────────────────────────────────────
//
// Schneegans' IEffects has Default / BestAppearance / BestPerformance / Custom variants.
// Custom takes ImmutableDictionary<Effect, bool> for the 17 named toggles.

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

// ─── Desktop icons ──────────────────────────────────────────────
//
// Schneegans' IDesktopIconSettings is Default or Custom (dictionary of DesktopIcon → visible).
// Our UI keeps the list of *visible* icons; anything not in the set maps to false in the dict.

public sealed class DesktopIconSettings
{
    public DesktopIconMode Mode { get; set; } = DesktopIconMode.Default;
    /// <summary>Ids (matching <c>DesktopIcon.json</c>) of icons the user wants visible.</summary>
    public HashSet<string> VisibleIcons { get; init; } = [];
    /// <summary>Independent checkbox - the standalone <c>DeleteEdgeDesktopIcon</c> flag on <c>Configuration</c>.</summary>
    public bool DeleteEdgeDesktopIcon { get; set; }
}

public enum DesktopIconMode
{
    Default,
    Specific,
}

// ─── Start menu folders (Windows 11 only) ───────────────────────
//
// Schneegans' IStartFolderSettings → Default or Custom (dictionary of StartFolder → visible).
// As with desktop icons we track only the visible ids; everything else maps to false.

public sealed class StartFolderSettings
{
    public StartFolderMode Mode { get; set; } = StartFolderMode.Default;
    /// <summary>Ids (matching <c>StartFolder.json</c>) of folders the user wants visible.</summary>
    public HashSet<string> VisibleFolders { get; init; } = [];
}

public enum StartFolderMode
{
    Default,
    Specific,
}

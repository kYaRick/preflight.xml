using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Static metadata for the <c>disk</c> section. The live Advanced UI is rendered by
/// <see cref="DiskSection"/> (the .razor file) because the layout is conditional on
/// the chosen mode - a flat option list would require the user to mentally sort which
/// fields still apply. The option list here still powers Docs and the registry lookup.
/// </summary>
public static class DiskSectionDefinition
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "disk",
        TitleKey = "Advanced.Section.disk",
        SubtitleKey = "Section.disk.Subtitle",
        IntroMarkdownPath = "content/sections/disk.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "mode",
                LabelKey = "Disk.Mode.Label",
                DescriptionKey = "Disk.Mode.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new("Interactive", "Disk.Mode.Interactive"),
                    new("AutoWipe", "Disk.Mode.AutoWipe"),
                    new("CustomScript", "Disk.Mode.CustomScript"),
                ],
                GetString = c => c.Disk.Mode.ToString(),
                SetString = (c, v) => c.Disk.Mode = Enum.TryParse<DiskMode>(v, out var m) ? m : DiskMode.Interactive,
            },
            new OptionDefinition
            {
                Id = "partition-style",
                LabelKey = "Disk.PartitionStyle.Label",
                DescriptionKey = "Disk.PartitionStyle.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new("Gpt", "Disk.PartitionStyle.Gpt"),
                    new("Mbr", "Disk.PartitionStyle.Mbr"),
                ],
                GetString = c => c.Disk.PartitionStyle.ToString(),
                SetString = (c, v) => c.Disk.PartitionStyle = Enum.TryParse<PartitionStyle>(v, out var p) ? p : PartitionStyle.Gpt,
            },
            new OptionDefinition
            {
                Id = "esp-size-mb",
                LabelKey = "Disk.EspSize.Label",
                DescriptionKey = "Disk.EspSize.Description",
                Kind = OptionKind.Number,
                Min = 100,
                Max = 2048,
                GetInt = c => c.Disk.EspSizeMb,
                SetInt = (c, v) => c.Disk.EspSizeMb = v,
            },
            new OptionDefinition
            {
                Id = "recovery",
                LabelKey = "Disk.Recovery.Label",
                DescriptionKey = "Disk.Recovery.Description",
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new("OnRecoveryPartition", "Disk.Recovery.Partition"),
                    new("OnWindowsPartition", "Disk.Recovery.Folder"),
                    new("Remove", "Disk.Recovery.None"),
                ],
                GetString = c => c.Disk.Recovery.ToString(),
                SetString = (c, v) => c.Disk.Recovery = Enum.TryParse<RecoveryMode>(v, out var r) ? r : RecoveryMode.OnRecoveryPartition,
            },
            new OptionDefinition
            {
                Id = "recovery-size-mb",
                LabelKey = "Disk.RecoverySize.Label",
                DescriptionKey = "Disk.RecoverySize.Description",
                Kind = OptionKind.Number,
                Min = 300,
                Max = 4096,
                GetInt = c => c.Disk.RecoverySizeMb,
                SetInt = (c, v) => c.Disk.RecoverySizeMb = v,
            },
            new OptionDefinition
            {
                Id = "custom-script",
                LabelKey = "Disk.CustomScript.Label",
                DescriptionKey = "Disk.CustomScript.Description",
                Kind = OptionKind.Textarea,
                Language = "batch",
                Rows = 12,
                PlaceholderKey = "Disk.CustomScript.Placeholder",
                GetString = c => c.Disk.CustomScript,
                SetString = (c, v) => c.Disk.CustomScript = string.IsNullOrWhiteSpace(v) ? null : v,
            },
            new OptionDefinition
            {
                Id = "install-disk-index",
                LabelKey = "Disk.InstallDiskIndex.Label",
                DescriptionKey = "Disk.InstallDiskIndex.Description",
                Kind = OptionKind.Number,
                Min = 0,
                Max = 31,
                GetInt = c => c.Disk.InstallDiskIndex ?? 0,
                SetInt = (c, v) => c.Disk.InstallDiskIndex = v,
            },
            new OptionDefinition
            {
                Id = "install-partition-index",
                LabelKey = "Disk.InstallPartitionIndex.Label",
                DescriptionKey = "Disk.InstallPartitionIndex.Description",
                Kind = OptionKind.Number,
                Min = 1,
                Max = 128,
                GetInt = c => c.Disk.InstallPartitionIndex ?? 0,
                SetInt = (c, v) => c.Disk.InstallPartitionIndex = v,
            },
        ],
    };
}

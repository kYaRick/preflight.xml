using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Virtual-machine guest-additions toggles. Schneegans installs each of these from an
/// embedded PowerShell script; the user just picks which ones to run.
/// </summary>
public static class VmSupportSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "vm-support",
        TitleKey = "Advanced.Section.vm-support",
        SubtitleKey = "Section.vm-support.Subtitle",
        IntroMarkdownPath = "content/sections/vm-support.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "virtualbox-guest-additions",
                LabelKey = "VmSupport.VirtualBoxGuestAdditions.Label",
                DescriptionKey = "VmSupport.VirtualBoxGuestAdditions.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.VmSupport.VirtualBoxGuestAdditions,
                SetBool = (c, v) => c.VmSupport.VirtualBoxGuestAdditions = v,
            },
            new OptionDefinition
            {
                Id = "vmware-tools",
                LabelKey = "VmSupport.VmwareTools.Label",
                DescriptionKey = "VmSupport.VmwareTools.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.VmSupport.VmwareTools,
                SetBool = (c, v) => c.VmSupport.VmwareTools = v,
            },
            new OptionDefinition
            {
                Id = "virtio-qemu-agent",
                LabelKey = "VmSupport.VirtIoAndQemuAgent.Label",
                DescriptionKey = "VmSupport.VirtIoAndQemuAgent.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.VmSupport.VirtIoAndQemuAgent,
                SetBool = (c, v) => c.VmSupport.VirtIoAndQemuAgent = v,
            },
            new OptionDefinition
            {
                Id = "parallels-tools",
                LabelKey = "VmSupport.ParallelsTools.Label",
                DescriptionKey = "VmSupport.ParallelsTools.Description",
                Kind = OptionKind.Checkbox,
                GetBool = c => c.VmSupport.ParallelsTools,
                SetBool = (c, v) => c.VmSupport.ParallelsTools = v,
            },
        ],
    };
}

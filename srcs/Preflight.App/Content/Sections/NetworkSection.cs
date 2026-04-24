using Preflight.App.Models;

namespace Preflight.App.Content.Sections;

/// <summary>
/// Wi-Fi configuration — four modes (Interactive / Skip / Configure / ProfileXml) with
/// mode-specific follow-up fields that appear only when relevant. The <c>.razor</c>
/// companion mounts a single <see cref="Layout.SectionView"/> against <see cref="Definition"/>;
/// the per-mode reveals are handled by <see cref="OptionDefinition.VisibleWhen"/>.
/// </summary>
public static class NetworkSection
{
    public static readonly SectionDefinition Definition = new()
    {
        Id = "network",
        TitleKey = "Advanced.Section.network",
        SubtitleKey = "Section.network.Subtitle",
        IntroMarkdownPath = "content/sections/network.{locale}.md",
        Options =
        [
            new OptionDefinition
            {
                Id = "wifi-mode",
                LabelKey = "Network.WifiMode.Label",
                DescriptionKey = "Network.WifiMode.Description",
                ShowDescriptionInAdvanced = true,
                Kind = OptionKind.Radio,
                InlineValues =
                [
                    new(nameof(WifiMode.Interactive), "Network.WifiMode.Interactive"),
                    new(nameof(WifiMode.Skip),        "Network.WifiMode.Skip"),
                    new(nameof(WifiMode.Configure),   "Network.WifiMode.Configure"),
                    new(nameof(WifiMode.ProfileXml),  "Network.WifiMode.ProfileXml"),
                ],
                GetString = c => c.Network.WifiMode.ToString(),
                SetString = (c, v) => c.Network.WifiMode = Enum.TryParse<WifiMode>(v, out var m) ? m : WifiMode.Interactive,
            },

            // ── Configure branch ──────────────────────────────────
            new OptionDefinition
            {
                Id = "wifi-ssid",
                LabelKey = "Network.Ssid.Label",
                Kind = OptionKind.Text,
                VisibleWhen = c => c.Network.WifiMode == WifiMode.Configure,
                GetString = c => c.Network.Ssid,
                SetString = (c, v) => c.Network.Ssid = v,
            },
            new OptionDefinition
            {
                Id = "wifi-hidden",
                LabelKey = "Network.SsidHidden.Label",
                Kind = OptionKind.Checkbox,
                VisibleWhen = c => c.Network.WifiMode == WifiMode.Configure,
                GetBool = c => c.Network.SsidHidden,
                SetBool = (c, v) => c.Network.SsidHidden = v,
            },
            new OptionDefinition
            {
                Id = "wifi-auth",
                LabelKey = "Network.Auth.Label",
                Kind = OptionKind.Dropdown,
                VisibleWhen = c => c.Network.WifiMode == WifiMode.Configure,
                InlineValues =
                [
                    new(nameof(WifiAuth.Open),          "Network.Auth.Open"),
                    new(nameof(WifiAuth.Wpa2Personal),  "Network.Auth.Wpa2Personal"),
                    new(nameof(WifiAuth.Wpa3Personal),  "Network.Auth.Wpa3Personal"),
                ],
                GetString = c => c.Network.Auth.ToString(),
                SetString = (c, v) => c.Network.Auth = Enum.TryParse<WifiAuth>(v, out var a) ? a : WifiAuth.Wpa2Personal,
            },
            new OptionDefinition
            {
                Id = "wifi-password",
                LabelKey = "Network.Password.Label",
                Kind = OptionKind.Text,
                // Password isn't relevant for Open auth; keep the field reachable for the
                // other two modes that share the Configure branch.
                VisibleWhen = c => c.Network.WifiMode == WifiMode.Configure && c.Network.Auth != WifiAuth.Open,
                GetString = c => c.Network.Password,
                SetString = (c, v) => c.Network.Password = v,
            },

            // ── ProfileXml branch ─────────────────────────────────
            new OptionDefinition
            {
                Id = "wifi-profile-xml",
                LabelKey = "Network.ProfileXml.Label",
                DescriptionKey = "Network.ProfileXml.Description",
                ShowDescriptionInAdvanced = true,
                Kind = OptionKind.Textarea,
                Language = "xml",
                TextareaRows = 10,
                VisibleWhen = c => c.Network.WifiMode == WifiMode.ProfileXml,
                GetString = c => c.Network.ProfileXml,
                SetString = (c, v) => c.Network.ProfileXml = v,
            },
        ],
    };
}

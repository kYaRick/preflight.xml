using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Preflight.App.Models;

namespace Preflight.App.Services;

/// <summary>
/// Serializes <see cref="UnattendConfig"/> into a Windows <c>autounattend.xml</c> document.
///
/// Phase 3a scope: Region, Edition, Users, Disk (install target only), Bloatware (FirstLogon script stub).
/// Subsequent phases extend this via additional private Apply* methods — the public surface stays stable.
/// </summary>
public sealed class UnattendXmlBuilder
{
    private static readonly XNamespace Unattend = "urn:schemas-microsoft-com:unattend";
    private static readonly XNamespace Wcm = "http://schemas.microsoft.com/WMIConfig/2002/State";

    private const string PublicKeyToken = "31bf3856ad364e35";

    private readonly IFormatProvider _invariant = CultureInfo.InvariantCulture;

    public string Build(UnattendConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var passes = new Dictionary<string, XElement>(StringComparer.Ordinal)
        {
            ["offlineServicing"] = NewPass("offlineServicing"),
            ["windowsPE"] = NewPass("windowsPE"),
            ["generalize"] = NewPass("generalize"),
            ["specialize"] = NewPass("specialize"),
            ["auditSystem"] = NewPass("auditSystem"),
            ["auditUser"] = NewPass("auditUser"),
            ["oobeSystem"] = NewPass("oobeSystem"),
        };

        ApplyRegion(passes, config.Region);
        ApplyEdition(passes, config.Edition);
        ApplyDisk(passes, config.Disk);
        ApplyUsers(passes, config.Users, config.FirstLogon);
        ApplyBloatware(passes, config.Bloatware);

        var root = new XElement(Unattend + "unattend",
            new XAttribute(XNamespace.Xmlns + "wcm", Wcm.NamespaceName),
            passes.Values);

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        return Serialize(doc);
    }

    // ─── Passes & component scaffolding ─────────────────────────────

    private static XElement NewPass(string pass) =>
        new(Unattend + "settings", new XAttribute("pass", pass));

    private static XElement GetOrAddComponent(XElement pass, string name, string architecture = "amd64")
    {
        var existing = pass.Elements(Unattend + "component")
            .FirstOrDefault(c => (string?)c.Attribute("name") == name
                && (string?)c.Attribute("processorArchitecture") == architecture);
        if (existing is not null) return existing;

        var comp = new XElement(Unattend + "component",
            new XAttribute("name", name),
            new XAttribute("processorArchitecture", architecture),
            new XAttribute("publicKeyToken", PublicKeyToken),
            new XAttribute("language", "neutral"),
            new XAttribute("versionScope", "nonSxS"));
        pass.Add(comp);
        return comp;
    }

    // ─── Region ─────────────────────────────────────────────────────

    private static void ApplyRegion(Dictionary<string, XElement> passes, RegionSettings region)
    {
        // windowsPE: show Setup UI in the configured language before OS is live.
        var winPeIntl = GetOrAddComponent(passes["windowsPE"], "Microsoft-Windows-International-Core-WinPE");
        winPeIntl.Add(new XElement(Unattend + "UILanguage", region.DisplayLanguage));
        winPeIntl.Add(new XElement(Unattend + "SetupUILanguage",
            new XElement(Unattend + "UILanguage", region.DisplayLanguage)));

        // oobeSystem: final locale configuration applied to the user session.
        var oobeIntl = GetOrAddComponent(passes["oobeSystem"], "Microsoft-Windows-International-Core");
        oobeIntl.Add(new XElement(Unattend + "InputLocale", region.InputLanguage));
        oobeIntl.Add(new XElement(Unattend + "SystemLocale", region.DisplayLanguage));
        oobeIntl.Add(new XElement(Unattend + "UILanguage", region.DisplayLanguage));
        oobeIntl.Add(new XElement(Unattend + "UserLocale", region.DisplayLanguage));
    }

    // ─── Edition ────────────────────────────────────────────────────

    private static void ApplyEdition(Dictionary<string, XElement> passes, WindowsEditionSettings edition)
    {
        var setup = GetOrAddComponent(passes["windowsPE"], "Microsoft-Windows-Setup");
        var userData = setup.Element(Unattend + "UserData") ?? AddChild(setup, "UserData");
        var productKey = userData.Element(Unattend + "ProductKey") ?? AddChild(userData, "ProductKey");

        var key = edition.KeyMode switch
        {
            ProductKeyMode.Generic => GenericKey(edition.Edition),
            ProductKeyMode.Custom => edition.ProductKey,
            ProductKeyMode.Interactive => null,
            ProductKeyMode.FromBios => null,
            _ => null,
        };

        if (!string.IsNullOrWhiteSpace(key))
        {
            productKey.Add(new XElement(Unattend + "Key", key));
            productKey.Add(new XElement(Unattend + "WillShowUI", "OnError"));
        }
        else
        {
            productKey.Add(new XElement(Unattend + "Key"));
            productKey.Add(new XElement(Unattend + "WillShowUI", "Always"));
        }

        if (userData.Element(Unattend + "AcceptEula") is null)
            userData.Add(new XElement(Unattend + "AcceptEula", "true"));
    }

    /// <summary>
    /// GVLK (generic volume license keys) published by Microsoft — activate against KMS only.
    /// Values sourced from cschneegans/unattend-generator/resource/WindowsEdition.json (MIT).
    /// </summary>
    private static string GenericKey(WindowsEdition edition) => edition switch
    {
        WindowsEdition.Home => "YTMG3-N6DKC-DKB77-7M9GH-8HVX7",
        WindowsEdition.HomeN => "4CPRK-NM3K3-X6XXQ-RXX86-WXCHW",
        WindowsEdition.HomeSingleLanguage => "BT79Q-G7N6G-PGBYW-4YWX6-6F4BT",
        WindowsEdition.Pro => "VK7JG-NPHTM-C97JM-9MPGT-3V66T",
        WindowsEdition.ProN => "2B87N-8KFHP-DKV6R-Y2C8J-PKCKT",
        WindowsEdition.ProEducation => "8PTT6-RNW4C-6V7J2-C2D3X-MHBPB",
        WindowsEdition.ProForWorkstations => "DXG7C-N36C4-C4HTG-X4T3X-2YV77",
        WindowsEdition.Education => "YNMGQ-8RYV3-4PGQ3-C8XTP-7CFBY",
        WindowsEdition.EducationN => "84NGF-MHBT6-FXBX8-QWJK7-DRR8H",
        WindowsEdition.Enterprise => "XGVPP-NMH47-7TTHJ-W3FW7-8HV2C",
        WindowsEdition.EnterpriseN => "WGGHN-J84D6-QYCPR-T7PJ7-X766F",
        _ => "VK7JG-NPHTM-C97JM-9MPGT-3V66T",
    };

    // ─── Disk ───────────────────────────────────────────────────────

    private static void ApplyDisk(Dictionary<string, XElement> passes, DiskSettings disk)
    {
        // Phase 3a ships with install-target only. Partitioning / DiskConfiguration lands in 3b.
        var setup = GetOrAddComponent(passes["windowsPE"], "Microsoft-Windows-Setup");
        var imageInstall = setup.Element(Unattend + "ImageInstall") ?? AddChild(setup, "ImageInstall");
        var osImage = imageInstall.Element(Unattend + "OSImage") ?? AddChild(imageInstall, "OSImage");

        if (osImage.Element(Unattend + "InstallTo") is null)
        {
            osImage.Add(new XElement(Unattend + "InstallTo",
                new XElement(Unattend + "DiskID", "0"),
                new XElement(Unattend + "PartitionID", disk.PartitionStyle == PartitionStyle.Gpt ? "3" : "1")));
        }

        if (osImage.Element(Unattend + "InstallFrom") is null)
        {
            osImage.Add(new XElement(Unattend + "InstallFrom",
                new XElement(Unattend + "MetaData",
                    new XAttribute(Wcm + "action", "add"),
                    new XElement(Unattend + "Key", "/IMAGE/INDEX"),
                    new XElement(Unattend + "Value", "1"))));
        }
    }

    // ─── Users ──────────────────────────────────────────────────────

    private static void ApplyUsers(Dictionary<string, XElement> passes, List<UserAccount> users, FirstLogonSettings firstLogon)
    {
        if (users.Count == 0) return;

        var shell = GetOrAddComponent(passes["oobeSystem"], "Microsoft-Windows-Shell-Setup");
        var userAccounts = shell.Element(Unattend + "UserAccounts") ?? AddChild(shell, "UserAccounts");
        var localAccounts = userAccounts.Element(Unattend + "LocalAccounts") ?? AddChild(userAccounts, "LocalAccounts");

        foreach (var account in users)
        {
            if (string.IsNullOrWhiteSpace(account.Name)) continue;

            var localAccount = new XElement(Unattend + "LocalAccount",
                new XAttribute(Wcm + "action", "add"),
                new XElement(Unattend + "Name", account.Name),
                new XElement(Unattend + "DisplayName", account.DisplayName ?? account.Name),
                new XElement(Unattend + "Group", GroupName(account.Group)));

            if (!string.IsNullOrEmpty(account.Password))
                localAccount.Add(BuildPassword("Password", account.Password, firstLogon.ObscurePasswordsWithBase64));

            localAccounts.Add(localAccount);
        }

        // OOBE block — minimum viable to suppress interactive prompts.
        if (shell.Element(Unattend + "OOBE") is null)
        {
            shell.Add(new XElement(Unattend + "OOBE",
                new XElement(Unattend + "ProtectYourPC", "3"),
                new XElement(Unattend + "HideEULAPage", "true"),
                new XElement(Unattend + "HideOEMRegistrationScreen", "true"),
                new XElement(Unattend + "HideOnlineAccountScreens", "true"),
                new XElement(Unattend + "HideWirelessSetupInOOBE", "true")));
        }
    }

    private static string GroupName(AccountGroup group) => group switch
    {
        AccountGroup.Administrators => "Administrators",
        AccountGroup.Users => "Users",
        _ => "Users",
    };

    /// <summary>
    /// Build a <c>&lt;Password&gt;</c>-shaped element. When obscured, the plaintext is UTF-16LE encoded
    /// with the element name appended, then Base64'd — the documented Windows Setup convention.
    /// </summary>
    private static XElement BuildPassword(string elementName, string plaintext, bool obscure)
    {
        var value = obscure
            ? Convert.ToBase64String(Encoding.Unicode.GetBytes(plaintext + elementName))
            : plaintext;

        return new XElement(Unattend + elementName,
            new XElement(Unattend + "Value", value),
            new XElement(Unattend + "PlainText", obscure ? "false" : "true"));
    }

    // ─── Bloatware ──────────────────────────────────────────────────

    private void ApplyBloatware(Dictionary<string, XElement> passes, BloatwareSettings bloatware)
    {
        // Phase 3a: inject a single FirstLogonCommand that removes the selected AppX packages.
        // Phase 3b will replace this with a proper script slot + schneegans-parity removal logic.
        if (bloatware.AppsToRemove.Count == 0) return;

        var shell = GetOrAddComponent(passes["oobeSystem"], "Microsoft-Windows-Shell-Setup");
        var firstLogon = shell.Element(Unattend + "FirstLogonCommands") ?? AddChild(shell, "FirstLogonCommands");

        var packages = string.Join(",", bloatware.AppsToRemove.Select(a => $"'{a.Replace("'", "''")}'"));
        var ps = $"Get-AppxPackage -AllUsers | Where-Object {{ $_.Name -in @({packages}) }} | Remove-AppxPackage -AllUsers";
        var cmd = $"powershell.exe -NoProfile -ExecutionPolicy Bypass -Command \"{ps}\"";

        var order = firstLogon.Elements(Unattend + "SynchronousCommand").Count() + 1;
        firstLogon.Add(new XElement(Unattend + "SynchronousCommand",
            new XAttribute(Wcm + "action", "add"),
            new XElement(Unattend + "Order", order.ToString(_invariant)),
            new XElement(Unattend + "CommandLine", cmd),
            new XElement(Unattend + "Description", "Remove selected AppX bloatware")));
    }

    // ─── Utility ────────────────────────────────────────────────────

    private static XElement AddChild(XElement parent, string localName)
    {
        var child = new XElement(Unattend + localName);
        parent.Add(child);
        return child;
    }

    private static string Serialize(XDocument doc)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n",
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        };

        using var sw = new StringWriterWithEncoding(Encoding.UTF8);
        using (var xw = XmlWriter.Create(sw, settings))
        {
            doc.Save(xw);
        }
        return sw.ToString();
    }

    private sealed class StringWriterWithEncoding(Encoding encoding) : StringWriter
    {
        public override Encoding Encoding { get; } = encoding;
    }
}

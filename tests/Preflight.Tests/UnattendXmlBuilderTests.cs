using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Preflight.App.Models;
using Preflight.App.Services;

namespace Preflight.Tests;

public sealed class UnattendXmlBuilderTests
{
    private static readonly XNamespace Ns = "urn:schemas-microsoft-com:unattend";
    private static readonly XNamespace Wcm = "http://schemas.microsoft.com/WMIConfig/2002/State";

    private static XDocument Build(UnattendConfig config)
    {
        var xml = new UnattendXmlBuilder().Build(config);
        return XDocument.Parse(xml);
    }

    [Fact]
    public void Empty_config_produces_wellformed_unattend_root()
    {
        var doc = Build(new UnattendConfig());
        Assert.NotNull(doc.Root);
        Assert.Equal(Ns + "unattend", doc.Root!.Name);

        var passes = doc.Root.Elements(Ns + "settings")
            .Select(e => (string?)e.Attribute("pass"))
            .ToArray();

        Assert.Contains("windowsPE", passes);
        Assert.Contains("oobeSystem", passes);
        Assert.Equal(7, passes.Length);
    }

    [Fact]
    public void Region_emits_UI_and_locale_elements_in_expected_passes()
    {
        var config = new UnattendConfig();
        config.Region.DisplayLanguage = "uk-UA";
        config.Region.InputLanguage = "uk-UA";

        var doc = Build(config);

        var winPe = doc.Root!.Elements(Ns + "settings")
            .First(s => (string?)s.Attribute("pass") == "windowsPE")
            .Elements(Ns + "component")
            .First(c => (string?)c.Attribute("name") == "Microsoft-Windows-International-Core-WinPE");
        Assert.Equal("uk-UA", (string?)winPe.Element(Ns + "UILanguage"));

        var oobe = doc.Root.Elements(Ns + "settings")
            .First(s => (string?)s.Attribute("pass") == "oobeSystem")
            .Elements(Ns + "component")
            .First(c => (string?)c.Attribute("name") == "Microsoft-Windows-International-Core");
        // InputLocale is the keyboard layout ID in LCID:KeyboardId format, not a BCP-47 tag.
        Assert.Equal("0422:00000422", (string?)oobe.Element(Ns + "InputLocale"));
        Assert.Equal("uk-UA", (string?)oobe.Element(Ns + "UserLocale"));
    }

    [Fact]
    public void Edition_Pro_emits_the_published_GVLK()
    {
        var config = new UnattendConfig { Edition = { Edition = WindowsEdition.Pro, KeyMode = ProductKeyMode.Generic } };
        var doc = Build(config);

        var key = doc.Descendants(Ns + "ProductKey").Single().Element(Ns + "Key")?.Value;
        Assert.Equal("VK7JG-NPHTM-C97JM-9MPGT-3V66T", key);
    }

    [Fact]
    public void Edition_Interactive_emits_empty_key_with_ShowUI_Always()
    {
        var config = new UnattendConfig { Edition = { KeyMode = ProductKeyMode.Interactive } };
        var doc = Build(config);

        var productKey = doc.Descendants(Ns + "ProductKey").Single();
        // Interactive mode emits the generic dummy key so Windows Setup shows the key prompt.
        Assert.Equal("00000-00000-00000-00000-00000", productKey.Element(Ns + "Key")?.Value);
        Assert.Equal("Always", (string?)productKey.Element(Ns + "WillShowUI"));
    }

    [Fact]
    public void LocalAccount_emits_wcm_action_add_and_group()
    {
        var config = new UnattendConfig();
        config.Users.Add(new UserAccount { Name = "admin", Group = AccountGroup.Administrators, Password = "p" });

        var doc = Build(config);

        var account = doc.Descendants(Ns + "LocalAccount").Single();
        Assert.Equal("add", (string?)account.Attribute(Wcm + "action"));
        Assert.Equal("admin", (string?)account.Element(Ns + "Name"));
        Assert.Equal("Administrators", (string?)account.Element(Ns + "Group"));
    }

    [Fact]
    public void Obscured_password_is_Base64_of_UTF16LE_plaintext_plus_elementName()
    {
        var config = new UnattendConfig { FirstLogon = { ObscurePasswordsWithBase64 = true } };
        config.Users.Add(new UserAccount { Name = "u", Password = "hunter2", Group = AccountGroup.Administrators });

        var doc = Build(config);

        var passwordValue = doc.Descendants(Ns + "LocalAccount").Single()
            .Element(Ns + "Password")!
            .Element(Ns + "Value")!
            .Value;

        var expected = Convert.ToBase64String(Encoding.Unicode.GetBytes("hunter2" + "Password"));
        Assert.Equal(expected, passwordValue);

        var accountPassword = doc.Descendants(Ns + "LocalAccount").Single().Element(Ns + "Password")!;
        Assert.Equal("false", (string?)accountPassword.Element(Ns + "PlainText"));
    }

    [Fact]
    public void Plaintext_password_is_emitted_as_is()
    {
        var config = new UnattendConfig { FirstLogon = { ObscurePasswordsWithBase64 = false } };
        config.Users.Add(new UserAccount { Name = "u", Password = "pw", Group = AccountGroup.Administrators });

        var doc = Build(config);

        var accountPassword = doc.Descendants(Ns + "LocalAccount").Single().Element(Ns + "Password")!;
        Assert.Equal("pw", (string?)accountPassword.Element(Ns + "Value"));
        Assert.Equal("true", (string?)accountPassword.Element(Ns + "PlainText"));
    }

    [Fact]
    public void Bloatware_selections_embed_package_selectors_in_xml()
    {
        var config = new UnattendConfig();
        // Use catalog IDs ("Remove{DisplayName}"), not package selectors directly.
        config.Bloatware.AppsToRemove.Add("RemoveNews");     // selector: Microsoft.BingNews
        config.Bloatware.AppsToRemove.Add("RemoveXboxApps"); // selector: Microsoft.GamingApp

        var xml = new UnattendXmlBuilder().Build(config);

        // Package selectors are embedded in a PS1 file inside the Extensions section.
        Assert.Contains("Microsoft.BingNews", xml, StringComparison.Ordinal);
        Assert.Contains("Microsoft.GamingApp", xml, StringComparison.Ordinal);
        Assert.Contains("Remove-AppxProvisionedPackage", xml, StringComparison.Ordinal);
    }

    [Fact]
    public void Disk_AutoWipe_emits_pe_diskpart_script_per_partition_style()
    {
        // Upstream folded disk handling into a Windows PE .cmd (diskpart + dism /Apply-Image)
        // instead of an autounattend <DiskConfiguration>/<InstallTo>. The diskpart lines are
        // echoed into the PE script verbatim, so assert the layout markers in the raw XML.
        var gpt = new UnattendXmlBuilder().Build(
            new UnattendConfig { Disk = { Mode = DiskMode.AutoWipe, PartitionStyle = PartitionStyle.Gpt } });
        Assert.Contains("CONVERT GPT", gpt, StringComparison.Ordinal);
        Assert.Contains("CREATE PARTITION EFI", gpt, StringComparison.Ordinal);
        Assert.Contains("/Apply-Image", gpt, StringComparison.Ordinal);

        // MBR lays out a primary system partition with no GPT conversion or EFI partition.
        var mbr = new UnattendXmlBuilder().Build(
            new UnattendConfig { Disk = { Mode = DiskMode.AutoWipe, PartitionStyle = PartitionStyle.Mbr } });
        Assert.DoesNotContain("CONVERT GPT", mbr, StringComparison.Ordinal);
        Assert.DoesNotContain("CREATE PARTITION EFI", mbr, StringComparison.Ordinal);
        Assert.Contains("/Apply-Image", mbr, StringComparison.Ordinal);
    }

    [Fact]
    public void SourceImage_selection_drives_dism_apply_image_param()
    {
        // Install-from selection only takes effect on the generated-PE (AutoWipe) path,
        // where our .cmd runs dism /Apply-Image with the chosen index/name.
        var byIndex = new UnattendXmlBuilder().Build(new UnattendConfig
        {
            Disk = { Mode = DiskMode.AutoWipe },
            SourceImage = { Mode = SourceImageMode.ByIndex, ImageIndex = 4 },
        });
        Assert.Contains("/Index:4", byIndex, StringComparison.Ordinal);

        var byName = new UnattendXmlBuilder().Build(new UnattendConfig
        {
            Disk = { Mode = DiskMode.AutoWipe },
            SourceImage = { Mode = SourceImageMode.ByName, ImageName = "Windows 11 Pro" },
        });
        Assert.Contains("/Name:", byName, StringComparison.Ordinal);
        Assert.Contains("Windows 11 Pro", byName, StringComparison.Ordinal);
    }

    [Fact]
    public void SkipIntegrityCheck_toggles_dism_checkintegrity_flag()
    {
        UnattendConfig Cfg(bool skip) => new()
        {
            Disk = { Mode = DiskMode.AutoWipe },
            SourceImage = { Mode = SourceImageMode.ByIndex, ImageIndex = 1, SkipIntegrityCheck = skip },
        };

        // Default keeps the integrity check; enabling the toggle drops it.
        Assert.Contains("/CheckIntegrity", new UnattendXmlBuilder().Build(Cfg(false)), StringComparison.Ordinal);
        Assert.DoesNotContain("/CheckIntegrity", new UnattendXmlBuilder().Build(Cfg(true)), StringComparison.Ordinal);
    }

    [Fact]
    public void Full_config_validates_against_autounattend_xsd()
    {
        var config = new UnattendConfig
        {
            Region = { DisplayLanguage = "en-US", InputLanguage = "en-US", HomeLocation = "US" },
            Edition = { Edition = WindowsEdition.Pro, KeyMode = ProductKeyMode.Generic },
            Disk = { PartitionStyle = PartitionStyle.Gpt },
            FirstLogon = { ObscurePasswordsWithBase64 = true },
        };
        config.Users.Add(new UserAccount { Name = "admin", DisplayName = "Administrator", Password = "pass", Group = AccountGroup.Administrators });
        config.Bloatware.AppsToRemove.Add("Microsoft.BingNews");

        var xml = new UnattendXmlBuilder().Build(config);

        var schemas = new XmlSchemaSet();
        var xsdPath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "autounattend.xsd");
        schemas.Add("urn:schemas-microsoft-com:unattend", xsdPath);

        var errors = new List<string>();
        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = schemas,
        };
        settings.ValidationEventHandler += (_, e) =>
        {
            if (e.Severity == XmlSeverityType.Error) errors.Add(e.Message);
        };

        using var reader = XmlReader.Create(new StringReader(xml), settings);
        while (reader.Read()) { /* drain */ }

        Assert.Empty(errors);
    }
}

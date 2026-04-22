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
        Assert.Equal("uk-UA", (string?)oobe.Element(Ns + "InputLocale"));
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
        Assert.True(string.IsNullOrEmpty(productKey.Element(Ns + "Key")?.Value));
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
        config.Users.Add(new UserAccount { Name = "u", Password = "hunter2", Group = AccountGroup.Users });

        var doc = Build(config);

        var passwordValue = doc.Descendants(Ns + "LocalAccount").Single()
            .Element(Ns + "Password")!
            .Element(Ns + "Value")!
            .Value;

        var expected = Convert.ToBase64String(Encoding.Unicode.GetBytes("hunter2" + "Password"));
        Assert.Equal(expected, passwordValue);

        Assert.Equal("false", (string?)doc.Descendants(Ns + "Password").Single().Element(Ns + "PlainText"));
    }

    [Fact]
    public void Plaintext_password_is_emitted_as_is()
    {
        var config = new UnattendConfig { FirstLogon = { ObscurePasswordsWithBase64 = false } };
        config.Users.Add(new UserAccount { Name = "u", Password = "pw", Group = AccountGroup.Users });

        var doc = Build(config);

        Assert.Equal("pw", (string?)doc.Descendants(Ns + "Password").Single().Element(Ns + "Value"));
        Assert.Equal("true", (string?)doc.Descendants(Ns + "Password").Single().Element(Ns + "PlainText"));
    }

    [Fact]
    public void Bloatware_selections_emit_FirstLogonCommand()
    {
        var config = new UnattendConfig();
        config.Bloatware.AppsToRemove.Add("Microsoft.BingNews");
        config.Bloatware.AppsToRemove.Add("Microsoft.GamingApp");

        var doc = Build(config);

        var cmd = doc.Descendants(Ns + "SynchronousCommand").Single();
        Assert.Equal("1", (string?)cmd.Element(Ns + "Order"));
        var line = cmd.Element(Ns + "CommandLine")?.Value ?? "";
        Assert.Contains("Microsoft.BingNews", line, StringComparison.Ordinal);
        Assert.Contains("Microsoft.GamingApp", line, StringComparison.Ordinal);
        Assert.Contains("Remove-AppxPackage", line, StringComparison.Ordinal);
    }

    [Fact]
    public void Disk_GPT_targets_partition_3_MBR_targets_partition_1()
    {
        var gpt = Build(new UnattendConfig { Disk = { PartitionStyle = PartitionStyle.Gpt } });
        Assert.Equal("3", (string?)gpt.Descendants(Ns + "InstallTo").Single().Element(Ns + "PartitionID"));

        var mbr = Build(new UnattendConfig { Disk = { PartitionStyle = PartitionStyle.Mbr } });
        Assert.Equal("1", (string?)mbr.Descendants(Ns + "InstallTo").Single().Element(Ns + "PartitionID"));
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

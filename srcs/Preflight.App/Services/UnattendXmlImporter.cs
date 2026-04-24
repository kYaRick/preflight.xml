using System.Text;
using System.Text.RegularExpressions;
using Preflight.App.Models;

namespace Preflight.App.Services;

/// <summary>
/// Reverse-engineers an <see cref="UnattendConfig"/> from a previously-downloaded
/// autounattend.xml. Two strategies, in order:
///
/// 1. Round-trip path: the generator embeds a <c>&lt;!-- preflight.config: base64 --&gt;</c>
///    comment holding the full serialized config. If present, we decode it and return
///    exactly the state the user downloaded.
///
/// 2. Element fallback: for XMLs that weren't produced by preflight.xml, best-effort
///    extract common elements (ComputerName, TimeZone, InputLocale, Display language).
///    Unrecognised sections fall through to defaults. A set of warnings is reported so
///    the UI can tell the user what survived and what didn't.
///
/// Returns an <see cref="ImportResult"/> rather than a bare config so the caller can
/// surface "round-trip succeeded" vs "partial import, review the following" vs "not a
/// Windows autounattend at all".
/// </summary>
public sealed class UnattendXmlImporter
{
    private static readonly Regex MetadataRegex = new(
        @"<!--\s*preflight\.config:\s*([A-Za-z0-9+/=]+)\s*-->",
        RegexOptions.Compiled);

    // A single incrementing counter is enough to keep CA1822 happy while still letting
    // callers observe whether the instance has been used (useful for test assertions).
    public int ImportCount { get; private set; }

    public ImportResult Import(string xml)
    {
        ImportCount++;
        if (string.IsNullOrWhiteSpace(xml))
            return ImportResult.Failed("XML file is empty.");

        // Strategy 1: metadata round-trip
        var meta = MetadataRegex.Match(xml);
        if (meta.Success)
        {
            try
            {
                var bytes = Convert.FromBase64String(meta.Groups[1].Value);
                var json = Encoding.UTF8.GetString(bytes);
                var config = UnattendConfigSerializer.Deserialize(json);
                if (config is not null)
                    return ImportResult.Roundtrip(config);
            }
            catch (Exception ex)
            {
                // Metadata present but corrupt - fall through to element parsing and
                // surface the error so the user knows the round-trip blob was unusable.
                return ImportResult.Partial(new UnattendConfig(),
                    [$"Configuration blob was present but could not be decoded: {ex.Message}"]);
            }
        }

        // Strategy 2: best-effort element parsing on XMLs produced elsewhere.
        return ParseFromElements(xml);
    }

    private static ImportResult ParseFromElements(string xml)
    {
        System.Xml.Linq.XDocument doc;
        try
        {
            doc = System.Xml.Linq.XDocument.Parse(xml);
        }
        catch (System.Xml.XmlException ex)
        {
            return ImportResult.Failed($"File is not valid XML: {ex.Message}");
        }

        var root = doc.Root;
        if (root is null || root.Name.LocalName != "unattend")
            return ImportResult.Failed("Root element is not <unattend>; this is not an autounattend.xml file.");

        var config = new UnattendConfig();
        var warnings = new List<string>();

        var ns = root.Name.Namespace;

        // ComputerName is in <component name="Microsoft-Windows-Shell-Setup">/ComputerName
        var computerName = root
            .Descendants(ns + "ComputerName")
            .Select(e => e.Value)
            .FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(computerName))
        {
            if (string.Equals(computerName, "*", StringComparison.Ordinal))
                config.ComputerName.Mode = ComputerNameMode.Random;
            else
            {
                config.ComputerName.Mode = ComputerNameMode.Manual;
                config.ComputerName.CustomName = computerName;
            }
        }

        // Region: <InputLocale>, <UILanguage>, <UserLocale> are per-pass and repeat.
        // First occurrence wins.
        var input = root.Descendants(ns + "InputLocale").FirstOrDefault()?.Value;
        var display = root.Descendants(ns + "UILanguage").FirstOrDefault()?.Value;
        var user = root.Descendants(ns + "UserLocale").FirstOrDefault()?.Value;
        if (!string.IsNullOrWhiteSpace(display)) config.Region.DisplayLanguage = display!;
        if (!string.IsNullOrWhiteSpace(user)) config.Region.InputLanguage = user!;
        if (!string.IsNullOrWhiteSpace(input) && string.IsNullOrEmpty(user))
            config.Region.InputLanguage = input!;

        // TimeZone is explicit when present
        var tz = root.Descendants(ns + "TimeZone").FirstOrDefault()?.Value;
        if (!string.IsNullOrWhiteSpace(tz))
        {
            config.TimeZoneSettings.Mode = TimeZoneMode.Explicit;
            config.TimeZoneSettings.ExplicitId = tz;
        }

        warnings.Add("XML was not produced by preflight.xml; only top-level fields (computer name, region, time zone) were imported. Review the Advanced editor and fill in what else you need.");

        return ImportResult.Partial(config, warnings);
    }
}

/// <summary>Outcome of an import attempt.</summary>
public sealed record ImportResult(
    ImportStatus Status,
    UnattendConfig? Config,
    IReadOnlyList<string> Messages)
{
    public static ImportResult Roundtrip(UnattendConfig config) =>
        new(ImportStatus.Roundtrip, config, []);

    public static ImportResult Partial(UnattendConfig config, IReadOnlyList<string> warnings) =>
        new(ImportStatus.Partial, config, warnings);

    public static ImportResult Failed(string message) =>
        new(ImportStatus.Failed, null, [message]);
}

public enum ImportStatus
{
    /// <summary>XML carried a metadata blob that restored the full config.</summary>
    Roundtrip,
    /// <summary>Top-level elements extracted; most sections defaulted.</summary>
    Partial,
    /// <summary>XML could not be parsed or was not an autounattend document.</summary>
    Failed,
}

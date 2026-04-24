using System.Text.Json;
using System.Text.Json.Serialization;
using Preflight.App.Models;

namespace Preflight.App.Services;

/// <summary>
/// JSON round-trip helper for <see cref="UnattendConfig"/>. Used by the builder to embed
/// a base64-encoded config blob inside the generated XML and by <see cref="UnattendXmlImporter"/>
/// to reconstruct the config when the user re-uploads that XML.
///
/// Enums are written as strings so the blob is human-inspectable in the embedded comment
/// (useful when debugging preset / generator behaviour) and survives enum renames as long
/// as the member names themselves don't change.
/// </summary>
public static class UnattendConfigSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string Serialize(UnattendConfig config) =>
        JsonSerializer.Serialize(config, Options);

    public static UnattendConfig? Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonSerializer.Deserialize<UnattendConfig>(json, Options);
    }
}

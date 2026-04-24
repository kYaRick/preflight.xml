using Preflight.App.Models;
using Schneegans.Unattend;

namespace Preflight.App.Services;

/// <summary>
/// Thin read-only view over <see cref="UnattendGenerator.Bloatwares"/>. Centralising the
/// iteration here means the Bloatware section (and anything else that wants the list -
/// Docs, future preset builders) doesn't need to take a dependency on the whole generator.
///
/// Registered as a singleton; the generator behind <see cref="UnattendXmlBuilder"/> is the
/// same instance so the catalog and the XML mapper stay in lockstep.
/// </summary>
public sealed class BloatwareCatalog
{
    private readonly IReadOnlyList<OptionValue> _items;

    public BloatwareCatalog(UnattendXmlBuilder xmlBuilder)
    {
        ArgumentNullException.ThrowIfNull(xmlBuilder);
        // The generator instance lives inside UnattendXmlBuilder (private readonly). We
        // expose the catalog through a dedicated accessor so we don't promote the whole
        // field to public just for this one use case.
        var bloatwares = xmlBuilder.GetBloatwareCatalog();
        _items = bloatwares
            .OrderBy(b => b.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(b => new OptionValue(b.Id, b.DisplayName))
            .ToList();
    }

    /// <summary>Full catalog - each entry's <c>Value</c> is the bloatware <c>Id</c> (e.g. <c>RemoveCopilot</c>) and <c>DisplayKey</c> is the literal display name (not a resource key).</summary>
    public IReadOnlyList<OptionValue> Items => _items;
}

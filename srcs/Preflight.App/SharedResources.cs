namespace Preflight.App;

/// <summary>
/// Marker type used by <see cref="Microsoft.Extensions.Localization.IStringLocalizer{T}"/>
/// to resolve the shared resx catalogue under <c>Resources/SharedResources{.uk}.resx</c>.
///
/// Note: this class MUST sit at the project root namespace, not inside
/// <c>Preflight.App.Resources.*</c> - otherwise <see cref="Microsoft.Extensions.Localization.LocalizationOptions.ResourcesPath"/>
/// doubles up ("Resources/Resources/SharedResources.resx") and string lookups silently fall back to keys.
/// </summary>
public sealed class SharedResources
{
}

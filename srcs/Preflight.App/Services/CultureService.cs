using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Preflight.App.Services;

/// <summary>
/// Reads, persists and applies the preferred UI culture.
/// Supported cultures: <c>en</c>, <c>uk</c>. Fallback: <c>en</c>.
/// </summary>
public sealed class CultureService
{
    public static readonly IReadOnlyList<CultureInfo> Supported = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("uk"),
    };

    private readonly IJSRuntime _js;
    private readonly NavigationManager _nav;

    public CultureService(IJSRuntime js, NavigationManager nav)
    {
        _js = js;
        _nav = nav;
    }

    public static CultureInfo Current => CultureInfo.CurrentUICulture;

    /// <summary>Persist the chosen culture and force a reload so resources rebind.</summary>
    public async Task SetAsync(string cultureTag)
    {
        if (!Supported.Any(c => c.TwoLetterISOLanguageName == cultureTag))
            cultureTag = "en";

        await _js.InvokeVoidAsync("preflightCulture.set", cultureTag);
        _nav.NavigateTo(_nav.Uri, forceLoad: true);
    }
}

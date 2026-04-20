using System.Globalization;
using Preflight.App.Services;

namespace Preflight.Tests;

public sealed class CultureServiceTests
{
    [Fact]
    public void Supported_cultures_include_en_and_uk()
    {
        var tags = CultureService.Supported
            .Select(c => c.TwoLetterISOLanguageName)
            .ToArray();

        Assert.Contains("en", tags);
        Assert.Contains("uk", tags);
    }

    [Fact]
    public void Current_reflects_thread_ui_culture()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("uk");
            Assert.Equal("uk", CultureService.Current.TwoLetterISOLanguageName);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}

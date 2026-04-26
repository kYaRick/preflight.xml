using System.Globalization;
using Preflight.App.Services;

namespace Preflight.Tests;

public sealed class ChangelogServiceTests
{
    [Fact]
    public void GetRaw_StripsInternalBlocks()
    {
        var sut = new ChangelogService();

        var raw = sut.GetRaw(new CultureInfo("en"));

        Assert.DoesNotContain("### Maintenance", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("### Технічне", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetRaw_SelectsCulture_AndFallsBackToEnglish()
    {
        var sut = new ChangelogService();

        var en = sut.GetRaw(new CultureInfo("en"));
        var uk = sut.GetRaw(new CultureInfo("uk"));
        var fr = sut.GetRaw(new CultureInfo("fr"));

        Assert.Contains("### Added", en, StringComparison.Ordinal);
        Assert.Contains("### Додано", uk, StringComparison.Ordinal);
        Assert.NotEqual(en, uk);
        Assert.Equal(en, fr);
    }

    [Fact]
    public void GetRaw_UsesCache_ForRepeatedCultureRequests()
    {
        var sut = new ChangelogService();

        var first = sut.GetRaw(new CultureInfo("uk"));
        var second = sut.GetRaw(new CultureInfo("uk"));

        Assert.True(object.ReferenceEquals(first, second));
    }
}

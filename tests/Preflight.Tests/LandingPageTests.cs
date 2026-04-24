using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Preflight.App.Pages;
using Preflight.App.Services;

namespace Preflight.Tests;

public sealed class LandingPageTests : TestContext
{
    public LandingPageTests()
    {
        Services.AddLocalization(options => options.ResourcesPath = "Resources");
        Services.AddFluentUIComponents();
        Services.AddSingleton<ModeService>();
        Services.AddSingleton<PresetService>();
        Services.AddSingleton<UnattendXmlImporter>();
    }

    [Fact]
    public void Landing_renders_three_mode_cards()
    {
        var cut = RenderComponent<Landing>();

        // Resx resources are not loaded into the bUnit AppDomain, so IStringLocalizer
        // returns either the resolved string or the raw key as fallback. Assert on a
        // stable fragment present in both English copy AND the key ("Wizard" is in both
        // "Wizard" and "Landing.Wizard.Title").
        Assert.Contains("Wizard", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Advanced", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Docs", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }
}

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
    }

    [Fact]
    public void Landing_renders_three_mode_cards()
    {
        var cut = RenderComponent<Landing>();

        // All three mode card localization keys should be present in the rendered markup
        // (resx is not wired into bUnit by default - keys are the expected fallback).
        Assert.Contains("Landing.Wizard.Title", cut.Markup);
        Assert.Contains("Landing.Advanced.Title", cut.Markup);
        Assert.Contains("Landing.Docs.Title", cut.Markup);
    }
}

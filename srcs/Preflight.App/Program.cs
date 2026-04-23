using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Preflight.App;
using Preflight.App.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// 🌐 Localization - resources live under Resources/
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// 🎨 Fluent UI
builder.Services.AddFluentUIComponents();

// 🧭 App-level services
builder.Services.AddScoped<CultureService>();
builder.Services.AddSingleton<ModeService>();
builder.Services.AddSingleton<PresetService>();
builder.Services.AddSingleton<UnattendXmlBuilder>();
builder.Services.AddSingleton<BloatwareCatalog>();

var host = builder.Build();

// Apply the user's preferred culture before the app renders.
var js = host.Services.GetRequiredService<IJSRuntime>();
var cultureTag = await js.InvokeAsync<string>("preflightCulture.get");
var culture = new CultureInfo(cultureTag);
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await host.RunAsync();

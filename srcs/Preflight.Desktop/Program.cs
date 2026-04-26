using System;
using Velopack;

namespace Preflight.Desktop;

/// <summary>
/// Custom WPF entry point so Velopack's hook commands run before any UI is
/// constructed. The default WPF main (auto-generated from App.xaml's
/// ApplicationDefinition) creates the App instance immediately, which is
/// too late to handle Velopack's <c>--veloapp-install</c> /
/// <c>--veloapp-uninstall</c> / <c>--veloapp-updated</c> sub-invocations:
/// those need to fire and call Environment.Exit before any window opens
/// or the splash flickers on screen during install.
/// </summary>
public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Velopack: handle install / update / uninstall hooks BEFORE WPF spins up.
        // VelopackApp.Run() calls Environment.Exit(0) after the hook completes,
        // so the WPF App constructor below only runs on a normal user-launch.
        // No source / channel info is needed here - this part is a local
        // bookkeeping handshake; UpdateService owns the GitHub-side wiring.
        VelopackApp.Build()
            .Run();

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}

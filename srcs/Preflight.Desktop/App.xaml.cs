using System.Windows;

namespace Preflight.Desktop;

public partial class App : Application
{
    /// <summary>
    /// Background updater. Process-wide single instance so the splash and
    /// the main window can both observe the same UpdateReady event without
    /// firing duplicate downloads.
    /// </summary>
    public static UpdateService Updates { get; } = new();

    /// <summary>
    /// Single-window startup. The splash is no longer a separate window —
    /// it lives inside MainWindow as an overlay (see SplashOverlay in
    /// MainWindow.xaml). Title bar + frame are shared between the splash
    /// phase and the rendered-app phase, so the user sees one window
    /// throughout, just with the body content swapping from splash card
    /// to WebView once Blazor signals ready.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var main = new MainWindow();
        main.Show();

        Updates.StartBackgroundCheck();
    }
}

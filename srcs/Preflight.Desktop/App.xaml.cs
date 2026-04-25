using System.Windows;
using System.Windows.Media.Animation;

namespace Preflight.Desktop;

public partial class App : Application
{
    // Off-screen parking spot for the main window while the splash is up.
    // Far enough that no monitor in any reasonable multi-display setup will
    // place pixels here, and large negative coordinates avoid the rare
    // "auto-snap to nearest screen" behaviour seen with values near 0.
    private const double OffscreenX = -32000;
    private const double OffscreenY = -32000;

    /// <summary>
    /// Two-window startup. The main window is parked off-screen so its HWND
    /// exists for WebView2 to attach to, but the user can't see it. When the
    /// page posts <c>preflight:ready</c> the main window is recentered and
    /// shown the same frame the splash fades out — no overlap, no flash of
    /// empty chrome around the splash.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var splash = new SplashWindow();
        splash.Show();

        var main = new MainWindow
        {
            // Manual + offscreen overrides the XAML's CenterScreen until we
            // re-center on FirstPageRendered. Opacity=0 is a belt-and-braces
            // backup for the brief moment between Show() and the recenter
            // (without it, some Win11 builds flash the chrome at the parked
            // coordinates as the HWND materializes).
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = OffscreenX,
            Top = OffscreenY,
            Opacity = 0,
            ShowInTaskbar = false,
        };
        main.FirstPageRendered += (_, _) => Reveal(splash, main);
        main.Show();
    }

    private static void Reveal(SplashWindow splash, MainWindow main)
    {
        // Recenter on the work-area before making the window visible. Using
        // SystemParameters.WorkArea (not PrimaryScreenWidth/Height) keeps
        // the window off the taskbar even on bottom-docked taskbar setups.
        var work = SystemParameters.WorkArea;
        main.Left = work.Left + ((work.Width - main.ActualWidth) / 2);
        main.Top = work.Top + ((work.Height - main.ActualHeight) / 2);
        main.ShowInTaskbar = true;
        main.Opacity = 1;
        main.Activate();

        // Splash fades out alone (no cross-fade). The main window is already
        // at full opacity behind it, so by the time the splash is gone the
        // user sees the rendered page directly — no perceived overlap.
        // IsHitTestVisible=false prevents the still-on-screen-but-fading
        // splash from intercepting a click meant for the now-active main
        // window during the 180ms transition.
        splash.IsHitTestVisible = false;
        var splashFade = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = System.TimeSpan.FromMilliseconds(180),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut },
        };
        splashFade.Completed += (_, _) => splash.Close();
        splash.BeginAnimation(Window.OpacityProperty, splashFade);
    }
}

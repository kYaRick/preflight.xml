using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Preflight.Desktop;

public partial class App : Application
{
    /// <summary>
    /// Cross-fade duration for the splash → main hand-off. Both the splash's
    /// fade-out and the main window's fade-in run in parallel with this
    /// exact duration so the transition reads as a single synchronized
    /// dissolve, not two separate animations playing back-to-back.
    /// </summary>
    private static readonly TimeSpan HandoffDuration = TimeSpan.FromMilliseconds(240);

    /// <summary>
    /// Two-window startup, Visual-Studio style. The splash floats alone on
    /// the desktop while the main window provisions WebView2 and renders
    /// Blazor in the background. The main window IS shown so WebView2's
    /// HWND is on-screen (otherwise the renderer pauses), but starts with
    /// Opacity=0 + AllowsTransparency=True in XAML, so it's a completely
    /// transparent layered window - invisible to the user, but visible to
    /// WPF/WebView2's IsVisible machinery so painting continues.
    ///
    /// Why this combo (Opacity=0 + AllowsTransparency=True), not Visibility:
    ///   - Visibility=Hidden cascades to WebView2.IsVisible=false → renderer
    ///     pauses → page never finishes loading.
    ///   - Opacity alone (without AllowsTransparency=True) on a WindowChrome
    ///     window leaks chrome through the layered alpha - caption stays
    ///     visible. AllowsTransparency=True forces a true layered render
    ///     surface where Opacity=0 means truly invisible.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var splash = new SplashWindow();
        splash.Show();

        var main = new MainWindow
        {
            // Hide the empty taskbar entry while the splash is the active
            // visual; flip it back on once the window fades in.
            ShowInTaskbar = false,
        };
        main.FirstPageRendered += (_, _) => RevealMain(main, splash);
        main.Show();
    }

    /// <summary>
    /// Cross-fades splash out and main in over <see cref="HandoffDuration"/>
    /// in parallel, so the two transitions are synchronized. Setting Opacity
    /// instantly while the splash separately fades caused the user to see
    /// the main window "pop in" while the splash was still on screen - that
    /// looked like a desync. With the cross-fade, the splash dissolves into
    /// the main window in a single smooth motion.
    /// </summary>
    private static void RevealMain(MainWindow main, SplashWindow splash)
    {
        main.ShowInTaskbar = true;
        main.Activate();

        var ease = new SineEase { EasingMode = EasingMode.EaseInOut };

        main.BeginAnimation(Window.OpacityProperty, new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = HandoffDuration,
            EasingFunction = ease,
        });

        splash.FadeAndClose(HandoffDuration);
    }
}

using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Preflight.Desktop;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
        VersionText.Text = "v" + GetInformationalVersion();
        // Drag-to-move on the splash so a slow WebView2 spin-up doesn't pin
        // the splash in the wrong place. Click anywhere outside the buttons
        // (there are none) and drag.
        MouseLeftButtonDown += (_, _) =>
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        };
    }

    /// <summary>
    /// Pulled from AssemblyInformationalVersionAttribute (set by the SDK from
    /// Directory.Build.props's Version), so the splash always shows the same
    /// version as the README badge.
    /// </summary>
    private static string GetInformationalVersion()
    {
        var attr = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attr is null) return "0.0.0";
        var v = attr.InformationalVersion;
        var plus = v.IndexOf('+');
        return plus < 0 ? v : v[..plus];
    }

    /// <summary>
    /// Fades the splash out and closes it. Optional duration so the caller
    /// can match the cross-fade timing of the main window's fade-in (App
    /// passes the same duration to both windows so they stay synchronized,
    /// avoiding the "main pops in while splash still fading" desync).
    /// </summary>
    public void FadeAndClose(System.TimeSpan? duration = null)
    {
        // IsHitTestVisible=false prevents the still-on-screen-but-fading
        // splash from intercepting clicks meant for the now-active main
        // window during the transition.
        IsHitTestVisible = false;

        var fade = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = duration ?? System.TimeSpan.FromMilliseconds(240),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        };
        fade.Completed += (_, _) => Close();
        BeginAnimation(OpacityProperty, fade);
    }
}

using System.Reflection;
using System.Windows;
using System.Windows.Input;

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

    // Pulled from AssemblyInformationalVersionAttribute (set by the SDK from
    // <Version>$(VersionPrefix)-$(VersionSuffix)</Version> in Directory.Build.props),
    // so the splash always shows the same version as the README badge.
    private static string GetInformationalVersion()
    {
        var attr = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attr is null) return "0.0.0";

        // Strip the SourceLink "+<commit-sha>" suffix the SDK appends in CI builds.
        var v = attr.InformationalVersion;
        var plus = v.IndexOf('+');
        return plus < 0 ? v : v[..plus];
    }
}

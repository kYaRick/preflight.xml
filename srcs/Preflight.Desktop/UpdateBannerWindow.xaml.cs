using System;
using System.Windows;

namespace Preflight.Desktop;

/// <summary>
/// Transparent borderless overlay window that floats the update-ready banner
/// above WebView2. Because WebView2 is a Win32 HWND it clips any WPF visual
/// painted inside the same window, so we use a separate top-level window that
/// the OS compositor stacks on top.
/// </summary>
public partial class UpdateBannerWindow : Window
{
    public event EventHandler? DismissRequested;
    public event EventHandler? RestartRequested;

    public UpdateBannerWindow()
    {
        InitializeComponent();
    }

    public void SetContent(string title, string subtitle, string restartLabel, string dismissLabel)
    {
        TitleText.Text = title;
        SubtitleText.Text = subtitle;
        RestartText.Text = restartLabel;
        DismissText.Text = dismissLabel;
    }

    public void SetSubtitle(string subtitle) => SubtitleText.Text = subtitle;

    private void OnDismissClick(object sender, RoutedEventArgs e) => DismissRequested?.Invoke(this, EventArgs.Empty);
    private void OnRestartClick(object sender, RoutedEventArgs e) => RestartRequested?.Invoke(this, EventArgs.Empty);
}

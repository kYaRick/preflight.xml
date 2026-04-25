using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;

namespace Preflight.Desktop;

public partial class MainWindow : Window
{
    // Single-shot signal that the page has actually rendered (Blazor mounted,
    // loading overlay starting to dismiss). Triggered by a postMessage from
    // index.html — *not* by NavigationCompleted, which fires when the document
    // is loaded but Blazor hasn't booted yet, so swapping the splash on
    // NavigationCompleted made the empty main frame flash for ~2s.
    public event EventHandler? FirstPageRendered;
    private bool _firstPageSignaled;
    private DispatcherTimer? _readyTimeoutTimer;

    // Marker we append to the WebView2 user agent so the page can tell it's
    // running inside our shell. Service worker registration is skipped (and
    // any previously-installed SWs unregistered) when this is detected,
    // which is the fix for the "intermittent 404" — a SW from an older
    // build was occasionally serving a stale index.html that lacked the
    // /index.html → / URL normalizer, sending Blazor through to NotFound.
    private const string DesktopUserAgentTag = "Preflight-Desktop";

    private static readonly Geometry MaximizeGlyph =
        Geometry.Parse("M0,0 H10 V10 H0 Z");
    private static readonly Geometry RestoreGlyph =
        Geometry.Parse("M2,0 H10 V8 H8 V2 H2 Z M0,2 H8 V10 H0 Z");

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closed += OnClosed;
        StateChanged += OnStateChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Preflight", "WebView2");

        var env = await CoreWebView2Environment.CreateAsync(
            browserExecutableFolder: null,
            userDataFolder: userDataFolder);

        await WebView.EnsureCoreWebView2Async(env);

        var wwwrootPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "wwwroot");

        WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "preflight.app",
            wwwrootPath,
            CoreWebView2HostResourceAccessKind.Allow);

        var ua = WebView.CoreWebView2.Settings.UserAgent ?? string.Empty;
        if (!ua.Contains(DesktopUserAgentTag))
        {
            WebView.CoreWebView2.Settings.UserAgent = (ua + " " + DesktopUserAgentTag).Trim();
        }

        WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
        WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;

        WebView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
        WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

        // Navigate to the index file directly. SetVirtualHostNameToFolderMapping
        // does not auto-serve a directory index, so navigating to the bare host
        // would 404. The inline normalizer in index.html (history.replaceState)
        // strips "/index.html" before Blazor's router reads location, so the
        // route resolves to "/" and Landing renders.
        WebView.Source = new Uri("https://preflight.app/index.html");
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        // Show the WebView so the in-page loading overlay is visible behind
        // the splash. The splash itself stays up; we don't dismiss until
        // preflight:ready arrives (Blazor actually rendered).
        WebView.Visibility = Visibility.Visible;

        // Safety net: only arm AFTER the document has loaded — the previous
        // version started this on window-load and could fire before WebView2
        // even cold-started, dismissing the splash to an empty main window
        // for 5–15s while Blazor was still booting. 15s after document-load
        // is enough for WASM init + Blazor mount on a slow first run, and
        // short enough that a hung page doesn't leave the user stranded.
        if (_readyTimeoutTimer is null && !_firstPageSignaled)
        {
            _readyTimeoutTimer = new DispatcherTimer(
                TimeSpan.FromSeconds(15),
                DispatcherPriority.Normal,
                (_, _) => SignalFirstPageRendered(),
                Dispatcher);
        }
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string? msg = null;
        try { msg = e.TryGetWebMessageAsString(); }
        catch { /* non-string message — ignore */ }

        if (msg == "preflight:ready") SignalFirstPageRendered();
    }

    private void SignalFirstPageRendered()
    {
        if (_firstPageSignaled) return;
        _firstPageSignaled = true;
        _readyTimeoutTimer?.Stop();
        FirstPageRendered?.Invoke(this, EventArgs.Empty);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _readyTimeoutTimer?.Stop();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (MaxRestorePath is null) return;
        MaxRestorePath.Data = WindowState == WindowState.Maximized
            ? RestoreGlyph
            : MaximizeGlyph;

        // When maximized, WindowChrome leaves a thin border that pokes past
        // the work area; collapsing the border eliminates the artifact.
        RootBorder.BorderThickness = WindowState == WindowState.Maximized
            ? new Thickness(0)
            : new Thickness(1);
        MaxRestoreButton.ToolTip = WindowState == WindowState.Maximized
            ? "Restore"
            : "Maximize";
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void OnMaxRestoreClick(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();
}

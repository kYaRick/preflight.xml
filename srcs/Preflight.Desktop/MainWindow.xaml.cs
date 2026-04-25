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
    private DispatcherTimer? _readyPollTimer;

    // Marker we append to the WebView2 user agent so the page can tell it's
    // running inside our shell. Service worker registration is skipped (and
    // any previously-installed SWs unregistered) when this is detected,
    // which is the fix for the "intermittent 404" — a SW from an older
    // build was occasionally serving a stale index.html that lacked the
    // /index.html → / URL normalizer, sending Blazor through to NotFound.
    private const string DesktopUserAgentTag = "Preflight-Desktop";

    // Cached wwwroot path used by the WebResourceRequested handler that
    // serves index.html for bare-host requests.
    private string _wwwrootPath = string.Empty;

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

        _wwwrootPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "wwwroot");

        WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "preflight.app",
            _wwwrootPath,
            CoreWebView2HostResourceAccessKind.Allow);

        // Bare-host directory requests need explicit handling because
        // SetVirtualHostNameToFolderMapping doesn't serve a directory index.
        // The initial navigation targets /index.html (which works directly),
        // but anything that does a full reload of the current URL after our
        // /index.html → / normalizer has run lands on / and would otherwise
        // 404. Concretely: the language switcher calls
        // NavigationManager.NavigateTo(Uri, forceLoad: true), which used to
        // break in the desktop shell. Mapping / to index.html here keeps
        // that path working.
        WebView.CoreWebView2.AddWebResourceRequestedFilter(
            "https://preflight.app/",
            CoreWebView2WebResourceContext.Document);
        WebView.CoreWebView2.WebResourceRequested += OnRootResourceRequested;

        var ua = WebView.CoreWebView2.Settings.UserAgent ?? string.Empty;
        if (!ua.Contains(DesktopUserAgentTag))
        {
            WebView.CoreWebView2.Settings.UserAgent = (ua + " " + DesktopUserAgentTag).Trim();
        }

        WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
        WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;

        // Clear stale service-worker / cached-asset state from prior runs
        // BEFORE the first navigation. Without this, a SW left over from an
        // older build was occasionally serving mismatched cached HTML/WASM
        // bundles — the page would "load" (NavigationCompleted fires) but
        // Blazor never finished booting, leaving the user staring at the
        // dark WebView background after the splash dismissed. The desktop
        // host doesn't need offline caching (files are local via
        // SetVirtualHostNameToFolderMapping), so wiping these on every
        // launch is essentially free.
        try
        {
            await WebView.CoreWebView2.Profile.ClearBrowsingDataAsync(
                CoreWebView2BrowsingDataKinds.ServiceWorkers
                | CoreWebView2BrowsingDataKinds.CacheStorage
                | CoreWebView2BrowsingDataKinds.DiskCache);
        }
        catch
        {
            // ClearBrowsingDataAsync can throw on unsupported runtime versions;
            // failing it is non-fatal — the JS-side fallback still cleans up.
        }

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
        // either preflight:ready arrives or the DOM poll sees Blazor mount.
        WebView.Visibility = Visibility.Visible;

        if (_firstPageSignaled) return;

        // Two-track readiness detection so we never depend on a single signal:
        //   1. preflight:ready postMessage from the new index.html — fast path
        //   2. DOM poll for Blazor's .layout class — backup if a stale cached
        //      HTML missing the postMessage call ever gets served, or if any
        //      JS error swallows the postMessage before it reaches the host
        // Both feed the same SignalFirstPageRendered() which is idempotent.
        _readyPollTimer ??= new DispatcherTimer(
            TimeSpan.FromMilliseconds(400),
            DispatcherPriority.Background,
            OnReadyPoll,
            Dispatcher);

        // Hard fallback: 25s after document-load. Generous for a cold WebView2
        // + WASM boot on a slow disk; short enough that a genuinely broken
        // page doesn't leave the splash hanging forever.
        _readyTimeoutTimer ??= new DispatcherTimer(
            TimeSpan.FromSeconds(25),
            DispatcherPriority.Normal,
            (_, _) => SignalFirstPageRendered(),
            Dispatcher);
    }

    private async void OnReadyPoll(object? sender, EventArgs e)
    {
        if (_firstPageSignaled) return;
        try
        {
            var result = await WebView.CoreWebView2.ExecuteScriptAsync(
                "!!document.querySelector('.layout')");
            // ExecuteScriptAsync returns the JSON-encoded result; for a bool
            // that's the literal string "true" or "false".
            if (result == "true") SignalFirstPageRendered();
        }
        catch
        {
            // Mid-navigation script failures are expected — keep polling.
        }
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string? msg = null;
        try { msg = e.TryGetWebMessageAsString(); }
        catch { /* non-string message — ignore */ }

        if (msg == "preflight:ready") SignalFirstPageRendered();
    }

    private void OnRootResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
        if (e.Request.Uri != "https://preflight.app/") return;

        try
        {
            var indexPath = Path.Combine(_wwwrootPath, "index.html");
            // FileStream ownership transfers to WebView2 — it reads on a
            // background thread and disposes when finished. No using/await.
            var stream = File.OpenRead(indexPath);
            e.Response = WebView.CoreWebView2.Environment.CreateWebResourceResponse(
                stream, 200, "OK", "Content-Type: text/html; charset=utf-8");
        }
        catch
        {
            // Leave Response unset — WebView2 falls back to its default 404.
        }
    }

    private void SignalFirstPageRendered()
    {
        if (_firstPageSignaled) return;
        _firstPageSignaled = true;
        _readyPollTimer?.Stop();
        _readyTimeoutTimer?.Stop();
        FirstPageRendered?.Invoke(this, EventArgs.Empty);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _readyPollTimer?.Stop();
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

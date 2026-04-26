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
    // index.html - *not* by NavigationCompleted, which fires when the document
    // is loaded but Blazor hasn't booted yet, so swapping the splash on
    // NavigationCompleted made the empty main frame flash for ~2s.
    public event EventHandler? FirstPageRendered;
    private bool _firstPageSignaled;
    private DispatcherTimer? _readyTimeoutTimer;
    private DispatcherTimer? _readyPollTimer;

    // Marker we append to the WebView2 user agent so the page can tell it's
    // running inside our shell. Service worker registration is skipped (and
    // any previously-installed SWs unregistered) when this is detected,
    // which is the fix for the "intermittent 404" - a SW from an older
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
        var userDataFolder = ResolveUserDataFolder();

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

        // SPA fallback via NavigationStarting. We can't use WebResourceRequested
        // here: by design, that event does NOT fire for resources served by
        // SetVirtualHostNameToFolderMapping (Microsoft confirmed this in the
        // WebView2Feedback issue #2003) - so our previous handler was dead
        // code, and the language switcher's full-reload landed on the
        // "Hmmm… can't reach this page" page because nothing intercepted
        // bare-host or SPA-route requests.
        //
        // NavigationStarting fires for every top-level navigation, including
        // ones that target the virtual host. The handler cancels requests
        // for paths that don't map to a real file under wwwroot, then
        // re-navigates to /index.html - preserving the original SPA route
        // as a #__r= hash fragment so the in-page boot script can restore
        // the URL after Blazor mounts.
        WebView.CoreWebView2.NavigationStarting += OnNavigationStarting;

        // Strip the right-click "Reload / Inspect / View source" context menu
        // that WebView2 inherits from Edge. The desktop shell shouldn't expose
        // browser chrome to the user, and the menu also surfaces Edge-branded
        // entries that look out-of-place against our themed window.
        WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;

        var ua = WebView.CoreWebView2.Settings.UserAgent ?? string.Empty;
        if (!ua.Contains(DesktopUserAgentTag))
        {
            WebView.CoreWebView2.Settings.UserAgent = (ua + " " + DesktopUserAgentTag).Trim();
        }

        WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
        WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;

        // Clear ONLY service-worker registrations from prior runs. The desktop
        // host disables SW registration in index.html (PF_IS_DESKTOP branch),
        // but a SW left active from an older build can still intercept the
        // current navigation and serve a stale response - that was the "blank
        // dark page after splash" scenario. We deliberately do NOT clear
        // DiskCache / CacheStorage here: an earlier version of this code did,
        // which forced WebView2 to re-prime its HTTP cache on every launch
        // and made every start after the very first noticeably slower than
        // the first one. SW is the only piece that can poison the load.
        try
        {
            await WebView.CoreWebView2.Profile.ClearBrowsingDataAsync(
                CoreWebView2BrowsingDataKinds.ServiceWorkers);
        }
        catch
        {
            // ClearBrowsingDataAsync can throw on unsupported runtime versions;
            // failing it is non-fatal - the JS-side fallback still cleans up.
        }

        WebView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
        WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        // Mirror the document <title> into Window.Title so the taskbar entry
        // reflects whatever page Blazor's <PageTitle> currently has set
        // (Wizard, Docs, Advanced, …). The custom title-bar text stays as
        // the brand "preflight.xml".
        WebView.CoreWebView2.DocumentTitleChanged += (_, _) =>
        {
            var t = WebView.CoreWebView2.DocumentTitle;
            if (!string.IsNullOrEmpty(t)) Title = t;
        };

        // Navigate to the index file directly. SetVirtualHostNameToFolderMapping
        // does not auto-serve a directory index, so navigating to the bare host
        // would 404. The inline normalizer in index.html (history.replaceState)
        // strips "/index.html" before Blazor's router reads location, so the
        // route resolves to "/" and Landing renders.
        WebView.Source = new Uri("https://preflight.app/index.html");
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        // WebView is visible from the start (XAML), so we don't toggle
        // Visibility here anymore. We only arm the readiness timers.
        if (_firstPageSignaled) return;

        // Two-track readiness detection so we never depend on a single signal:
        //   1. preflight:ready postMessage from the new index.html - fast path
        //   2. DOM poll for Blazor's .layout class - backup if a stale cached
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
            // Defensive readiness check. Earlier versions returned true when
            // the overlay was simply missing - but it's also missing during
            // a brief window early in document parsing, which let the splash
            // dismiss before Blazor had even started rendering. Now we
            // require Blazor's `.layout` to be in the DOM (proves the SPA
            // actually mounted) AND the in-page overlay to either be
            // fading-out (is-done) or fully removed (post-fade cleanup).
            var script = "(function(){"
                + "var l=document.querySelector('.layout');"
                + "if(!l)return false;"
                + "var o=document.getElementById('loading-overlay');"
                + "return !o||o.classList.contains('is-done');"
                + "})()";
            var result = await WebView.CoreWebView2.ExecuteScriptAsync(script);
            if (result == "true") SignalFirstPageRendered();
        }
        catch
        {
            // Mid-navigation script failures are expected - keep polling.
        }
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string? msg = null;
        try { msg = e.TryGetWebMessageAsString(); }
        catch { /* non-string message - ignore */ }

        if (msg == "preflight:ready") SignalFirstPageRendered();
    }

    private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        // SPA fallback: cancel requests that don't map to a real file under
        // wwwroot, then redirect to /index.html. Bare-host paths reload
        // straight to /index.html (URL normalizer in index.html will strip
        // it back to "/"); deeper SPA routes (/wizard, /docs, …) carry the
        // original path as a #__r= hash so the boot script can restore it
        // before Blazor's router reads location.
        if (string.IsNullOrEmpty(e.Uri)) return;

        Uri uri;
        try { uri = new Uri(e.Uri); }
        catch { return; }

        if (!string.Equals(uri.Host, "preflight.app", StringComparison.OrdinalIgnoreCase))
            return;

        var pathTrim = uri.AbsolutePath.TrimStart('/');

        // Real file on disk? Let SetVirtualHostNameToFolderMapping serve it.
        if (pathTrim.Length > 0)
        {
            if (pathTrim.Contains("..", StringComparison.Ordinal)) return;
            var localPath = Path.Combine(
                _wwwrootPath,
                pathTrim.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(localPath)) return;
        }

        // Bare host or SPA route - cancel and redirect to /index.html.
        e.Cancel = true;

        string redirect;
        if (pathTrim.Length == 0)
        {
            // "/" or "" - clean redirect; the URL normalizer in index.html
            // will strip "/index.html" back to "/" once the page loads.
            redirect = "https://preflight.app/index.html" + uri.Query + uri.Fragment;
        }
        else
        {
            // SPA route - encode the original path so the boot script can
            // replaceState back to it after the document loads, before
            // Blazor's router reads window.location.
            var route = uri.AbsolutePath + uri.Query;
            redirect = "https://preflight.app/index.html#__r="
                + Uri.EscapeDataString(route);
        }

        // Defer to next dispatcher pass so the cancellation is observed
        // before the new navigation begins.
        Dispatcher.BeginInvoke(() => WebView.CoreWebView2.Navigate(redirect));
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

    /// <summary>
    /// Picks where WebView2 should keep its profile (cache, localStorage,
    /// cookies, IndexedDB). Portable-first: stash it next to the executable
    /// so zipping or copying the install folder takes the user's settings
    /// along - friendly to portable distribution and Blazor PWA installs.
    /// Falls back to %LocalAppData% only when the install folder is
    /// read-only (e.g. someone dropped the exe into Program Files), so a
    /// privileged install location still works without elevating.
    /// </summary>
    private static string ResolveUserDataFolder()
    {
        var portable = Path.Combine(AppContext.BaseDirectory, "data", "WebView2");
        try
        {
            Directory.CreateDirectory(portable);
            // Probe write access - Directory.CreateDirectory succeeds on
            // read-only targets too, so a real write is the only honest
            // capability check.
            var probe = Path.Combine(portable, ".write-probe");
            File.WriteAllBytes(probe, Array.Empty<byte>());
            File.Delete(probe);
            return portable;
        }
        catch
        {
            // Read-only install location - fall back to per-user state in
            // %LocalAppData%\Preflight\WebView2. The app still runs; the
            // user just won't get the portable behaviour.
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Preflight", "WebView2");
        }
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

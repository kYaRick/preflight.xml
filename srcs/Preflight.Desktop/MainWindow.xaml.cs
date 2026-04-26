using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;

namespace Preflight.Desktop;

public partial class MainWindow : Window
{
    private const double SplashWindowWidth = 580;
    private const double SplashWindowHeight = 320;
    private const double SplashCornerRadius = 18;
    private const double ReadyCornerRadius = 14;

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

    // Cached settings folder (sibling to the WebView2 user-data folder).
    // Last-used save / open directories are persisted here so the file
    // dialogs come back to where the user left off across launches. Uses
    // the same root as the portable WebView2 profile, so distributing the
    // app folder takes these preferences along.
    private string _settingsFolder = string.Empty;

    // Configured window geometry from XAML (the "ready" app frame).
    private readonly double _readyWindowWidth;
    private readonly double _readyWindowHeight;
    private readonly double _readyMinWidth;
    private readonly double _readyMinHeight;

    private static readonly Geometry MaximizeGlyph =
        Geometry.Parse("M0,0 H10 V10 H0 Z");
    private static readonly Geometry RestoreGlyph =
        Geometry.Parse("M2,0 H10 V8 H8 V2 H2 Z M0,2 H8 V10 H0 Z");

    public MainWindow()
    {
        InitializeComponent();

        _readyWindowWidth = Width;
        _readyWindowHeight = Height;
        _readyMinWidth = MinWidth;
        _readyMinHeight = MinHeight;
        EnterSplashWindowMode();

        SplashVersionText.Text = "v" + GetInformationalVersion();
        FirstPageRendered += (_, _) => DismissSplashOverlay();
        Loaded += OnLoaded;
        Closed += OnClosed;
        StateChanged += OnStateChanged;
        SizeChanged += (_, _) => RepositionBannerWindow();
        LocationChanged += (_, _) => RepositionBannerWindow();

        // Subscribe to the singleton updater. When a download completes the
        // service raises UpdateReady from a background thread; marshal back
        // to the dispatcher to show the overlay banner window.
        App.Updates.UpdateReady += OnUpdateReady;
    }

    // Overlay window that shows the update banner above WebView2.
    private UpdateBannerWindow? _bannerWindow;
    // Remembers the version string so the banner can be retranslated on lang change.
    private string? _updateReadyVersion;

    /// <summary>
    /// Shows the update-ready banner in a separate transparent overlay window
    /// so it always renders above WebView2's HWND airspace.
    /// </summary>
    private async void OnUpdateReady(object? sender, string version)
    {
        var op = Dispatcher.InvokeAsync(async () =>
        {
            var lang = await GetCultureAsync().ConfigureAwait(true);

            (string title, string subtitle, string restart, string later) = lang switch
            {
                "uk" => (
                    $"✈️ Готова нова версія {version}",
                    "Перезапустіть, щоб застосувати оновлення.",
                    "Перезапустити",
                    "Пізніше"),
                _ => (
                    $"✈️ Update {version} ready",
                    "Restart preflight.xml to apply the new build.",
                    "Restart now",
                    "Later"),
            };

            _updateReadyVersion = version;

            if (_bannerWindow is null)
            {
                _bannerWindow = new UpdateBannerWindow { Owner = this };
                _bannerWindow.DismissRequested += (_, _) => _bannerWindow.Hide();
                _bannerWindow.RestartRequested += OnBannerRestartRequested;
                // After the first layout pass we get real ActualWidth/Height.
                _bannerWindow.ContentRendered += (_, _) => RepositionBannerWindow();
            }

            _bannerWindow.SetContent(title, subtitle, restart, later);
            _bannerWindow.Show();
            // Show() triggers layout; reposition now that dimensions are known.
            RepositionBannerWindow();
        });

        await op.Task.Unwrap();
    }

    private void RetranslateBannerWindow(string lang)
    {
        if (_bannerWindow is null || _updateReadyVersion is null) return;

        (string title, string subtitle, string restart, string later) = lang switch
        {
            "uk" => (
                $"✈️ Готова нова версія {_updateReadyVersion}",
                "Перезапустіть, щоб застосувати оновлення.",
                "Перезапустити",
                "Пізніше"),
            _ => (
                $"✈️ Update {_updateReadyVersion} ready",
                "Restart preflight.xml to apply the new build.",
                "Restart now",
                "Later"),
        };

        _bannerWindow.SetContent(title, subtitle, restart, later);
    }

    private void RepositionBannerWindow()
    {
        if (_bannerWindow is null) return;

        // Force a layout pass so ActualWidth/Height are valid even if called
        // before the window has painted.
        _bannerWindow.UpdateLayout();
        var bw = _bannerWindow.ActualWidth > 0 ? _bannerWindow.ActualWidth : 400;
        var bh = _bannerWindow.ActualHeight > 0 ? _bannerWindow.ActualHeight : 70;

        const int margin = 14;
        _bannerWindow.Left = Left + (Width - bw) / 2;
        _bannerWindow.Top = Top + Height - bh - margin;
    }

    private async System.Threading.Tasks.Task<string> GetCultureAsync()
    {
        try
        {
            var raw = await WebView.CoreWebView2.ExecuteScriptAsync(
                "(window.preflightCulture && window.preflightCulture.get && window.preflightCulture.get()) || 'en'");
            return raw?.Trim('"') ?? "en";
        }
        catch
        {
            return "en";
        }
    }

    private async void OnBannerRestartRequested(object? sender, EventArgs e)
    {
        if (App.Updates.ApplyAndRestart())
        {
            Application.Current.Shutdown();
            return;
        }

        if (_bannerWindow is null) return;

        var lang = await GetCultureAsync().ConfigureAwait(true);
        _bannerWindow.SetSubtitle(App.Updates.IsDryRunEnabled
            ? lang switch
            {
                "uk" => "Тестовий режим: перезапуск та застосування оновлення пропущено.",
                _ => "Test mode: restart and update apply were skipped.",
            }
            : lang switch
            {
                "uk" => "Не вдалося застосувати оновлення. Спробуйте пізніше.",
                _ => "Could not apply the update. Please try again later.",
            });
    }

    /// <summary>
    /// Pulled from AssemblyInformationalVersionAttribute (set by the SDK from
    /// Directory.Build.props's Version), so the splash overlay always shows
    /// the same version string as the README badge.
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
    /// Dismiss the in-window splash overlay once Blazor has signalled ready.
    /// Sequence: stop the looping shimmer / plane-bob storyboards (cheap
    /// hygiene), flip WebView back to Visible so its renderer un-throttles
    /// and paints the settled DOM, then fade the overlay out and remove it
    /// from the layout. The fade runs on the overlay's Opacity; once
    /// Completed, Visibility=Collapsed pulls it out of hit-testing entirely.
    /// </summary>
    private void DismissSplashOverlay()
    {
        if (TryFindResource("SplashShimmerStoryboard") is Storyboard sh)
            sh.Stop(this);
        if (TryFindResource("SplashPlaneBobStoryboard") is Storyboard pb)
            pb.Stop(this);

        RestoreReadyWindowMode();

        // Keep startup visually clean: show desktop chrome only after the
        // app is ready and the splash starts dismissing.
        TitleBarRoot.Visibility = Visibility.Visible;

        // Un-throttle WebView2: from now on it paints, replacing the splash
        // overlay's surface with the rendered Blazor page in 1–2 frames.
        WebView.Visibility = Visibility.Visible;

        SplashOverlay.IsHitTestVisible = false;
        var fade = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(280),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut },
        };
        fade.Completed += (_, _) => SplashOverlay.Visibility = Visibility.Collapsed;
        SplashOverlay.BeginAnimation(OpacityProperty, fade);
    }

    private void EnterSplashWindowMode()
    {
        MinWidth = 0;
        MinHeight = 0;
        Width = SplashWindowWidth;
        Height = SplashWindowHeight;
        RootBorder.CornerRadius = new CornerRadius(SplashCornerRadius);
    }

    private void RestoreReadyWindowMode()
    {
        var centerX = Left + (ActualWidth > 0 ? ActualWidth : Width) / 2;
        var centerY = Top + (ActualHeight > 0 ? ActualHeight : Height) / 2;

        MinWidth = _readyMinWidth;
        MinHeight = _readyMinHeight;
        Width = _readyWindowWidth;
        Height = _readyWindowHeight;
        RootBorder.CornerRadius = new CornerRadius(ReadyCornerRadius);

        // Keep the resize transition anchored to the same center point.
        var workArea = SystemParameters.WorkArea;
        var targetLeft = centerX - (Width / 2);
        var targetTop = centerY - (Height / 2);
        var maxLeft = workArea.Right - Width;
        var maxTop = workArea.Bottom - Height;

        Left = Math.Min(Math.Max(targetLeft, workArea.Left), Math.Max(workArea.Left, maxLeft));
        Top = Math.Min(Math.Max(targetTop, workArea.Top), Math.Max(workArea.Top, maxTop));
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var userDataFolder = ResolveUserDataFolder();
        _settingsFolder = Path.GetDirectoryName(userDataFolder) ?? userDataFolder;

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

        // Patch preflightCulture.set so it also posts a message to the
        // desktop shell whenever the user switches language. The helper
        // object may be created after document-start, so retry briefly.
        await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("""
            (function () {
                function patchCultureSetter() {
                    const obj = window.preflightCulture;
                    if (!obj || typeof obj.set !== 'function' || obj.__pfCultureBridgePatched) {
                        return false;
                    }

                    const orig = obj.set.bind(obj);
                    obj.set = function (value) {
                        orig(value);
                        try {
                            window.chrome.webview.postMessage('preflight:culture:' + value);
                        } catch (_) {
                            // Ignore host-bridge errors.
                        }
                    };
                    obj.__pfCultureBridgePatched = true;
                    return true;
                }

                if (patchCultureSetter()) return;

                const timer = setInterval(function () {
                    if (patchCultureSetter()) {
                        clearInterval(timer);
                    }
                }, 100);

                setTimeout(function () { clearInterval(timer); }, 10000);
            })();
            """);
        // Intercept the WebView2/Edge default download bar so the user gets
        // a system-themed Save-As dialog (Win11's IFileDialog auto-themes
        // light/dark) instead of the in-page browser-style bar. The path
        // the user picks is persisted as the last-save directory so the
        // next save lands in the same place.
        WebView.CoreWebView2.DownloadStarting += OnDownloadStarting;
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
        if (msg is null) return;

        if (msg == "preflight:ready") { SignalFirstPageRendered(); return; }

        const string CulturePrefix = "preflight:culture:";
        if (msg.StartsWith(CulturePrefix, StringComparison.Ordinal))
        {
            var lang = msg[CulturePrefix.Length..];
            RetranslateBannerWindow(lang);
            return;
        }

        // RPC for the JS-side file picker. Format:
        //   file:open:<accept>:<correlation-id>
        // We respond via PostWebMessageAsString with:
        //   file:open:result:{"id":"...","name":"...","text":"..."}
        // Handled async so the OS dialog is modal to the main window
        // without blocking the WebView2 message loop.
        const string OpenPrefix = "file:open:";
        const string OpenResultPrefix = "file:open:result:";
        if (msg.StartsWith(OpenPrefix, StringComparison.Ordinal)
            && !msg.StartsWith(OpenResultPrefix, StringComparison.Ordinal))
        {
            var rest = msg[OpenPrefix.Length..];
            // The accept mask never contains a ':' (it's something like
            // ".xml" or ".json"), so a single split is unambiguous.
            var sep = rest.IndexOf(':');
            if (sep > 0)
            {
                var accept = rest[..sep];
                var requestId = rest[(sep + 1)..];
                _ = HandleFileOpenRequestAsync(accept, requestId);
            }
        }
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

    /// <summary>
    /// Buffer between "page is ready" (Blazor mounted, in-page overlay
    /// dismissed) and the splash starting its cross-fade. Without this, the
    /// splash hand-off lands right when theme.js's page-enter animation
    /// (translateY + scale + blur on .pf-page, ~550ms, plus staggered
    /// children up to ~660ms) is mid-flight - the user sees the splash
    /// dissolve into cards still flying into position. 500ms is enough to
    /// catch the bulk of the animation while still feeling responsive.
    /// </summary>
    private const int FirstPageRenderedSettleDelayMs = 500;

    private async void SignalFirstPageRendered()
    {
        if (_firstPageSignaled) return;
        _firstPageSignaled = true;
        _readyPollTimer?.Stop();
        _readyTimeoutTimer?.Stop();

        // Hold the splash up while the in-page page-enter animation settles,
        // so the cross-fade reveals fully-rendered content rather than
        // mid-flight cards.
        await Task.Delay(FirstPageRenderedSettleDelayMs).ConfigureAwait(true);

        FirstPageRendered?.Invoke(this, EventArgs.Empty);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _readyPollTimer?.Stop();
        _readyTimeoutTimer?.Stop();
        App.Updates.UpdateReady -= OnUpdateReady;
        _bannerWindow?.Close();
        _bannerWindow = null;
    }

    // ─── File save: WebView2 download interception ───────────────────────
    //
    // CoreWebView2.DownloadStarting fires for every initiated download.
    // Setting e.Handled = true suppresses the WebView2/Edge download bar
    // (the in-WebView strip at the bottom that looks out-of-place against
    // a desktop chrome). We then put up a Win11-themed Save-As dialog,
    // assign e.ResultFilePath to whatever the user chose, and let the
    // download proceed silently to that destination. The handler uses
    // e.GetDeferral() so the synchronous OS dialog doesn't make the WV2
    // message pump wait inside the event invocation.

    private async void OnDownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
    {
        var deferral = e.GetDeferral();
        try
        {
            e.Handled = true;

            var suggestedName = Path.GetFileName(e.ResultFilePath);
            var ext = Path.GetExtension(suggestedName);
            var initialDir = ReadLastDir(LastSaveDirFile);
            var title = await GetLocalizedTitleAsync("save").ConfigureAwait(true);

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = suggestedName,
                Title = title,
                InitialDirectory = initialDir,
                AddExtension = true,
                OverwritePrompt = true,
            };
            if (!string.IsNullOrEmpty(ext))
            {
                var label = ext.TrimStart('.').ToUpperInvariant();
                dialog.Filter = $"{label} (*{ext})|*{ext}|All files (*.*)|*.*";
                dialog.DefaultExt = ext;
            }

            if (dialog.ShowDialog(this) == true)
            {
                e.ResultFilePath = dialog.FileName;
                WriteLastDir(LastSaveDirFile, Path.GetDirectoryName(dialog.FileName));
            }
            else
            {
                e.Cancel = true;
            }
        }
        finally
        {
            deferral.Complete();
        }
    }

    // ─── File open: postMessage RPC from JS ──────────────────────────────
    //
    // files.js (PF_IS_DESKTOP branch) posts "file:open:<accept>:<id>" when
    // Blazor calls preflightFiles.pickText. We reply with the file's text
    // payload via PostWebMessageAsString. Tied together by correlation id
    // so multiple in-flight pickers don't cross.

    private async Task HandleFileOpenRequestAsync(string accept, string requestId)
    {
        var initialDir = ReadLastDir(LastOpenDirFile);
        var title = await GetLocalizedTitleAsync("open").ConfigureAwait(true);

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = title,
            InitialDirectory = initialDir,
            CheckFileExists = true,
            Multiselect = false,
        };
        if (!string.IsNullOrEmpty(accept) && accept.StartsWith('.'))
        {
            var label = accept.TrimStart('.').ToUpperInvariant();
            dialog.Filter = $"{label} (*{accept})|*{accept}|All files (*.*)|*.*";
            dialog.DefaultExt = accept;
        }

        string? name = null;
        string? text = null;
        if (dialog.ShowDialog(this) == true)
        {
            try
            {
                text = await File.ReadAllTextAsync(dialog.FileName).ConfigureAwait(true);
                name = Path.GetFileName(dialog.FileName);
                WriteLastDir(LastOpenDirFile, Path.GetDirectoryName(dialog.FileName));
            }
            catch
            {
                // Read failure - reply with null payload so the JS promise
                // resolves null (matches user-cancel semantics on the JS side).
            }
        }

        var payload = JsonSerializer.Serialize(new { id = requestId, name, text });
        try
        {
            WebView.CoreWebView2.PostWebMessageAsString("file:open:result:" + payload);
        }
        catch
        {
            // CoreWebView2 may have been disposed during the dialog if the
            // user closed the window - nothing to do.
        }
    }

    // ─── Localization helper ──────────────────────────────────────────────
    //
    // Reads the persisted UI culture from JS (preflightCulture.get reads
    // localStorage["preflight.culture"]) and returns a localized title for
    // the given dialog kind. Falls back to English on any error so the
    // dialog still has a sane title even if the JS bridge is unavailable.

    private async Task<string> GetLocalizedTitleAsync(string kind)
    {
        try
        {
            var raw = await WebView.CoreWebView2.ExecuteScriptAsync(
                "(window.preflightCulture && window.preflightCulture.get && window.preflightCulture.get()) || 'en'");
            var lang = raw?.Trim('"') ?? "en";
            return (kind, lang) switch
            {
                ("save", "uk") => "Зберегти файл як…",
                ("open", "uk") => "Відкрити файл",
                ("save", _) => "Save file as…",
                ("open", _) => "Open file",
                _ => "preflight.xml",
            };
        }
        catch
        {
            return kind == "save" ? "Save file as…" : "Open file";
        }
    }

    // ─── Last-directory persistence ───────────────────────────────────────

    private string LastSaveDirFile => Path.Combine(_settingsFolder, "last-save-dir.txt");
    private string LastOpenDirFile => Path.Combine(_settingsFolder, "last-open-dir.txt");

    private static string ReadLastDir(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var dir = File.ReadAllText(path).Trim();
                if (Directory.Exists(dir)) return dir;
            }
        }
        catch { /* ignore */ }
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    private static void WriteLastDir(string path, string? dir)
    {
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, dir);
        }
        catch { /* ignore */ }
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
            // %LocalAppData%\preflight.xml\WebView2. The app still runs; the
            // user just won't get the portable behaviour.
            // Folder name matches the Velopack packId so a future
            // managed install (Setup.exe) and the portable fallback share
            // one user-data directory.
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "preflight.xml", "WebView2");
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
        RootBorder.CornerRadius = WindowState == WindowState.Maximized
            ? new CornerRadius(0)
            : new CornerRadius(ReadyCornerRadius);
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

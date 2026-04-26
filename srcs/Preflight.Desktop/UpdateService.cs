using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace Preflight.Desktop;

/// <summary>
/// Polls GitHub Releases for a newer version of preflight.xml in the
/// channel matching this build, downloads it silently in the background,
/// and raises <see cref="UpdateReady"/> when a relaunch will apply it.
/// </summary>
/// <remarks>
/// <para>
/// <b>Channel selection.</b> Pre-1.0 builds ride the <c>alpha</c> channel
/// (Directory.Build.props exposes <c>0.1.x-alpha</c>). When the project
/// stabilises, switching pre-release semantics is a single string change
/// here plus the matching <c>--channel</c> in the release pipeline. The
/// channel string lives at one place on purpose - app and pipeline must
/// agree or <c>RELEASES-&lt;channel&gt;</c> won't be found.
/// </para>
/// <para>
/// <b>Quietness.</b> Network unreachable, repo missing, no newer release,
/// download failure - all are non-fatal. The desktop is offline-capable
/// (it embeds the PWA), so an update probe must never block startup or
/// surface an error popup. Failures log to debug output and the service
/// silently does nothing.
/// </para>
/// <para>
/// <b>Portable-build awareness.</b> When the user launched a fresh
/// <c>Portable.zip</c> extracted to a folder Velopack can't manage
/// (e.g. read-only path, no <c>Update.exe</c>), <see cref="UpdateManager.IsInstalled"/>
/// is false and the update flow is skipped entirely. The PWA shell still
/// works; the user just upgrades by re-downloading the next portable zip.
/// </para>
/// </remarks>
public sealed partial class UpdateService
{
    private const string RepoUrl = "https://github.com/kYaRick/preflight.xml";

    /// <summary>
    /// Channel name that <c>vpk pack --channel</c> uses on the publishing
    /// side. Must match exactly or no updates will be discovered.
    /// </summary>
    private const string Channel = "alpha";

    /// <summary>
    /// Cool-down before the first probe so the WebView2 boot has the
    /// network all to itself. WebView2 first-paint ~2-4s on cold disks,
    /// so 8s gives a comfortable buffer.
    /// </summary>
    private static readonly TimeSpan StartupGrace = TimeSpan.FromSeconds(8);

    private readonly UpdateManager _manager;
    private bool _started;

    public bool IsDryRunEnabled
    {
        get
        {
            var enabled = false;
            TryGetDryRunEnabled(ref enabled);
            return enabled;
        }
    }

    /// <summary>The version that has been downloaded and is ready to apply.</summary>
    public string? PendingVersion { get; private set; }

    /// <summary>
    /// Fires when a new version is downloaded and ready.
    /// May be raised from a background thread; UI subscribers must marshal
    /// back to the dispatcher before touching controls.
    /// </summary>
    public event EventHandler<string>? UpdateReady;

    public UpdateService()
    {
        // GithubSource also accepts a PAT for private repos. This repo is
        // public, so we pass null and rely on anonymous Releases API access.
        // PreRelease=true is how Velopack discovers any non-stable channel
        // (including the alpha we publish into).
        var source = new GithubSource(RepoUrl, accessToken: null, prerelease: true);
        _manager = new UpdateManager(source, new UpdateOptions
        {
            ExplicitChannel = Channel,
        });

        ConfigureUpdateTestHooks();
    }

    /// <summary>Idempotently kicks off the background update check.</summary>
    public void StartBackgroundCheck()
    {
        if (_started) return;
        _started = true;

        Task? simulationTask = null;
        TryGetUpdateTestSimulationTask(ref simulationTask);
        if (simulationTask is not null)
        {
            _ = simulationTask;
            return;
        }

        // Fire-and-forget. We capture exceptions inside the loop so the
        // task scheduler never sees an unobserved task exception.
        _ = Task.Run(RunOnceAsync);
    }

    /// <summary>
    /// Triggers a relaunch into the freshly-installed bits. Must be called
    /// after <see cref="UpdateReady"/> has fired - calling it without a
    /// downloaded update is a no-op (Velopack throws which we swallow).
    /// </summary>
    public bool ApplyAndRestart()
    {
        var skipApply = false;
        OnDryRunApplyRequested(ref skipApply);
        if (skipApply) return false;

        try
        {
            _manager.WaitExitThenApplyUpdates(null);
            // Velopack dispatches the relaunch; the caller still needs to
            // shut its UI down so Update.exe can replace the binaries
            // without a file lock conflict.
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[UpdateService] ApplyAndRestart failed: {ex.Message}");
            return false;
        }
    }

    partial void ConfigureUpdateTestHooks();
    partial void TryGetUpdateTestSimulationTask(ref Task? simulationTask);
    partial void OnDryRunApplyRequested(ref bool skipApply);
    partial void TryGetDryRunEnabled(ref bool enabled);

    private async Task RunOnceAsync()
    {
        try
        {
            await Task.Delay(StartupGrace).ConfigureAwait(false);

            // IsInstalled is false in dev (running under a debugger from
            // bin/Debug) and for portable extractions where Update.exe is
            // missing. Either way, there's nothing for us to do.
            if (!_manager.IsInstalled)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[UpdateService] Not a managed install - skipping update check.");
                return;
            }

            var update = await _manager.CheckForUpdatesAsync().ConfigureAwait(false);
            if (update is null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[UpdateService] No update available.");
                return;
            }

            await _manager.DownloadUpdatesAsync(update).ConfigureAwait(false);

            PendingVersion = update.TargetFullRelease.Version.ToString();
            UpdateReady?.Invoke(this, PendingVersion);
            System.Diagnostics.Debug.WriteLine(
                $"[UpdateService] Update {PendingVersion} downloaded - awaiting restart.");
        }
        catch (Exception ex)
        {
            // Network / GitHub-rate-limit / corrupt RELEASES file - all
            // non-fatal. The PWA shell keeps working with the bits the
            // user already has; we'll try again on next launch.
            System.Diagnostics.Debug.WriteLine(
                $"[UpdateService] Background check failed: {ex.Message}");
        }
    }
}

#if UPDATE_TEST
using System;
using System.Threading.Tasks;

namespace Preflight.Desktop;

public sealed partial class UpdateService
{
    private bool _testMode;
    private bool _dryRun;
    private string _testVersion = "9.9.9-test";
    private int _testDelayMs = 1800;

    private const string TestModeEnv = "PREFLIGHT_DESKTOP_UPDATE_TEST_MODE";
    private const string TestVersionEnv = "PREFLIGHT_DESKTOP_UPDATE_TEST_VERSION";
    private const string TestDelayMsEnv = "PREFLIGHT_DESKTOP_UPDATE_TEST_DELAY_MS";
    private const string DryRunEnv = "PREFLIGHT_DESKTOP_UPDATE_DRY_RUN";

    partial void ConfigureUpdateTestHooks()
    {
        _testMode = GetEnvBool(TestModeEnv);
        _dryRun = GetEnvBool(DryRunEnv);
        _testVersion = Environment.GetEnvironmentVariable(TestVersionEnv)
            ?? _testVersion;
        _testDelayMs = GetEnvInt(TestDelayMsEnv, _testDelayMs);
    }

    partial void TryGetUpdateTestSimulationTask(ref Task? simulationTask)
    {
        if (!_testMode) return;
        simulationTask = Task.Run(RunSimulatedAsync);
    }

    partial void OnDryRunApplyRequested(ref bool skipApply)
    {
        if (!_dryRun) return;

        System.Diagnostics.Debug.WriteLine(
            "[UpdateService] Dry-run mode: restart/apply skipped.");
        skipApply = true;
    }

    partial void TryGetDryRunEnabled(ref bool enabled)
    {
        enabled = _dryRun;
    }

    private async Task RunSimulatedAsync()
    {
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(_testDelayMs)).ConfigureAwait(false);
            PendingVersion = _testVersion;
            UpdateReady?.Invoke(this, _testVersion);
            System.Diagnostics.Debug.WriteLine(
                $"[UpdateService] TEST MODE: simulated update '{_testVersion}'.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[UpdateService] TEST MODE failed: {ex.Message}");
        }
    }

    private static bool GetEnvBool(string name)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(raw)) return false;

        return raw.Equals("1", StringComparison.OrdinalIgnoreCase)
            || raw.Equals("true", StringComparison.OrdinalIgnoreCase)
            || raw.Equals("yes", StringComparison.OrdinalIgnoreCase)
            || raw.Equals("on", StringComparison.OrdinalIgnoreCase);
    }

    private static int GetEnvInt(string name, int fallback)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        return int.TryParse(raw, out var value) && value >= 0
            ? value
            : fallback;
    }
}
#endif

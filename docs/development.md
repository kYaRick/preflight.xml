# Development Guide

This document describes development workflows, testing modes, and local setup for working on preflight.xml.

## Quick Start

```bash
# Clone and restore
git clone https://github.com/kYaRick/preflight.xml.git
cd preflight.xml
dotnet restore

# Build and run
dotnet build
dotnet run --project srcs/Preflight.Desktop

# Run tests
dotnet test
```

---

## Testing Features

### Update Service Testing

The desktop app includes **optional compile-time feature flags** for testing the update flow without modifying the live GitHub release channel. These are **Debug-only** - they don't ship in Release builds.

#### Enabling Update Test Mode

1. **In Visual Studio / VS Code:**
   - Select the launch profile **"Preflight.Desktop Update Test"** from the Run dropdown.
   - Press F5 or Run > Start Debugging.

2. **From command line:**
   ```bash
   dotnet run --project srcs/Preflight.Desktop \
     --configuration Debug \
     --launch-profile "Preflight.Desktop Update Test"
   ```

#### How It Works

When the Update Test profile is active:

- **After ~1.2 seconds**, an "update ready" banner appears at the bottom of the window.
- The banner shows version `0.1.2-alpha-test` and localized copy about the available update.
- Clicking "Restart now" will show a message: `"Test mode: restart and update apply were skipped."` instead of actually restarting.
  - This allows you to test the UI flow without closing the app.
  - See [OnBannerRestartRequested](../srcs/Preflight.Desktop/MainWindow.xaml.cs) for the dry-run logic.

#### Environment Variables

You can customize the test behavior via environment variables (only read in Debug builds):

| Variable | Default | Purpose |
|----------|---------|---------|
| `PREFLIGHT_DESKTOP_UPDATE_TEST_MODE` | (not set) | Set to `1`, `true`, `yes`, or `on` to enable simulated update. |
| `PREFLIGHT_DESKTOP_UPDATE_TEST_VERSION` | `9.9.9-test` | Version string to display in the update banner. |
| `PREFLIGHT_DESKTOP_UPDATE_TEST_DELAY_MS` | `1800` | Milliseconds to wait before triggering UpdateReady (default: 1.8s). |
| `PREFLIGHT_DESKTOP_UPDATE_DRY_RUN` | (not set) | Set to `1`, `true`, `yes`, or `on` to prevent actual restart/apply. |

**Example:** To test the banner appearing instantly with a custom version:

```bash
$env:PREFLIGHT_DESKTOP_UPDATE_TEST_MODE="1"
$env:PREFLIGHT_DESKTOP_UPDATE_TEST_VERSION="0.2.0-beta"
$env:PREFLIGHT_DESKTOP_UPDATE_TEST_DELAY_MS="0"
$env:PREFLIGHT_DESKTOP_UPDATE_DRY_RUN="1"

dotnet run --project srcs/Preflight.Desktop --configuration Debug
```

### Feature Flag Compilation

The update testing code lives in a separate partial file: [UpdateService.UpdateTest.cs](../srcs/Preflight.Desktop/UpdateService.UpdateTest.cs), wrapped in `#if UPDATE_TEST`.

**To disable the feature:**

Edit [Preflight.Desktop.csproj](../srcs/Preflight.Desktop/Preflight.Desktop.csproj) and comment out the UPDATE_TEST PropertyGroup:

```xml
<!--
    UPDATE_TEST: Enable update service testing mode.
    Allows simulating UpdateReady, dry-run restart, and env-based test configuration.
    Only compiled in Debug builds. Remove this PropertyGroup to disable.
-->
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
    <DefineConstants>$(DefineConstants);UPDATE_TEST</DefineConstants>
</PropertyGroup>
```

Then rebuild:
```bash
dotnet clean
dotnet build --configuration Debug
```

---

## Architecture Notes

### Desktop Shell

The desktop app is a pure .NET 10 WPF + WebView2 shell:
- **No Node/npm:** Pure C#/XAML only.
- **No backend:** The Blazor WASM PWA (from `srcs/Preflight.App`) is published at build time and served by WebView2 via virtual host mapping.
- **Offline-capable:** The entire PWA is bundled and runs client-side.

### Splash & Loading

- The startup splash is **in-window** (not a separate window), created as an overlay in [MainWindow.xaml](../srcs/Preflight.Desktop/MainWindow.xaml).
- Splash size adapts to compact mode on launch (680x420), then resizes to full dimensions (1080x900) once Blazor is ready.
- Rounded corners and styling use Fluent Design tokens from App.xaml.

### Update Flow

1. **UpdateService** polls GitHub Releases in the background (8s after startup).
2. If a newer version is found and downloaded, `UpdateReady` event fires.
3. **MainWindow** listens to this event and shows the update banner.
4. Clicking "Restart now" calls `UpdateService.ApplyAndRestart()`, which uses Velopack to apply the update and relaunch.
   - In **dry-run mode** (test), it skips the restart and shows a debug message instead.

---

## Project Structure

```
srcs/Preflight.Desktop/
  ├── App.xaml / App.xaml.cs          # WPF app entry
  ├── Program.cs                      # Velopack hooks + STAThread Main
  ├── MainWindow.xaml / .cs           # Main window, title bar, splash, update banner
  ├── UpdateService.cs                # Update polling service (production code)
  ├── UpdateService.UpdateTest.cs     # Test hooks (#if UPDATE_TEST only)
  └── wwwroot/                        # Blazor WASM PWA files (generated at build time)
```

---

## Building & Publishing

### Debug
```bash
dotnet build --configuration Debug
```

### Release (self-contained)
```bash
dotnet publish --configuration Release \
  --runtime win-x64 \
  --self-contained
```

See [publishing.md](./publishing.md) for Velopack bundling and release channel setup.

---

## Troubleshooting

**Q: The update banner doesn't appear in debug mode.**  
A: Ensure the "Preflight.Desktop Update Test" launch profile is selected. Check that `PREFLIGHT_DESKTOP_UPDATE_TEST_MODE=1` is set.

**Q: Clicking "Restart now" closes the app (not dry-run).**  
A: Check if `PREFLIGHT_DESKTOP_UPDATE_DRY_RUN=1` is set. Without it, the real Velopack update flow runs.

**Q: Compile error about unused test fields in Release.**  
A: This shouldn't happen - if it does, ensure `UPDATE_TEST` is only defined for Debug. Run `dotnet clean` and rebuild.

---

## See Also

- [docs/architecture.md](./architecture.md) - System design overview
- [docs/ci-cd.md](./ci-cd.md) - CI/CD pipeline & GitHub Actions
- [srcs/Preflight.Desktop/UpdateService.cs](../srcs/Preflight.Desktop/UpdateService.cs) - Update service implementation

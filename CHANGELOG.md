<!--
  Authoring guide: docs/changelog.md (Keep a Changelog 1.1.0).
  Wrap chore / CI / refactor entries in `internal:start` ... `internal:end`
  HTML-comment markers - they show on GitHub but are stripped from the
  in-app "What's new" modal.
-->

<!-- internal:start -->
# Changelog

All notable changes to **preflight.xml** are documented here. Format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project adheres
to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
<!-- internal:end -->

## [Unreleased]

### Added

- View Transitions API for SPA navigation - Landing → Wizard / Advanced / Docs
  and wizard step changes now crossfade smoothly with no flicker. Falls back
  silently to instant navigation in browsers that don't implement the spec.
- "What's new" modal - embedded CHANGELOG.md rendered inline, sharing the
  same overlay chrome as the command palette and shortcuts help.
- Custom XML import modal - drag-and-drop zone, paste pad, and a "Browse"
  fallback to the OS picker, all in one styled dialog instead of the
  bare-browser file dialog.
- Windows desktop auto-update (Velopack, alpha channel) - the desktop
  shell quietly checks GitHub Releases for newer alpha builds in the
  background, downloads the delta, and offers a one-click "Restart now"
  banner inside the app. No installer, no admin rights - portable zip
  in, portable update out.

### Changed

- Loading bar no longer overflows on narrow phones - the SVG runway scales
  uniformly to fit any viewport, with adequate edge margins.
- Trail under the loading-screen plane now draws ONLY behind the plane,
  with a soft fade-in at the runway start.
- "Go to top" plane button rebuilt - solid accent gradient, larger glyph,
  tactile press feedback, and a longer custom-eased scroll so the trip
  back to the top reads as deliberate motion instead of an instant jump.
- Command palette + Shortcuts help modal now animate open AND close (the
  old open-only animation made dismissal feel abrupt).
- Modals are now centred vertically in the viewport instead of pinned
  to 15vh from the top - large modals like the changelog no longer feel
  like they're falling off the page.
- Advanced XML preview modal now fades in/out with the same chrome family
  as the other modals (previously it snapped open with no transition).

### Fixed

- Wizard final-step action buttons no longer overlap on mobile - the
  action stacks now wrap and the page bottom padding clears any floating
  control.
- Ukrainian translation: "Підкрутити в Advanced" → "Доналаштувати в Advanced".
- Ukrainian "What's new" modal now actually renders the UA changelog -
  MSBuild was routing CHANGELOG.uk.md to a satellite assembly because of
  the .uk. infix, so the lookup always fell back to the EN file.
- Import XML modal now blurs the page header (and everything else behind
  it) - the modal was rendered inside the body grid, which trapped its
  backdrop-filter inside a stacking context that sat below the sticky
  header.

<!-- internal:start -->
### Maintenance

- Refactored ModeCard to render its content inside an `<a href>` so SPA
  navigation flows through the View Transitions click interceptor instead
  of programmatic Nav.NavigateTo (which bypassed it).
- Added `ChangelogService` with regex-based internal-block filter and
  per-culture resource lookup.
- Embedded CHANGELOG.{en,uk}.md as compile-time resources via
  `<EmbeddedResource LogicalName="..." WithCulture="false" />` in
  Preflight.App.csproj. WithCulture="false" stops MSBuild from treating
  the .uk. infix as a satellite-resource culture marker.
- Moved the changelog authoring guide out of the file's own header into
  `docs/changelog.md` so the source file isn't 60 lines of HTML comment
  before the actual content.
- Promoted the import XML modal to a top-level layout slot via
  `ImportModalService` so its overlay sits above the FluentLayout
  stacking context.
- Switched Preflight.Desktop's AssemblyName from `Preflight` to
  `preflight.xml` so the executable, Velopack PackId and live PWA name
  all match. The published exe is now `preflight.xml.exe`; Windows hides
  the `.exe` in the UI so the user sees `preflight.xml`.
- Custom WPF entry point (`Program.Main`) so `VelopackApp.Build().Run()`
  intercepts hook commands (`--veloapp-install` / `--veloapp-uninstall`
  / `--veloapp-updated`) before any window is constructed.
- Added `UpdateService` with `GithubSource` + `ExplicitChannel="alpha"`,
  fired from `App.OnStartup` after an 8s grace period. Failures are
  swallowed - desktop runs offline-first; missing updates are not an
  error condition.
- New justfile recipes `desktop-publish`, `desktop-pack`,
  `desktop-release` wrap the dotnet publish + `vpk pack --noInst`
  (portable-only) + `vpk upload github --merge --pre` flow.
- Extended `release.yml` with a `desktop` job on `windows-latest` that
  runs after the PWA package job, producing
  `preflight.xml-alpha-Portable.zip` + `RELEASES-alpha` + the matching
  full/delta nupkg files, and merging them into the same draft GitHub
  Release the PWA job already created.
- Split the Blazor wwwroot copy in Preflight.Desktop.csproj into
  `CopyBlazorToOutDir` (post-Build) + `CopyBlazorToPublishDir`
  (post-Publish) so `dotnet publish --output …` ships the PWA inside
  the bundle - vpk pack used to ship a desktop folder with no wwwroot.
<!-- internal:end -->

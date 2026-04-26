<div align="center">

# 📝 Changelog authoring guide

<sub>How to edit <code>CHANGELOG.md</code> / <code>CHANGELOG.uk.md</code> so the in-app modal, the GitHub release notes, and the rendered file on GitHub all stay coherent.</sub>

</div>

> [!IMPORTANT]
> The changelog files at the repo root are the **single source of truth** for:
> - The "What's new" modal in the running app (web + desktop)
> - GitHub Release notes (auto-published from these files by CI on tag push)
> - The repo's CHANGELOG view on GitHub

Format follows [Keep a Changelog 1.1.0](https://keepachangelog.com).

---

## ✏️ What to write

Each released version gets a `## [X.Y.Z] - YYYY-MM-DD` heading. Inside it,
group entries under H3 sections in this order (skip ones that don't apply):

| Section          | When to use                                       |
| :--------------- | :------------------------------------------------ |
| `### Added`      | New features visible to the user                  |
| `### Changed`    | Behaviour or visual changes to existing features  |
| `### Fixed`      | Bug fixes                                         |
| `### Removed`    | Features taken out                                |
| `### Deprecated` | Features that still work but will be removed soon |
| `### Security`   | Vulnerability or hardening fixes                  |

## 🙈 Hide developer-only entries

End users don't need to read about CI tweaks, dependency bumps, refactors
with no visible effect, or maintenance chores. Wrap those in HTML-comment
markers:

```markdown
<!-- internal:start -->
### Maintenance
- Bumped Foo to v2.x.y
- Refactored ChangelogService for testability
<!-- internal:end -->
```

Anything between those markers is:

- ✅ visible on GitHub (HTML comments collapse in the renderer, but the
  block content still shows in raw view and PR diffs)
- 🚫 stripped before the in-app modal renders (see
  `srcs/Preflight.App/Services/ChangelogService.cs`)

The same markers also hide the file's H1 title and intro paragraph from
the modal - the modal already renders its own localised title, so the
duplicated markdown heading would only add visual noise. Keep the title
block wrapped in markers in **every** language file.

Use this for: chores, build/CI, internal refactors, dependency updates
(unless the bump fixed a user-visible bug - then mention the fix, not
the version bump).

## 🗣️ Voice & tone

- One bullet = one user-visible change. Don't bundle.
- Lead with the noun (`Loading bar no longer overflows...`, not `We fixed...`).
- Past tense for fixes, present for added/changed.
- Link to the issue/PR when it adds context: `(#123)`.
- Keep it short. The modal is a teaser; the full discussion lives on GitHub.

## 🌐 Translations

`CHANGELOG.uk.md` mirrors `CHANGELOG.md` in Ukrainian. When you add an entry
in EN, add the equivalent in UK - or accept that UA users will see the EN
fallback for that release.

> [!NOTE]
> Files named `CHANGELOG.<lang>.md` look like culture-specific satellite
> resources to MSBuild. The `.csproj` overrides this with
> `WithCulture="false"` so they end up in the main assembly where
> `ChangelogService` looks for them.

<sub>← Back to the [docs index](README.md).</sub>

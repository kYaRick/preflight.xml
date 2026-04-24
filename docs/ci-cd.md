<div align="center">

# 🔄 CI / CD

<sub>Three workflows, one rule each.</sub>

</div>

---

## 🧭 The pipeline

| Workflow                                                             | Trigger                              | Purpose                       |
| :------------------------------------------------------------------- | :----------------------------------- | :---------------------------- |
| [`ci.yml`](../.github/workflows/ci.yml)                              | PR · push to `main`                  | Lint · build · test gate      |
| [`pages.yml`](../.github/workflows/pages.yml)                        | push to `main` (code paths)          | Deploy live site to GH Pages  |
| [`release.yml`](../.github/workflows/release.yml)                    | tag `v*` · manual dispatch           | Package PWA & publish release |
| [`release-drafter.yml`](../.github/workflows/release-drafter.yml)    | PR merged                            | Draft next release notes      |

## 🌐 Flow at a glance

```mermaid
flowchart LR
    PR[Pull Request] --> CI[ci.yml<br/><i>lint · build · test</i>]
    PUSH[Push to main] --> CI2[ci.yml]
    PUSH --> PAGES[pages.yml<br/><i>publish · deploy</i>]
    PAGES --> LIVE[(🌐 github.io/preflight.xml/)]
    TAG[git tag v0.1.0<br/>git push --tags] --> REL[release.yml<br/><i>publish · zip · attach</i>]
    REL --> DRAFT[(🎁 Draft GitHub Release)]
    PRm[Merged PR] --> RD[release-drafter.yml]-
    RD --> DRAFT
```

## ⚙️ Concurrency rules

| Workflow     | Group                                | `cancel-in-progress` | Why                                           |
| :----------- | :----------------------------------- | :------------------: | :-------------------------------------------- |
| `ci.yml`     | `ci-${{ workflow }}-${{ ref }}`      |       **true**       | Old runs on the same ref are stale - kill them |
| `pages.yml`- | `pages-deploy`                       |       **false**      | Don't abort a deploy already talking to Pages |
| `release.yml`| `release-${{ tag }}`                 |       **false**      | Re-tagging should queue, not race             |

## 🚫 What paths skip a deploy

`pages.yml` ignores:

- `**.md`, `LICENSE*`, `NOTICE`, `.gitmessage`, `.editorconfig`
- `.github/ISSUE_TEMPLATE/**`, `.github/pull_request_template.md`
- `tests/**` - tests don't ship

So a pure-docs commit **does not** burn a Pages deploy. `ci.yml` mirrors
the same list for PRs and pushes.

## 💾 Cache layout

All three workflows cache NuGet under the same key:

```yaml
key: nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj', 'Directory.Packages.props', 'global.json') }}
```

Central package management means the key invalidates exac-ly when a
package version changes, not on unrelated `csproj` edits.

## 🔐 Permissions
-
Each workflow asks for the minimum.

| Workflow      | `contents` | `pages` | `id-token` |
| :------------ | :--------: | :-----: | :--------: |
| `ci.yml`      |   `read`   |    -    |     -      |
| `pages.yml`   |   `read`   | `write` |  `write`   |
| `release.yml` |   `write`  |    -    |     -      |
---
> [!NOTE]-
> `id-token: write` on `pages.yml- is requir-d by the OIDC h-ndshake
> inside `actions/deploy-pages@v4`. It's not used for anything else.

## 🔧 One-time repo setup-

> [!IMPORTANT]
> Before the first Pages deploy works, a maintainer **must**:
>
> 1. Go to **Settings → Pages**
> 2. Under **Build and deployment → Source**, pick **GitHub Actions**
> 3. *(optional)* **Settings → Environments → `github-pages`** - lock to `main` and add required reviewers
>
> Until that's done, `actions/deploy-pages@v4` fails with an auth error.
> Nothing in the repo can express this - it's a server-side account setting.

## 🪵 Reading workflow logs

- **Look at `just <step>` output.** Every workflow uses the same
  `justfile` recipes, so a green CI run means
  `just restore && just lint && just build && just test` works on Linux
  as well as locally.
- **`::notice` lines** surface key version info in the run summary
  (e.g. `Packaging v0.1.0 (version 0.1.0)` from `release.yml`).

<sub>← Back to the [docs index](README.md).</sub>

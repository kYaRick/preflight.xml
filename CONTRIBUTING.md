# 🤝 Contributing to preflight.xml

First off - thanks for taking the time to contribute! 🎉

This project is **in pre-alpha bootstrap phase**. The Blazor application itself has not yet been scaffolded, so contribution surface is currently limited to tooling, documentation, and repository setup. Once the app lands, this guide will expand.

---

## 📋 Ground rules

- Be respectful. This is a small, friendly project.
- Keep PRs **focused and small** - one logical change per PR.
- **Open an issue first** if the change is larger than a quick fix - it avoids wasted effort on both sides.

---

## 🛠️ Development setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [`just`](https://github.com/casey/just) - command runner (`winget install Casey.Just` / `brew install just` / `cargo install just`)

### Bootstrap

```bash
git clone https://github.com/kYaRick/preflight.xml.git
cd preflight.xml
just init      # 🧩 configures git hooks + restores packages
```

### Everyday commands

| Command         | Purpose                                   |
| --------------- | ----------------------------------------- |
| `just`          | 📋 list all available recipes             |
| `just run`      | 🚀 run the app (hot-reload via `watch`)   |
| `just build`    | 🏗️ build all projects                     |
| `just test`     | 🧪 run tests                              |
| `just format`   | 🎨 auto-format source                     |
| `just lint`     | ✅ verify formatting - no file changes    |
| `just publish`  | 📦 produce a release build                |
| `just clean`    | 🧹 wipe build artifacts                   |

---

## 🌿 Branching

- `main` - always deployable, protected
- Feature branches - `<type>/<short-kebab-summary>`, e.g. `feat/disk-editor`, `fix/navmenu-active-state`

---

## ✍️ Commit style

We use [Conventional Commits](https://www.conventionalcommits.org/). The template is already in `.gitmessage` - set it once:

```bash
git config commit.template .gitmessage
```

Types: `feat` · `fix` · `docs` · `style` · `refactor` · `perf` · `test` · `build` · `ci` · `chore` · `revert`

Scopes: `ui` · `xml` · `pwa` · `core` · `ci` · `docs` · `deps` · `config`

**Example:** `feat(xml): add disk partitioning editor`

---

## 🔀 Pull requests

1. Fork → branch → commit → push → open PR against `main`
2. Fill out the PR template
3. Link the related issue (`Closes #123`)
4. Ensure CI is green
5. Request review

Reviewers may ask for squash-merge with a clean conventional-commit title.

---

## 🧭 Code style

- `.editorconfig` is the source of truth - your IDE will pick it up automatically.
- Warnings are treated as errors. Fix them, don't suppress.
- Prefer Fluent UI Blazor components over custom HTML/CSS.
- No inline styles or hardcoded hex colors - use Fluent Design Tokens.

---

## 🧪 Tests

Once the test project is scaffolded, new features require unit or bUnit component tests. Bug fixes should include a regression test.

---

## 🐛 Reporting bugs / requesting features

Use the **[Issues tab](https://github.com/kYaRick/preflight.xml/issues/new/choose)** - templates are provided for bugs, feature requests, and missing options.

For questions and open-ended discussion, use **[Discussions](https://github.com/kYaRick/preflight.xml/discussions)**.

---

## 🔐 Security

Please report vulnerabilities privately - see [SECURITY.md](SECURITY.md).

# 🤖 AGENTS.md - preflight.xml

Brief for any AI coding assistant (Claude, Copilot, Cursor, Codex, etc.) working in this repo.

---

## 🎯 Project

**preflight.xml** - a Blazor WebAssembly PWA that generates `autounattend.xml` answer files for unattended Windows 10 / 11 setup. Runs fully client-side, hosted on GitHub Pages, installable offline.

**Audience:** IT admins, sysadmins, homelabbers, DevOps.

**Status:** 🟧 Pre-alpha. Blazor WASM shell scaffolded (Phase 2a). UX architecture pending (Phase 2b).

**Reference implementation:** https://schneegans.de/windows/unattend-generator/ - the canonical prior art. Its C# source (MIT) is vendored into `srcs/Preflight.Unattend/` and compiled as the XML generation engine; a narrative structural spec lives at `.ai/reference/schneegans-spec.md` (local / gitignored). Consult both before scaffolding UI or choosing option coverage.

---

## 🧱 Stack

- **.NET 10** · `Nullable` + `ImplicitUsings` enabled · warnings as errors
- **Blazor WebAssembly** (PWA, no server)
- **Microsoft Fluent UI Blazor** for all UI
- **GitHub Pages** hosting (static)
- Central package management via `Directory.Packages.props`

---

## 📁 Folder layout (planned)

```
preflight.xml/
├── srcs/
│   └── Preflight.App/        # Blazor WASM PWA project
├── tests/
│   └── Preflight.Tests/      # bUnit + xUnit test project
├── .github/                  # CI, templates, automation
├── .editorconfig
├── Directory.Build.props
├── Directory.Packages.props
├── LICENSE
└── README.md
```

---

## 📐 Conventions

### Naming
- **Types:** PascalCase - `XmlGeneratorService`, `DiskOption`
- **Locals / params:** camelCase - `selectedLanguage`, `partitionSize`
- **Services:** suffix with `Service`
- **Components:** one `.razor` file per component, named after what it renders
- **C# / Razor files:** PascalCase · **Config / docs:** kebab-case

### Code style
- Enforced by `.editorconfig`
- `var` when type is apparent
- `System.*` usings first
- No `this.` field qualification

### Commits
- [Conventional Commits](https://www.conventionalcommits.org/) - see [`.gitmessage`](.gitmessage)
- `git config commit.template .gitmessage` on clone

---

## ✅ Do

- Use Fluent UI components (`FluentButton`, `FluentTextField`, `FluentDataGrid`, …)
- Use Fluent Design Tokens for all styling (colors, spacing, typography)
- Keep `.razor` files thin - put logic in services / code-behind
- Add NuGet versions to `Directory.Packages.props`, reference without `Version` in `.csproj`
- Preserve PWA offline capability

## ❌ Don't

- Add `npm`, `node`, `yarn`, or any JS-tooling - this is pure .NET
- Inline styles or hardcoded hex colors - use design tokens
- Put business logic in `.razor` files
- Pin package versions directly in `.csproj`
- Push directly to `main` without CI passing

---

## 🔐 Scope guardrails

- **Don't** add backend / server-side anything - all XML generation happens client-side
- **Don't** introduce PowerShell / Bash build scripts - use `dotnet` CLI or GitHub Actions
- **Don't** scaffold files for features not listed under [Planned features](README.md#-planned-features)
- **Do** ask the maintainer before adding new top-level dependencies

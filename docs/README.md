<div align="center">

# 📚 Docs

<sub>Living reference for <code>preflight.xml</code> - read what you need, skip what you don't.</sub>

</div>

> [!NOTE]
> Every page is deliberately short. If a topic needs a 1000-line essay,
> it probably belongs upstream in .NET / Blazor / Pages docs, not here.

---

## 🗺️ Find your page

|                  I want to…                   | Go to                                          |
| :-------------------------------------------- | :--------------------------------------------- |
| Understand the module layout                  | [architecture.md](architecture.md)             |
| Know what CI will run on my PR                | [ci-cd.md](ci-cd.md)                           |
| Deploy a new build to the live site           | [publishing.md](publishing.md)                 |
| Ship a versioned release & self-host archive  | [releasing.md](releasing.md)                   |
| Resync vendored `Preflight.Unattend`          | [upstream-sync.md](upstream-sync.md)           |

## 🎨 Conventions

- **Diagrams** use [Mermaid](https://mermaid.js.org/) - GitHub renders them inline.
- **Callouts** use GitHub's `[!NOTE]` / `[!TIP]` / `[!IMPORTANT]` / `[!WARNING]` / `[!CAUTION]` alerts.
- **Code samples** are copy-paste ready. Shell is `bash` unless stated; assume `pwd` is the repo root.
- **Paths** are always relative to the repo root (e.g. `srcs/Preflight.App/`).

## 🧭 Source of truth

| Question                          | Ask…                                                           |
| :-------------------------------- | :------------------------------------------------------------- |
| "How does this component work?"   | The source code - these docs don't duplicate API surfaces      |
| "When / why did this change?"     | `git log` / commit messages / linked issues                    |
| "Is this doc out of date?"        | The code wins - please open a PR to fix the doc                |

## 🤝 Contributing to docs

Open a PR against the relevant `docs/*.md`. Keep edits **short, truthful,
and current** - if you add a paragraph, check whether an older one
became redundant and delete it in the same commit.

<sub>← Back to the [main README](../README.md).</sub>

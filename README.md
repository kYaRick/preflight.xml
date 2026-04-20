<div align="center">

# ✈️ preflight.xml

**Visual builder for Windows `autounattend.xml` - in your browser, offline, instantly.**

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Blazor WASM](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor&logoColor=white)](https://learn.microsoft.com/aspnet/core/blazor/)
[![Fluent UI](https://img.shields.io/badge/Fluent%20UI-Blazor-0078D4?logo=microsoft&logoColor=white)](https://www.fluentui-blazor.net/)
[![PWA](https://img.shields.io/badge/PWA-offline--ready-5A0FC8?logo=pwa&logoColor=white)](https://web.dev/progressive-web-apps/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Status](https://img.shields.io/badge/status-pre--alpha-orange)]()

</div>

---

## 🧭 What is this?

`preflight.xml` is a **zero-install, browser-based** editor that helps IT admins, sysadmins, and homelabbers craft `autounattend.xml` answer files for unattended Windows 10 / 11 installations - **without touching raw XML**.

> 📖 About the file format - see the [Unattended Windows Setup Reference](https://learn.microsoft.com/en-us/windows-hardware/customize/desktop/unattend/) from Microsoft.

---

## 🎯 Planned features

- 🖼️  Visual editor for every major `autounattend.xml` pass & component
- 👀 Live XML preview with syntax highlighting
- 💽 Disk layout configurator (GPT / MBR, EFI, Recovery)
- 👤 User accounts & credential management
- 🔒 Privacy, telemetry & security toggles
- 🧼 Bloatware removal & app provisioning
- 📜 Custom PowerShell / CMD script injection
- 📥 Import / export of saved configurations
- 📴 Full offline support via PWA
- 🌍 i18n-ready UI

---

## 🧱 Tech stack

| Layer        | Choice                                    |
| ------------ | ----------------------------------------- |
| Runtime      | .NET 10 · Blazor WebAssembly              |
| UI           | Microsoft Fluent UI Blazor                |
| Hosting      | GitHub Pages (static, no backend)         |
| Distribution | Installable PWA, fully offline-capable    |

---

## 🚀 Getting started

> ⚠️ The project is in the **pre-alpha bootstrap phase** - the Blazor application has not yet been scaffolded.

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [`just`](https://github.com/casey/just) - the command runner (one-time install)
  ```bash
  winget install Casey.Just     # Windows
  brew install just             # macOS
  cargo install just            # any platform
  ```

### Clone & bootstrap

```bash
git clone https://github.com/kYaRick/preflight.xml.git
cd preflight.xml
just init        # 🧩 configures git hooks and restores packages
```

### Everyday commands

Run `just` with no arguments to see the full list. Most common ones:

| Command         | What it does                                  |
| --------------- | --------------------------------------------- |
| `just init`     | 🧩 one-time onboarding setup                  |
| `just run`      | 🚀 start the app with hot-reload              |
| `just build`    | 🏗️ build all projects                         |
| `just test`     | 🧪 run the test suite                         |
| `just format`   | 🎨 auto-format code                           |
| `just lint`     | ✅ verify formatting (CI-friendly)            |
| `just publish`  | 📦 produce a release build for GitHub Pages   |
| `just clean`    | 🧹 remove all build artifacts                 |

> 💡 Prefer raw `dotnet` commands? The `justfile` is short and readable - every recipe is just a thin wrapper.

---

## 🤝 Contributing

Contributions are welcome once the app scaffold lands. Before opening a PR:

1. Open an issue to discuss the change
2. Follow [Conventional Commits](https://www.conventionalcommits.org/) - see [`.gitmessage`](.gitmessage)
3. Keep diffs focused and small

---

## 🇺🇦 Stand with Ukraine

This project is built by a Ukrainian developer during an ongoing war. If you use `preflight.xml` and want to give back, please consider supporting the people actually defending Europe's eastern border:

- 🛡️ **[Хартія - Kharchenko Foundation](https://www.khartiiafoundation.com/)** - equipment, medical aid and training for Ukraine's defenders
- 🦅 **[Azov ONE](https://azov.one/fundraisers)** - fundraising hub for Azov brigade units on the front line

Any contribution matters. 💙💛

---

## 📜 License

Released under the [MIT License](LICENSE) © [kYaRick](https://github.com/kYaRick).

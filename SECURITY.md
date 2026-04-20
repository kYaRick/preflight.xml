# 🔐 Security Policy

## 📬 Reporting a vulnerability

If you believe you've found a security issue in **preflight.xml**, please **do not open a public issue**.

Instead, report it privately via GitHub's [Private Vulnerability Reporting](https://github.com/kYaRick/preflight.xml/security/advisories/new).

Please include:

- A clear description of the issue
- Steps to reproduce (minimal repro preferred)
- Impact assessment (what an attacker could achieve)
- Any suggested mitigation, if you have one

## ⏱️ Response expectations

- Initial acknowledgement: **within 72 hours**
- Triage & severity assessment: **within 7 days**
- Fix target: depends on severity; coordinated disclosure by default

## 🌐 Scope

`preflight.xml` is a **client-side, static** Blazor WebAssembly PWA. It processes data entirely in the user's browser - there is no backend, no database, no telemetry, no user accounts.

In-scope concerns include:

- XSS / injection in rendered XML or UI
- Malicious imported configurations
- Supply-chain risks in NuGet dependencies

Out of scope:

- Self-XSS via intentionally pasted scripts
- Issues in user-modified forks or self-hosted deployments
- Vulnerabilities in upstream dependencies already disclosed publicly

Thanks for helping keep users safe. 🙏

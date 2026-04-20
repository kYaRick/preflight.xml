namespace Preflight.App.Models;

/// <summary>
/// The three top-level UX modes. All three share a single <see cref="UnattendConfig"/>.
/// </summary>
public enum AppMode
{
    /// <summary>Step-by-step guided flow for non-experts.</summary>
    Wizard,

    /// <summary>Full manual editor - every option visible.</summary>
    Advanced,

    /// <summary>Read-only reference for every autounattend.xml option.</summary>
    Docs,
}

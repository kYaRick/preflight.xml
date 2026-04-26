using Preflight.App.Layout;

namespace Preflight.App.Services;

/// <summary>
/// Bridges page-level "open the import modal" intents to the modal that
/// lives in <see cref="MainLayout"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why a service.</b> The import modal needs its <c>backdrop-filter</c>
/// to blur the sticky page header. That works only when the overlay is in
/// a stacking context that contains the header - i.e. rendered as a sibling
/// of <c>FluentLayout</c> at the layout root. Page components live inside
/// the body grid, so a modal authored on a page would be trapped in the
/// body's stacking context (which sits below the header's z-index).
/// </para>
/// <para>
/// <b>Contract.</b> A page calls <see cref="OpenAsync"/> with a result
/// handler. <see cref="MainLayout"/> subscribes to <see cref="StateChanged"/>,
/// renders the modal in the open state, and forwards the import payload
/// back to the handler the page passed in.
/// </para>
/// </remarks>
public sealed class ImportModalService
{
    public bool IsOpen { get; private set; }

    /// <summary>Active result handler for the currently open invocation.</summary>
    public Func<ImportXmlModal.ImportPayload, Task>? ResultHandler { get; private set; }

    public event Action? StateChanged;

    /// <summary>Open the modal and route the result to <paramref name="onImported"/>.</summary>
    public void Open(Func<ImportXmlModal.ImportPayload, Task> onImported)
    {
        ResultHandler = onImported;
        IsOpen = true;
        StateChanged?.Invoke();
    }

    /// <summary>Close the modal. Called by the modal's own close affordances.</summary>
    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        ResultHandler = null;
        StateChanged?.Invoke();
    }

    /// <summary>Forward an import payload back to the page that opened the modal.</summary>
    internal Task NotifyImportedAsync(ImportXmlModal.ImportPayload payload)
        => ResultHandler?.Invoke(payload) ?? Task.CompletedTask;
}

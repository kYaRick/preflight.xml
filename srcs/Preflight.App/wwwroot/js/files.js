// preflightFiles - thin JS interop for browser-side file I/O.
// Exposed: download(filename, text, mime) - triggers a user-save dialog via a temporary anchor.
window.preflightFiles = (() => {
  function download(filename, text, mime) {
    const blob = new Blob([text], { type: mime || "application/xml" });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = filename || "autounattend.xml";
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
    // Give the browser a moment to start the download before revoking.
    setTimeout(() => URL.revokeObjectURL(url), 1500);
  }

  async function copyText(text) {
    if (navigator.clipboard && window.isSecureContext) {
      try {
        await navigator.clipboard.writeText(text);
        return true;
      } catch {
        /* fall through to legacy path */
      }
    }
    const ta = document.createElement("textarea");
    ta.value = text;
    ta.setAttribute("readonly", "");
    ta.style.position = "fixed";
    ta.style.opacity = "0";
    document.body.appendChild(ta);
    ta.select();
    let ok = false;
    try {
      ok = document.execCommand("copy");
    } catch {
      ok = false;
    }
    document.body.removeChild(ta);
    return ok;
  }

  // pickText: open a hidden <input type="file"> and resolve with the selected file's
  // text contents. Accept-mask defaults to *.xml; pass a custom value (e.g. ".json")
  // to narrow or widen the picker. Returns null when the user cancels.
  function pickText(accept) {
    return new Promise((resolve) => {
      const input = document.createElement("input");
      input.type = "file";
      input.accept = accept || ".xml";
      input.style.display = "none";
      let settled = false;

      const cleanup = () => {
        window.removeEventListener("focus", onFocus);
        if (input.parentNode) input.parentNode.removeChild(input);
      };

      const onChange = async () => {
        settled = true;
        const file = input.files && input.files[0];
        if (!file) { cleanup(); resolve(null); return; }
        try {
          const text = await file.text();
          cleanup();
          resolve({ name: file.name, text });
        } catch {
          cleanup();
          resolve(null);
        }
      };

      // The cancel button on native pickers doesn't fire `change`; we rely on the
      // window focus event to detect the dialog closing without a selection.
      const onFocus = () => {
        setTimeout(() => {
          if (!settled) { cleanup(); resolve(null); }
        }, 250);
      };

      input.addEventListener("change", onChange);
      window.addEventListener("focus", onFocus, { once: true });
      document.body.appendChild(input);
      input.click();
    });
  }

  // attachDropZone: bind native HTML5 drag-and-drop events on `el` and stream
  // the dropped file's text back to .NET via the supplied DotNetObjectReference.
  // Toggles `is-dragover` while a drag is hovering for CSS feedback. The
  // depth counter prevents flicker when the cursor crosses child elements
  // (per-element dragenter/dragleave fires repeatedly on each crossing).
  // Disposer is stashed on the element itself so detachDropZone can find it.
  function attachDropZone(el, dotNetRef) {
    if (!el) return;
    // Idempotent: remove any prior binding before re-attaching.
    detachDropZone(el);

    let depth = 0;
    const handlers = {
      dragenter: (e) => { e.preventDefault(); depth++; el.classList.add("is-dragover"); },
      dragover:  (e) => { e.preventDefault(); e.dataTransfer.dropEffect = "copy"; },
      dragleave: (e) => { e.preventDefault(); depth = Math.max(0, depth - 1); if (depth === 0) el.classList.remove("is-dragover"); },
      drop: async (e) => {
        e.preventDefault();
        depth = 0;
        el.classList.remove("is-dragover");
        const file = e.dataTransfer?.files?.[0];
        if (!file) return;
        try {
          const text = await file.text();
          await dotNetRef.invokeMethodAsync("OnFileDropped", file.name, text);
        } catch (err) {
          await dotNetRef.invokeMethodAsync("OnFileDroppedError", String(err?.message || err));
        }
      },
    };

    for (const [evt, fn] of Object.entries(handlers)) el.addEventListener(evt, fn);
    el._pfDropZoneCleanup = () => {
      for (const [evt, fn] of Object.entries(handlers)) el.removeEventListener(evt, fn);
      el.classList.remove("is-dragover");
    };
  }

  function detachDropZone(el) {
    if (el?._pfDropZoneCleanup) {
      el._pfDropZoneCleanup();
      delete el._pfDropZoneCleanup;
    }
  }

  return { download, copyText, pickText, attachDropZone, detachDropZone };
})();

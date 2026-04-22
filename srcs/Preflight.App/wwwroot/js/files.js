// preflightFiles - thin JS interop for browser-side file I/O.
// Exposed: download(filename, text, mime) — triggers a user-save dialog via a temporary anchor.
window.preflightFiles = (() => {
    function download(filename, text, mime) {
        const blob = new Blob([text], { type: mime || 'application/xml' });
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = filename || 'autounattend.xml';
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
        // Give the browser a moment to start the download before revoking.
        setTimeout(() => URL.revokeObjectURL(url), 1500);
    }

    async function copyText(text) {
        if (navigator.clipboard && window.isSecureContext) {
            try { await navigator.clipboard.writeText(text); return true; }
            catch { /* fall through to legacy path */ }
        }
        const ta = document.createElement('textarea');
        ta.value = text;
        ta.setAttribute('readonly', '');
        ta.style.position = 'fixed';
        ta.style.opacity = '0';
        document.body.appendChild(ta);
        ta.select();
        let ok = false;
        try { ok = document.execCommand('copy'); } catch { ok = false; }
        document.body.removeChild(ta);
        return ok;
    }

    return { download, copyText };
})();

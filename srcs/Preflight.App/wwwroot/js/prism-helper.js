// Prism bridge for Blazor. Blazor owns the <code> element's ref but renders
// it empty; this helper writes the current XML as textContent (which wipes
// any spans Prism added previously) and re-runs the highlighter. Called from
// OnAfterRenderAsync so token colours stay in sync with the preview.
window.preflightPrism = {
  highlight(el, text) {
    if (!el) return;
    el.textContent = text ?? "";
    // Prism loads with defer; on the very first render the script may not
    // have executed yet. Retry once on the next frame - by then it's there.
    if (window.Prism && window.Prism.highlightElement) {
      window.Prism.highlightElement(el);
    } else {
      requestAnimationFrame(() => {
        if (window.Prism && window.Prism.highlightElement) {
          window.Prism.highlightElement(el);
        }
      });
    }
  },
};

// DOM portal for escaping a trapped stacking context - needed because the
// Advanced page is rendered inside `.pf-page-enter`, whose will-change +
// transform create a new stacking context that traps position:fixed overlays
// behind the sticky header (z-index: 50). Attach() reparents the overlay to
// <body> on open; detach() snaps it back to the original parent right before
// Blazor's diff removes it - otherwise Blazor can't find it to remove.
window.preflightPortal = {
  attach(el) {
    if (!el || el.parentNode === document.body) return;
    el.__pfOrigParent = el.parentNode;
    el.__pfOrigNext = el.nextSibling;
    document.body.appendChild(el);
  },
  detach(el) {
    if (!el) return;
    const parent = el.__pfOrigParent;
    if (!parent) return;
    const next = el.__pfOrigNext;
    if (next && next.parentNode === parent) parent.insertBefore(el, next);
    else parent.appendChild(el);
    delete el.__pfOrigParent;
    delete el.__pfOrigNext;
  },
};

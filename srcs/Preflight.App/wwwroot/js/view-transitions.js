// Native View Transitions for Blazor WASM SPA navigation - opacity crossfade.
//
// A capture-phase click listener catches internal anchor clicks before Blazor's
// own handler, then drives the navigation inside document.startViewTransition so
// the browser snapshots old → new and crossfades. The crossfade look is defined
// in app.css (::view-transition-*(root)) and is OPACITY-ONLY:
//   • No transform → nothing slides or scales (earlier transform versions read
//     as "jerky" and moved the header/footer).
//   • No view-transition-name on .pf-page → the content box is NOT treated as a
//     morphing persistent element, so it can't scale between routes of different
//     height (that was the "small then stretches" double-render).
//   • Snapshot-based → no post-commit opacity blink (the WAAPI fallback fades
//     after the content already painted, which flickers; VT doesn't).
//
// Programmatic Nav.NavigateTo (Landing cards, wizard, ModeCard) bypasses this
// interceptor; theme.js's replayRouteAnim handles those with a plain WAAPI fade.
// The __pfVtStamp below tells replayRouteAnim "a VT just ran, don't double-fade".
//
// Falls back silently where startViewTransition is unavailable (Firefox): the
// module no-ops and Blazor's normal click navigation runs without a crossfade.
window.preflightViewTransitions = (() => {
  if (typeof document.startViewTransition !== "function") {
    return { supported: false };
  }

  const isInternalLeftClick = (e) =>
    e.button === 0 &&
    !e.metaKey &&
    !e.ctrlKey &&
    !e.shiftKey &&
    !e.altKey &&
    !e.defaultPrevented;

  const isNavigableLink = (link) => {
    if (!link) return false;
    if (!link.getAttribute("href")) return false;
    if (link.target && link.target !== "_self") return false;
    if (link.hasAttribute("download")) return false;
    if (link.getAttribute("rel")?.includes("external")) return false;
    return true;
  };

  document.addEventListener(
    "click",
    (e) => {
      if (!isInternalLeftClick(e)) return;
      const link = e.target.closest("a[href], fluent-anchor[href]");
      if (!isNavigableLink(link)) return;

      let url;
      try {
        url = new URL(link.getAttribute("href"), document.baseURI);
      } catch {
        return;
      }
      if (url.origin !== location.origin) return;

      // Same path, only a hash change → let the browser do the in-page jump.
      if (
        url.pathname + url.search === location.pathname + location.search &&
        url.hash !== location.hash
      ) {
        return;
      }
      // Exactly the current URL → swallow, nothing to navigate.
      if (url.href === location.href) {
        e.preventDefault();
        e.stopImmediatePropagation();
        return;
      }

      // preventDefault only (not stopImmediatePropagation) so any @onclick
      // side-effects on the same anchor still run; Blazor's own link handler
      // bails when defaultPrevented is already true, so no double navigation.
      e.preventDefault();

      // Tell replayRouteAnim a native VT is handling this nav (skip its fade).
      window.__pfVtStamp = performance.now();

      document.startViewTransition(() => {
        history.pushState(null, "", url.href);
        window.dispatchEvent(new PopStateEvent("popstate"));
      });
    },
    { capture: true },
  );

  return { supported: true };
})();

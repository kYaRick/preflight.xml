// Native View Transitions for Blazor WASM SPA navigation.
//
// Why: the previous WAAPI replayRouteAnim ran AFTER Blazor committed the
// new DOM, so a brief frame of "old layout / new content" flashed before
// the fade-in started. View Transitions API solves this at the browser
// layer: it snapshots the old DOM, runs our DOM-mutation callback,
// snapshots the new DOM, then crossfades between the two. No flicker
// possible - the snapshot is the last visible frame of the old page.
//
// Strategy:
//   Capture-phase click listener catches internal anchor clicks BEFORE
//   Blazor's own handler. We preventDefault + stopImmediatePropagation,
//   then drive the navigation ourselves INSIDE document.startViewTransition:
//     1. history.pushState (URL change)
//     2. dispatch popstate so Blazor's NavigationManager picks it up
//   Both are synchronous; Blazor's Router then renders the new component
//   synchronously. By the time the callback returns, DOM has mutated, so
//   the browser's "new state" snapshot at the next animation frame is
//   correct.
//
// Why not patch history.pushState directly: tried that - Blazor's own
// navigateTo uses an ASYNC JS→.NET interop call after pushState to notify
// the router. With pushState wrapped in startViewTransition, the URL
// changes inside the callback but Blazor's render fires AFTER the callback
// returns (in a later microtask). The View Transition then captures a
// "new" snapshot before Blazor has rendered, freezing the page on the
// old DOM until the user clicks again. Hence the click-interceptor
// pattern: we drive both the URL change AND the render trigger inside
// the synchronous VT callback.
//
// Coverage: anchor clicks only. Programmatic Nav.NavigateTo from C# (e.g.
// Landing card @onclick handlers, wizard Step+1 buttons) bypasses this
// interceptor and gets no crossfade - but also has no flicker, just an
// instant nav. To animate those too, the Razor templates would need to
// use <a href> instead of @onclick, which is a bigger change.
//
// Falls back silently in browsers without startViewTransition (Firefox at
// time of writing): the entire module becomes a no-op and Blazor's normal
// click handler runs the navigation without any crossfade.
window.preflightViewTransitions = (() => {
  if (typeof document.startViewTransition !== "function") {
    return { supported: false };
  }

  const isInternalLeftClick = (e) => {
    if (e.button !== 0) return false;
    if (e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return false;
    if (e.defaultPrevented) return false;
    return true;
  };

  const isNavigableLink = (link) => {
    if (!link) return false;
    // <a> exposes .href via prototype getter; <fluent-anchor> only exposes
    // it via attribute. Read both ways so both element types work.
    const href = link.getAttribute("href");
    if (!href) return false;
    if (link.target && link.target !== "_self") return false;
    if (link.hasAttribute("download")) return false;
    if (link.getAttribute("rel")?.includes("external")) return false;
    return true;
  };

  document.addEventListener(
    "click",
    (e) => {
      if (!isInternalLeftClick(e)) return;
      // Accept native <a href> and Fluent UI's <fluent-anchor href>. The
      // Fluent web component renders its inner <a> in shadow DOM, so a
      // plain "a[href]" selector misses it. The href attribute lives on
      // the custom element itself, which we detect here.
      const link = e.target.closest("a[href], fluent-anchor[href]");
      if (!isNavigableLink(link)) return;

      // Blazor href values are relative to <base href> (e.g. "/" or
      // "/preflight.xml/" on a subpath deploy), NOT to the current URL.
      // Resolving "wizard/guided/3" against location.href on page
      // /wizard/guided/2 produces /wizard/guided/wizard/guided/3 - wrong.
      // document.baseURI honors the <base> element so we get
      // /wizard/guided/3 like Blazor's NavigationManager does.
      const rawHref = link.getAttribute("href");
      let url;
      try {
        url = new URL(rawHref, document.baseURI);
      } catch {
        return;
      }
      if (url.origin !== location.origin) return;

      // Same URL (modulo hash) - let the browser handle the in-page jump.
      if (
        url.pathname + url.search === location.pathname + location.search &&
        url.hash !== location.hash
      ) {
        return;
      }
      // Same URL exactly - nothing to do.
      if (url.href === location.href) {
        e.preventDefault();
        e.stopImmediatePropagation();
        return;
      }

      // Take ownership of this navigation away from Blazor's default click
      // handler so we can drive both the URL change AND the render trigger
      // inside a single synchronous View Transition callback.
      //
      // We deliberately DO NOT call stopImmediatePropagation here - that
      // would block Blazor's @onclick handlers attached to the same anchor
      // (used for side-effects like ModeService.SwitchMode that should
      // run alongside the navigation). preventDefault alone is enough:
      //  - Browser's native navigation: blocked by preventDefault.
      //  - Blazor's link interceptor: bails out when defaultPrevented is
      //    true, so it won't double-navigate.
      //  - Component @onclick handlers: still fire (state side-effects OK).
      e.preventDefault();

      document.startViewTransition(() => {
        // pushState + popstate dispatch is the most stable way to drive
        // Blazor's NavigationManager without depending on Blazor._internal
        // APIs. Both calls are synchronous; Blazor's popstate listener
        // fires the .NET LocationChanged event which renders the new route
        // inline, so the DOM has mutated by the time this callback returns.
        history.pushState(null, "", url.href);
        window.dispatchEvent(new PopStateEvent("popstate"));
      });
    },
    { capture: true },
  );

  return { supported: true };
})();

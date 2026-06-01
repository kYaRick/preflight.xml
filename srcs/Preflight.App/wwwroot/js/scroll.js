// Scroll helpers for the go-top plane button.
//
// The app does NOT always scroll the window. FluentUI's <FluentBodyContent>
// renders a `.body-content` element inside the layout grid that becomes the
// real scroll container on most routes (the document itself stays pinned to
// the viewport height). Two problems fell out of assuming `window`:
//   1. A scroll listener on `window` never fires for an inner scroller -
//      scroll events do NOT bubble - so the button stayed hidden on every
//      page whose content scrolled inside .body-content.
//   2. window.scrollTo()/scrollY operate on the document, which isn't moving,
//      so the "go top" click animated nothing and read as a dead button.
//
// Both are fixed by (a) resolving the element that actually scrolls at call
// time, (b) listening in the CAPTURE phase on `document` so scroll from ANY
// element is observed without relying on bubbling, and (c) animating that
// element's scrollTop directly.
window.preflightScroll = (() => {
  const THRESHOLD = 400;
  // Distance->duration mapping. Short scrolls finish quickly so the
  // button feels responsive; long scrolls get a roomier ride so the
  // user can register the motion. Capped so even a 20k-px page stays
  // under a second.
  const MIN_DURATION_MS = 320;
  const MAX_DURATION_MS = 900;
  const PX_PER_MS = 6;

  let currentlyVisible = false;
  let raf = 0;
  let ref = null;
  let activeAnim = 0;

  // Resolve the element that is ACTUALLY scrolling right now. Preference:
  //   1. any known candidate whose scrollTop is already > 0 (it's the live one)
  //   2. any candidate that is scrollable (overflow auto/scroll + content taller)
  //   3. document.scrollingElement as a last resort
  // Re-resolved on every call because the scroller can differ per route.
  function getScroller() {
    const cands = [
      document.scrollingElement,
      document.querySelector(".body-content"),
      document.querySelector("fluent-body-content"),
      document.querySelector(".pf-page"),
    ].filter(Boolean);

    for (const el of cands) {
      if (el.scrollTop > 0) return el;
    }
    for (const el of cands) {
      const cs = getComputedStyle(el);
      const scrollable = cs.overflowY === "auto" || cs.overflowY === "scroll";
      if (scrollable && el.scrollHeight > el.clientHeight + 2) return el;
    }
    return document.scrollingElement || document.documentElement;
  }

  function getScrollY() {
    return getScroller().scrollTop || 0;
  }

  function setScrollY(y) {
    getScroller().scrollTop = y;
  }

  function tick() {
    raf = 0;
    const shouldShow = getScrollY() > THRESHOLD;
    if (shouldShow === currentlyVisible) return;
    currentlyVisible = shouldShow;
    // Toggle the button's visibility class DIRECTLY in JS - no .NET round-trip,
    // no Blazor StateHasChanged. Routing this through Blazor meant the first
    // scroll past the threshold triggered a full layout re-render (a diff over
    // the whole FluentUI component tree), which dropped one frame and read as a
    // one-time "jerk" on the first scroll. Pure DOM class toggle has zero
    // render cost. The button's markup is static (no Blazor-bound class), so a
    // later Blazor render won't clobber what we set here.
    const btn = document.querySelector(".pf-go-top");
    if (btn) btn.classList.toggle("is-visible", shouldShow);
  }

  function onScroll() {
    if (raf) return;
    raf = requestAnimationFrame(tick);
  }

  function watchTop(dotNetRef) {
    ref = dotNetRef;
    // Seed initial state so the button shows immediately on a deep scroll
    // that was restored by the browser.
    currentlyVisible = !currentlyVisible; // force the threshold check to fire
    tick();
    // Capture phase + document target: scroll events don't bubble, but they
    // DO propagate during capture, so this single listener catches scrolling
    // on the window AND on any inner scroll container (.body-content, etc.).
    document.removeEventListener("scroll", onScroll, true);
    document.addEventListener("scroll", onScroll, { passive: true, capture: true });
  }

  // easeInOutCubic - slow start, fast middle, slow end. Feels like a
  // takeoff-then-glide, which matches the plane glyph metaphor.
  function easeInOutCubic(t) {
    return t < 0.5
      ? 4 * t * t * t
      : 1 - Math.pow(-2 * t + 2, 3) / 2;
  }

  function cancelActive() {
    if (activeAnim) {
      cancelAnimationFrame(activeAnim);
      activeAnim = 0;
    }
  }

  // If the user starts wheeling/touching during the scripted scroll, bail
  // out - they're taking control and our animation would fight them.
  function attachInterruptListeners() {
    const cancel = () => {
      cancelActive();
      window.removeEventListener("wheel",     cancel, { passive: true });
      window.removeEventListener("touchstart", cancel, { passive: true });
      window.removeEventListener("keydown",   cancel);
    };
    window.addEventListener("wheel",     cancel, { passive: true });
    window.addEventListener("touchstart", cancel, { passive: true });
    window.addEventListener("keydown",   cancel);
    return cancel;
  }

  // RAF-driven smooth scroll on a specific element's scrollTop. Used as a
  // fallback when Element.scrollTo({behavior:'smooth'}) is unavailable.
  function animateScrollTop(scroller, startY) {
    cancelActive();
    const duration = Math.max(
      MIN_DURATION_MS,
      Math.min(MAX_DURATION_MS, startY / PX_PER_MS)
    );
    const startedAt = performance.now();
    const detach = attachInterruptListeners();

    function step(now) {
      const t = Math.min(1, (now - startedAt) / duration);
      scroller.scrollTop = startY * (1 - easeInOutCubic(t));
      if (t < 1) {
        activeAnim = requestAnimationFrame(step);
      } else {
        activeAnim = 0;
        detach();
      }
    }
    activeAnim = requestAnimationFrame(step);
  }

  function toTop() {
    // Resolve the live scroller and pin it so a mid-flight re-resolve (its
    // scrollTop hitting 0 changes the candidate set) can't retarget us.
    const scroller = getScroller();
    const startY = scroller.scrollTop || 0;
    if (startY <= 0) return;

    // Deliberately do NOT honor prefers-reduced-motion here. Clicking the
    // go-top button is an explicit request for the animated flight back to
    // the top; on systems with "reduced motion" enabled (e.g. Windows with
    // animations turned off) BOTH native smooth scroll AND a reduce-gated
    // tween collapse to an instant jump, which is exactly the "it just
    // snaps to the top" the user reported. We also avoid native
    // Element.scrollTo({behavior:'smooth'}) for the same reason: the engine
    // suppresses its animation under reduced-motion. A hand-driven RAF tween
    // is the only path that animates regardless of the OS setting.
    animateScrollTop(scroller, startY);
  }

  return { watchTop, toTop };
})();

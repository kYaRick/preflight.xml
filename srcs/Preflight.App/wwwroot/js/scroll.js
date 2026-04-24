// Scroll helpers for the go-top plane button.
// - watchTop: throttles the scroll event and calls .NET OnScrollVisibility
//   only when the 400px threshold is crossed (reduces interop chatter).
// - toTop: smooth-scrolls to top, respecting prefers-reduced-motion (which
//   collapses smooth to an instant jump).
window.preflightScroll = (() => {
  const THRESHOLD = 400;
  let currentlyVisible = false;
  let raf = 0;
  let ref = null;

  function getScrollY() {
    return window.scrollY || document.documentElement.scrollTop || 0;
  }

  function tick() {
    raf = 0;
    const shouldShow = getScrollY() > THRESHOLD;
    if (shouldShow === currentlyVisible) return;
    currentlyVisible = shouldShow;
    if (ref) {
      try { ref.invokeMethodAsync("OnScrollVisibility", shouldShow); }
      catch { /* component unmounted */ }
    }
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
    window.removeEventListener("scroll", onScroll);
    window.addEventListener("scroll", onScroll, { passive: true });
  }

  function toTop() {
    const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    window.scrollTo({ top: 0, behavior: reduce ? "auto" : "smooth" });
  }

  return { watchTop, toTop };
})();

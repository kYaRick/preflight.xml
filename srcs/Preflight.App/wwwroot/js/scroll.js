// Scroll helpers for the go-top plane button.
// - watchTop: throttles the scroll event and calls .NET OnScrollVisibility
//   only when the 400px threshold is crossed (reduces interop chatter).
// - toTop: RAF-driven smooth scroll with a custom ease and a duration
//   scaled to distance. Native window.scrollTo({behavior:'smooth'}) is
//   browser-controlled and on tall pages reads as "instantaneous jump"
//   because the duration caps regardless of distance - that's exactly
//   the "kicks you to the top" feel we want to avoid.
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

  // If the user starts wheeling/touching during the animation, bail out
  // - they're taking control, our scripted scroll would fight them.
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

  function toTop() {
    const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    const startY = getScrollY();
    if (startY <= 0) return;
    if (reduce) {
      window.scrollTo(0, 0);
      return;
    }

    cancelActive();
    const distance = startY;
    const duration = Math.max(
      MIN_DURATION_MS,
      Math.min(MAX_DURATION_MS, distance / PX_PER_MS)
    );
    const startedAt = performance.now();
    const detach = attachInterruptListeners();

    function step(now) {
      const elapsed = now - startedAt;
      const t = Math.min(1, elapsed / duration);
      const eased = easeInOutCubic(t);
      window.scrollTo(0, startY * (1 - eased));
      if (t < 1) {
        activeAnim = requestAnimationFrame(step);
      } else {
        activeAnim = 0;
        detach();
      }
    }
    activeAnim = requestAnimationFrame(step);
  }

  return { watchTop, toTop };
})();

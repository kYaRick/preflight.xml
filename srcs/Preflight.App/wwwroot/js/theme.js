// 🎨 Single source of truth for the selected UI theme.
//
//    Storage:   localStorage['preflight.theme']  →  'system' | 'light' | 'dark'
//    HTML tag:  <html class="theme-light|theme-dark" data-pf-theme-source="...">
//               The class always reflects the EFFECTIVE theme, even for system,
//               so CSS-only surfaces like Prism can style correctly.
//
//    Called by:
//      - index.html inline <head> script (initial paint, before Blazor boot)
//      - Preflight.App/Layout/ThemeSwitcher.razor (runtime changes)
//      - Preflight.App/Layout/MainLayout.razor (initial Mode sync)
//
//    We deliberately do NOT use Fluent's built-in StorageName=".."
//    because it writes a JSON blob that the pre-boot CSS script
//    cannot parse in time; unifying on one plain string keeps
//    pre-boot chrome and post-boot UI consistent.
// Tiny UI helper - keeps .pf-switcher sliding indicators aligned with
// whatever button carries .is-active, even after a responsive media
// query rearranges button widths (e.g. hiding language labels on mobile).
//
//   alignIndicators()   - measure every switcher and sync the indicator
//                         style. Called from Razor after a render
//                         (no round-trip needed), and automatically on
//                         window resize.
//   measureActive(root) - legacy single-switcher measurement, retained
//                         so Razor can still get the initial metrics
//                         before the resize observer is wired up.
window.preflightUI = (() => {
  let raf = 0;

  const alignIndicators = () => {
    const switchers = document.querySelectorAll(".pf-switcher");
    switchers.forEach((sw) => {
      const active = sw.querySelector(".pf-switcher__btn.is-active");
      const indicator = sw.querySelector(".pf-switcher__indicator");
      if (!active || !indicator) return;
      indicator.style.transform = `translateX(${active.offsetLeft}px)`;
      indicator.style.width = `${active.offsetWidth}px`;
      indicator.style.opacity = "1";
    });
  };

  const scheduleAlign = () => {
    cancelAnimationFrame(raf);
    raf = requestAnimationFrame(alignIndicators);
  };

  window.addEventListener("resize", scheduleAlign);
  // Also re-align when fonts finish loading (emoji + custom fonts shift widths).
  if (document.fonts?.ready) {
    document.fonts.ready.then(scheduleAlign);
  }

  return {
    alignIndicators,
    measureActive: (root) => {
      if (!root) return null;
      const active = root.querySelector(".pf-switcher__btn.is-active");
      if (!active) return null;
      return { left: active.offsetLeft, width: active.offsetWidth };
    },
  };
})();

// Page-enter animation - driven by Web Animations API, not CSS.
// WAAPI animations run even when the browser's CSS engine is in reduced-
// motion mode (e.g. automated test runners force it on) because WAAPI
// animations are independent of the @media (prefers-reduced-motion) CSS
// branch. We still honor the user's actual preference by picking a gentler
// keyframe set when matchMedia reports reduce=true.
//
// Called from:
//   - index.html, right after the loading overlay fades out (first paint)
//   - MainLayout.OnAfterRenderAsync, whenever NavigationManager fires
//     LocationChanged (SPA navigation between routes)
window.preflightNav = (() => {
  // No-op. We deliberately DON'T run any WAAPI animation on .pf-page.
  //
  // History: this used to slide/scale/fade .pf-page on every navigation and on
  // first paint. Every variant caused a problem - transforms slid the chrome and
  // broke the fixed Code FAB's containing block; the opacity fade promoted
  // .pf-page to a compositing layer, and that layer churning on the FIRST scroll
  // after a route loaded read as a one-time "jerk". Animating a persistent
  // content container on a SPA that repaints instantly is just trouble.
  //
  // Navigation smoothness now comes ENTIRELY from the native View Transition
  // crossfade (view-transitions.js + ::view-transition CSS), which animates a
  // browser snapshot - it never touches .pf-page's own layer, so it can't cause
  // the FAB bug or the first-scroll churn. Anchor navigations (sidebar, docs,
  // breadcrumbs, in-page links) get that crossfade. Programmatic Nav.NavigateTo
  // (Landing cards, wizard Next, ModeCard) and first paint appear immediately -
  // calm and stable, no layer work behind the user's first scroll.
  //
  // Functions kept as no-ops because index.html and MainLayout still call them.
  const replayPageAnim = () => {};
  const replayRouteAnim = () => {};

  return { replayPageAnim, replayRouteAnim };
})();

(() => {
  const KEY = "preflight.theme";
  const MEDIA = window.matchMedia?.("(prefers-color-scheme: dark)") ?? null;

  const getEffectiveTheme = (mode) => {
    if (mode === "light" || mode === "dark") return mode;
    return MEDIA?.matches ? "dark" : "light";
  };

  const applyThemeState = (mode) => {
    const html = document.documentElement;
    const effective = getEffectiveTheme(mode);

    html.classList.remove("theme-light", "theme-dark", "theme-system");
    html.classList.add(`theme-${effective}`);
    if (mode === "system") {
      html.classList.add("theme-system");
    }

    html.dataset.pfTheme = effective;
    html.dataset.pfThemeSource = mode;
    return effective;
  };

  const animateThemeSwap = (mode) => {
    const html = document.documentElement;
    const swap = () => applyThemeState(mode);

    html.classList.add("theme-transitioning");
    window.clearTimeout(window.preflightTheme._tClear);
    window.preflightTheme._tClear = window.setTimeout(
      () => html.classList.remove("theme-transitioning"),
      700,
    );

    const reduce = window.matchMedia?.(
      "(prefers-reduced-motion: reduce)",
    ).matches;
    if (!reduce && document.startViewTransition) {
      document.startViewTransition(swap);
    } else {
      swap();
    }
  };

  window.preflightTheme = {
    /** 'system' | 'light' | 'dark' */
    get: () => {
      try {
        const v = localStorage.getItem(KEY);
        return v === "light" || v === "dark" ? v : "system";
      } catch {
        return "system";
      }
    },

    // User-initiated theme change: animated path. Layers two mechanisms:
    //   1. <html class="theme-transitioning"> for ~500ms - opts every
    //      themed surface into CSS transitions on bg/color/border. Works
    //      in every browser, and covers FluentDesignTheme's separate
    //      re-paint (which runs outside the View Transition).
    //   2. document.startViewTransition() (Chromium/Safari) - adds a
    //      full-viewport crossfade on top of #1.
    // Both are skipped for prefers-reduced-motion.
    set: (mode) => {
      try {
        if (mode === "light" || mode === "dark") {
          localStorage.setItem(KEY, mode);
        } else {
          localStorage.removeItem(KEY);
        }
      } catch {
        /* no-op */
      }

      animateThemeSwap(mode);
    },

    // Raw apply - no animation. Used by the inline boot script in
    // index.html to set the stored theme before first paint.
    apply: (mode) => applyThemeState(mode),
  };

  const onSystemThemeChanged = () => {
    if (window.preflightTheme.get() !== "system") return;
    animateThemeSwap("system");
  };

  if (MEDIA) {
    if (typeof MEDIA.addEventListener === "function") {
      MEDIA.addEventListener("change", onSystemThemeChanged);
    } else if (typeof MEDIA.addListener === "function") {
      MEDIA.addListener(onSystemThemeChanged);
    }
  }
})();

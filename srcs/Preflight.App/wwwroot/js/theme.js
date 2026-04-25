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
  const EASE = "cubic-bezier(0.16, 1, 0.3, 1)";

  const animateRoot = (root, keyframes, options) => {
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        root.animate(keyframes, options);
      });
    });
  };

  // First-paint animation only - called once by index.html after the
  // loading overlay starts fading. Not used for SPA navigation (the
  // app should navigate normally; route-level view transitions were
  // removed because the history monkey-patch could desync Blazor's
  // router from the rendered view.
  const replayPageAnim = () => {
    const root = document.querySelector(".pf-page");
    if (!root) return;
    const reduce = window.matchMedia?.(
      "(prefers-reduced-motion: reduce)",
    ).matches;

    root.getAnimations({ subtree: true }).forEach((a) => a.cancel());

    if (reduce) {
      root.animate([{ opacity: 0 }, { opacity: 1 }], {
        duration: 280,
        easing: EASE,
        fill: "both",
      });
      return;
    }

    root.animate(
      [
        {
          opacity: 0,
          transform: "translateY(24px) scale(0.98)",
          filter: "blur(4px)",
        },
        { opacity: 1, transform: "translateY(0) scale(1)", filter: "blur(0)" },
      ],
      { duration: 550, easing: EASE, fill: "both" },
    );

    const kids = root.querySelectorAll(
      "h1, .fluent-messagebar, fluent-card, " +
        'fluent-button[appearance="accent"], fluent-button[appearance="outline"]',
    );
    kids.forEach((child, i) => {
      child.animate(
        [
          { opacity: 0, transform: "translateY(20px)" },
          { opacity: 1, transform: "translateY(0)" },
        ],
        { duration: 460, delay: 100 + i * 70, easing: EASE, fill: "both" },
      );
    });
  };

  // Route change animation - runs ONLY after Blazor has already committed the
  // new page content. That keeps navigation ownership entirely in Blazor and
  // avoids the router/history desync caused by the old pushState monkey-patch.
  //
  // Skipped when the browser supports the View Transitions API: in that case
  // the click interceptor in view-transitions.js already wraps Blazor's DOM
  // mutation in document.startViewTransition() and the browser crossfades
  // the old → new snapshots natively. Running this on top would double-animate
  // and reintroduce the flicker the View Transition was meant to kill.
  const replayRouteAnim = () => {
    if (typeof document.startViewTransition === "function") return;
    const root = document.querySelector(".pf-page");
    if (!root) return;

    const reduce = window.matchMedia?.(
      "(prefers-reduced-motion: reduce)",
    ).matches;

    root.getAnimations({ subtree: true }).forEach((a) => a.cancel());

    if (reduce) {
      animateRoot(root, [{ opacity: 0.82 }, { opacity: 1 }], {
        duration: 200,
        easing: EASE,
        fill: "both",
      });
      return;
    }

    // Bigger numbers than before — the previous values (14px / scale 0.992)
    // were too subtle to read as "the page changed". Drop the blur (it was
    // expensive on low-end mobile and only added a smear, not the sense of
    // motion). Stagger more kids and longer so the page assembles in front
    // of the user instead of snapping in.
    animateRoot(
      root,
      [
        { opacity: 0, transform: "translateY(28px) scale(0.985)" },
        { opacity: 1, transform: "translateY(0) scale(1)" },
      ],
      { duration: 480, easing: EASE, fill: "both" },
    );

    const kids = root.querySelectorAll(
      "h1, h2, .fluent-messagebar, fluent-card, .pf-section__head, .pf-xml-panel, fluent-anchor, fluent-button[appearance='accent']",
    );
    kids.forEach((child, i) => {
      animateRoot(
        child,
        [
          { opacity: 0, transform: "translateY(16px)" },
          { opacity: 1, transform: "translateY(0)" },
        ],
        {
          duration: 380,
          delay: 60 + Math.min(i, 8) * 50,
          easing: EASE,
          fill: "both",
        },
      );
    });
  };

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

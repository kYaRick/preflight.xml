// 🎨 Single source of truth for the selected UI theme.
//
//    Storage:   localStorage['preflight.theme']  →  'system' | 'light' | 'dark'
//    HTML tag:  <html class="theme-light|theme-dark"> (empty when system)
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
        const switchers = document.querySelectorAll('.pf-switcher');
        switchers.forEach(sw => {
            const active = sw.querySelector('.pf-switcher__btn.is-active');
            const indicator = sw.querySelector('.pf-switcher__indicator');
            if (!active || !indicator) return;
            indicator.style.transform = `translateX(${active.offsetLeft}px)`;
            indicator.style.width = `${active.offsetWidth}px`;
            indicator.style.opacity = '1';
        });
    };

    const scheduleAlign = () => {
        cancelAnimationFrame(raf);
        raf = requestAnimationFrame(alignIndicators);
    };

    window.addEventListener('resize', scheduleAlign);
    // Also re-align when fonts finish loading (emoji + custom fonts shift widths).
    if (document.fonts?.ready) {
        document.fonts.ready.then(scheduleAlign);
    }

    return {
        alignIndicators,
        measureActive: (root) => {
            if (!root) return null;
            const active = root.querySelector('.pf-switcher__btn.is-active');
            if (!active) return null;
            return { left: active.offsetLeft, width: active.offsetWidth };
        },
    };
})();

(() => {
    const KEY = 'preflight.theme';

    window.preflightTheme = {
        /** 'system' | 'light' | 'dark' */
        get: () => {
            try {
                const v = localStorage.getItem(KEY);
                return v === 'light' || v === 'dark' ? v : 'system';
            } catch { return 'system'; }
        },

        set: (mode) => {
            try {
                if (mode === 'light' || mode === 'dark') {
                    localStorage.setItem(KEY, mode);
                } else {
                    localStorage.removeItem(KEY);
                }
            } catch { /* no-op */ }
            window.preflightTheme.apply(mode);
        },

        apply: (mode) => {
            const html = document.documentElement;
            html.classList.remove('theme-light', 'theme-dark');
            if (mode === 'light' || mode === 'dark') {
                html.classList.add('theme-' + mode);
            }
        },
    };
})();

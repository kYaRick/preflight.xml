// Left-edge "download the app" dock (web only). The dock markup is rendered by
// the Blazor Landing page; this module wires up its two interactions:
//   • tap the handle  → toggle the panel open/closed (slide)
//   • drag the handle → move the dock vertically (position persisted)
// A small movement threshold separates a tap from a drag so the two never
// fight. The handle is a real <button>, so keyboard (Enter/Space) toggles too.
//
// Landing mounts/unmounts across Blazor SPA navigation, so a MutationObserver
// re-runs init whenever the dock element (re)appears; a per-element guard keeps
// it idempotent.
(function () {
    'use strict';

    var STORAGE_KEY = 'pf-edge-dock-top';
    var DRAG_THRESHOLD = 5; // px before a press counts as a drag, not a tap
    var EDGE_GAP = 8;       // keep the dock this far from top/bottom edges

    function clampTop(centerY, dockHeight) {
        var half = dockHeight / 2;
        var min = half + EDGE_GAP;
        var max = window.innerHeight - half - EDGE_GAP;
        if (max < min) { return window.innerHeight / 2; }
        return Math.max(min, Math.min(max, centerY));
    }

    function init() {
        var dock = document.getElementById('pf-edge-dock');
        if (!dock || dock.__pfDockInit) { return; }
        dock.__pfDockInit = true;

        var handle = dock.querySelector('.pf-edge-dock__handle');
        if (!handle) { return; }

        // Restore a previously dragged position (clamped to the current viewport).
        try {
            var saved = parseFloat(window.localStorage.getItem(STORAGE_KEY));
            if (!isNaN(saved)) {
                dock.style.setProperty('--pf-dock-top',
                    clampTop(saved, dock.offsetHeight) + 'px');
            }
        } catch (_) { /* storage blocked - default to centred */ }

        var dragging = false;
        var moved = false;
        var startY = 0;
        var startCenter = 0;

        function onPointerDown(e) {
            if (e.button !== undefined && e.button !== 0) { return; }
            dragging = true;
            moved = false;
            startY = e.clientY;
            var rect = dock.getBoundingClientRect();
            startCenter = rect.top + rect.height / 2;
            try { handle.setPointerCapture(e.pointerId); } catch (_) { /* ignore */ }
        }

        function onPointerMove(e) {
            if (!dragging) { return; }
            var dy = e.clientY - startY;
            if (!moved && Math.abs(dy) > DRAG_THRESHOLD) { moved = true; }
            if (moved) {
                var top = clampTop(startCenter + dy, dock.offsetHeight);
                dock.style.setProperty('--pf-dock-top', top + 'px');
            }
        }

        function onPointerUp(e) {
            if (!dragging) { return; }
            dragging = false;
            try { handle.releasePointerCapture(e.pointerId); } catch (_) { /* ignore */ }

            if (moved) {
                var top = dock.style.getPropertyValue('--pf-dock-top').trim();
                try { window.localStorage.setItem(STORAGE_KEY, parseFloat(top)); } catch (_) { /* ignore */ }
            } else {
                toggle();
            }
        }

        function toggle() {
            var open = dock.classList.toggle('is-open');
            handle.setAttribute('aria-expanded', String(open));
        }

        handle.addEventListener('pointerdown', onPointerDown);
        handle.addEventListener('pointermove', onPointerMove);
        handle.addEventListener('pointerup', onPointerUp);
        handle.addEventListener('pointercancel', function () { dragging = false; });
        // Suppress the synthetic click that follows pointerup so it never
        // double-toggles; pointerup already handled the tap.
        handle.addEventListener('click', function (e) { e.preventDefault(); });
        // Keyboard activation (the pointer flow skips keyboard).
        handle.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar') {
                e.preventDefault();
                toggle();
            }
        });

        // Re-clamp on resize so a saved position can't strand the dock off-screen.
        window.addEventListener('resize', function () {
            var cur = parseFloat(dock.style.getPropertyValue('--pf-dock-top'));
            if (!isNaN(cur)) {
                dock.style.setProperty('--pf-dock-top',
                    clampTop(cur, dock.offsetHeight) + 'px');
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Landing re-renders on SPA navigation - re-init when the dock reappears.
    var observer = new MutationObserver(init);
    observer.observe(document.documentElement, { childList: true, subtree: true });

    window.preflightEdgeDock = { init: init };
})();

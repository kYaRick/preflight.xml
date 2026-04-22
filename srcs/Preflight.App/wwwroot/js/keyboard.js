// ⌨️ Global keyboard shortcut bridge between the browser and Blazor.
//
//    Blazor components register a DotNetObjectReference via
//    window.preflightKeyboard.register(dotNetRef). The ref must expose
//    a [JSInvokable] method `OnShortcut(string id)`. Shortcut ids:
//      "palette"   → Ctrl+K / Cmd+K
//      "help"      → Shift+? (or just `?`)
//      "escape"    → Escape (used to close overlays)
//
//    Shortcuts are IGNORED when focus is inside an editable element
//    (input / textarea / contenteditable) so typing still works.
// A page-scoped variant of the same pattern — AdvancedShell registers itself
// while mounted, unregisters on dispose. Kept separate from preflightKeyboard so
// the global layout subscription (Ctrl+K, ?) isn't clobbered when the Advanced
// page swaps its own ref in. Shortcut ids:
//      "xml-preview"   → Ctrl+Shift+X / Cmd+Shift+X
window.preflightAdvancedShortcuts = (() => {
    let subscriber = null;
    let attached = false;

    const onKeyDown = (e) => {
        if (!subscriber) return;
        // Ctrl/Cmd + Shift + X — toggle the live XML preview modal.
        if ((e.metaKey || e.ctrlKey) && e.shiftKey && !e.altKey && e.key.toLowerCase() === 'x') {
            e.preventDefault();
            subscriber.invokeMethodAsync('OnShortcut', 'xml-preview');
        }
    };

    return {
        register: (dotNetRef) => {
            subscriber = dotNetRef;
            if (!attached) {
                window.addEventListener('keydown', onKeyDown);
                attached = true;
            }
        },
        unregister: () => {
            subscriber = null;
            if (attached) {
                window.removeEventListener('keydown', onKeyDown);
                attached = false;
            }
        },
    };
})();

window.preflightKeyboard = (() => {
    let subscriber = null;
    let attached = false;

    const isEditable = (el) => {
        if (!el) return false;
        const tag = el.tagName;
        if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return true;
        if (el.isContentEditable) return true;
        // FluentTextField and friends use shadow DOM; check closest fluent-* input
        if (el.closest && el.closest('fluent-text-field, fluent-text-area, fluent-search, fluent-select, fluent-combobox')) {
            return true;
        }
        return false;
    };

    const onKeyDown = (e) => {
        if (!subscriber) return;

        // Cmd/Ctrl+K - command palette (works even inside inputs, by convention)
        if ((e.metaKey || e.ctrlKey) && !e.shiftKey && !e.altKey && e.key.toLowerCase() === 'k') {
            e.preventDefault();
            subscriber.invokeMethodAsync('OnShortcut', 'palette');
            return;
        }

        // Escape - always fires, lets overlays close
        if (e.key === 'Escape') {
            subscriber.invokeMethodAsync('OnShortcut', 'escape');
            return;
        }

        // ? / Shift+/ - help. Ignore when typing in a field.
        if (e.key === '?' && !isEditable(e.target)) {
            e.preventDefault();
            subscriber.invokeMethodAsync('OnShortcut', 'help');
            return;
        }
    };

    return {
        register: (dotNetRef) => {
            subscriber = dotNetRef;
            if (!attached) {
                window.addEventListener('keydown', onKeyDown);
                attached = true;
            }
        },
        unregister: () => {
            subscriber = null;
            if (attached) {
                window.removeEventListener('keydown', onKeyDown);
                attached = false;
            }
        },
    };
})();

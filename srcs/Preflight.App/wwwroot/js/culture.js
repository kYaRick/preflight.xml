// 🌐 Culture helper - read preferred UI culture from localStorage,
//    fall back to the browser's preferred language.
//    Returns a BCP-47 tag consumed by Preflight.App/Program.cs at startup.
window.preflightCulture = {
    get: () => {
        const stored = localStorage.getItem('preflight.culture');
        if (stored) return stored;
        const nav = (navigator.language || 'en').toLowerCase();
        return nav.startsWith('uk') ? 'uk' : 'en';
    },
    set: (value) => localStorage.setItem('preflight.culture', value),
};

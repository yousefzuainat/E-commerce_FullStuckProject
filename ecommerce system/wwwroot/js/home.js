/* ============================================================
   VOLTEX — Home Page JavaScript
   wwwroot/js/home.js
   ============================================================ */

/* ── Flash Sale Countdown Timer ── */
(function initCountdown() {
    // Set countdown 8h 42m 17s from first load (persisted in sessionStorage)
    const key = 'voltex-sale-end';
    let end = parseInt(sessionStorage.getItem(key) || '0', 10);
    if (!end || end < Date.now()) {
        end = Date.now() + (8 * 3600 + 42 * 60 + 17) * 1000;
        sessionStorage.setItem(key, end);
    }

    function tick() {
        const d = Math.max(0, end - Date.now());
        const safe = (id, val) => { const el = document.getElementById(id); if (el) el.textContent = val; };
        safe('ch', String(Math.floor(d / 3600000)).padStart(2, '0'));
        safe('cm', String(Math.floor(d % 3600000 / 60000)).padStart(2, '0'));
        safe('cs', String(Math.floor(d % 60000 / 1000)).padStart(2, '0'));
    }

    setInterval(tick, 1000);
    tick();
})();

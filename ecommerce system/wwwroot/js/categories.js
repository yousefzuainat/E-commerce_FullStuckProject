/* ============================================================
   VOLTEX — Categories Index JavaScript
   wwwroot/js/categories.js
   ============================================================ */

(function initCategoryAdmin() {
    const grid      = document.getElementById('catGrid');
    const noResults = document.getElementById('catNoResults');
    const searchIn  = document.getElementById('catSearch');
    const sortSel   = document.getElementById('catSort');

    if (!grid) return; // empty-state page has no grid

    function getCards() {
        return Array.from(grid.querySelectorAll('.cag-card'));
    }

    function filterAndSort() {
        const query = (searchIn ? searchIn.value.toLowerCase().trim() : '');
        const sort  = (sortSel  ? sortSel.value : 'name');

        let cards = getCards();

        /* ── Filter ── */
        cards.forEach(c => {
            const name  = (c.dataset.name  || '').toLowerCase();
            const match = !query || name.includes(query);
            c.style.display = match ? '' : 'none';
        });

        /* ── Sort visible cards ── */
        const visible = cards.filter(c => c.style.display !== 'none');

        visible.sort((a, b) => {
            const na = a.dataset.name  || '';
            const nb = b.dataset.name  || '';
            const ca = parseInt(a.dataset.count || '0', 10);
            const cb = parseInt(b.dataset.count || '0', 10);

            if (sort === 'name')      return na.localeCompare(nb);
            if (sort === 'name-desc') return nb.localeCompare(na);
            if (sort === 'count')     return cb - ca;
            return 0;
        });

        /* Re-append in sorted order */
        visible.forEach(c => grid.appendChild(c));

        /* No results message */
        if (noResults) {
            noResults.style.display = (visible.length === 0) ? 'block' : 'none';
        }
    }

    if (searchIn) searchIn.addEventListener('input',  filterAndSort);
    if (sortSel)  sortSel.addEventListener('change',  filterAndSort);
})();

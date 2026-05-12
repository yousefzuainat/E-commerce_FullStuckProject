/* ============================================================
   VOLTEX — Products Index JavaScript
   wwwroot/js/products.js
   ============================================================ */

(function initProductsPage() {
    const grid       = document.getElementById('prodGrid');
    const noResults  = document.getElementById('prodNoResults');
    const searchIn   = document.getElementById('prodSearch');
    const catFilter  = document.getElementById('prodCatFilter');
    const sortSel    = document.getElementById('prodSort');
    const gridBtn    = document.getElementById('viewGrid');
    const listBtn    = document.getElementById('viewList');

    /* ── View toggle (grid ↔ list) ── */
    if (gridBtn && listBtn && grid) {
        gridBtn.addEventListener('click', () => {
            grid.classList.remove('list-view');
            gridBtn.classList.add('active');
            listBtn.classList.remove('active');
            localStorage.setItem('voltex-prod-view', 'grid');
        });
        listBtn.addEventListener('click', () => {
            grid.classList.add('list-view');
            listBtn.classList.add('active');
            gridBtn.classList.remove('active');
            localStorage.setItem('voltex-prod-view', 'list');
        });
        /* Restore last view */
        if (localStorage.getItem('voltex-prod-view') === 'list') {
            listBtn.click();
        }
    }

    if (!grid) return; // empty-state page

    /* ── Filter + Sort ── */
    function getCards() {
        return Array.from(grid.querySelectorAll('.pag-card'));
    }

    function applyFilters() {
        const query = (searchIn  ? searchIn.value.toLowerCase().trim() : '');
        const cat   = (catFilter ? catFilter.value : '');
        const sort  = (sortSel  ? sortSel.value : 'name');

        let cards = getCards();

        /* Filter */
        cards.forEach(c => {
            const name     = (c.dataset.name     || '').toLowerCase();
            const brand    = (c.dataset.brand    || '').toLowerCase();
            const category = (c.dataset.category || '').toLowerCase();

            const matchQuery = !query || name.includes(query) || brand.includes(query);
            const matchCat   = !cat   || category === cat.toLowerCase();

            c.style.display = (matchQuery && matchCat) ? '' : 'none';
        });

        /* Sort visible */
        const visible = cards.filter(c => c.style.display !== 'none');
        visible.sort((a, b) => {
            const na = a.dataset.name  || '';
            const nb = b.dataset.name  || '';
            const pa = parseFloat(a.dataset.price || '0');
            const pb = parseFloat(b.dataset.price || '0');

            if (sort === 'name')         return na.localeCompare(nb);
            if (sort === 'name-desc')    return nb.localeCompare(na);
            if (sort === 'price-asc')    return pa - pb;
            if (sort === 'price-desc')   return pb - pa;
            return 0;
        });
        visible.forEach(c => grid.appendChild(c));

        if (noResults) {
            noResults.style.display = visible.length === 0 ? 'block' : 'none';
        }
    }

    if (searchIn)  searchIn.addEventListener('input',  applyFilters);
    if (catFilter) catFilter.addEventListener('change', applyFilters);
    if (sortSel)   sortSel.addEventListener('change',  applyFilters);
})();

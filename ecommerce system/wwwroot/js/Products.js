(function () {
    const searchIn = document.getElementById('prodSearch');
    const sortSel = document.getElementById('prodSort');
    const tableBody = document.getElementById('prodTableBody');
    const noResults = document.getElementById('prodNoResults');

    function filterAndSort() {
        if (!tableBody) return;

        const query = (searchIn ? searchIn.value.toLowerCase().trim() : '');
        const sort = (sortSel ? sortSel.value : 'name');

        // Get all rows that are products (exclude the empty state row if it exists)
        const rows = Array.from(tableBody.querySelectorAll('.prod-row'));

        /* ── 1. Filter ── */
        rows.forEach(row => {
            const name = (row.dataset.name || '').toLowerCase();
            const match = !query || name.includes(query);
            row.style.display = match ? '' : 'none';
        });

        /* ── 2. Sort visible rows ── */
        const visibleRows = rows.filter(row => row.style.display !== 'none');

        visibleRows.sort((a, b) => {
            const na = a.dataset.name || '';
            const nb = b.dataset.name || '';
            const pa = parseFloat(a.dataset.price || '0');
            const pb = parseFloat(b.dataset.price || '0');

            if (sort === 'name') return na.localeCompare(nb);
            if (sort === 'name-desc') return nb.localeCompare(na);
            if (sort === 'price-asc') return pa - pb;
            if (sort === 'price-desc') return pb - pa;
            return 0;
        });

        /* ── 3. Re-append in sorted order ── */
        visibleRows.forEach(row => tableBody.appendChild(row));

        /* ── 4. No Results Message ── */
        if (noResults) {
            noResults.style.display = (rows.length > 0 && visibleRows.length === 0) ? 'block' : 'none';
        }
    }

    if (searchIn) searchIn.addEventListener('input', filterAndSort);
    if (sortSel) sortSel.addEventListener('change', filterAndSort);
})();
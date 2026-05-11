/* ============================================================
   VOLTEX — Shared JavaScript
   wwwroot/js/voltex.js
   ============================================================ */

/* ── Theme copy (dark / light) ── */
const DC = {
    tag: '// Season 2026 Drop',
    title: '<span class="ht-dim">NEXT</span><br><span class="ht-orange">LEVEL</span><br><span class="ht-yellow">GEAR</span>',
    sub: 'Built for champions. From pro peripherals to precision tools — equipment engineered to keep you at the top of your game.',
    btn2: 'Explore Builds',
    banner: 'FLASH SALE<br>GAMING GEAR',
    nl: 'JOIN THE VOLTEX COMMUNITY',
    cart: 'CART',
    logo: 'VOLTEX'
};
const LC = {
    tag: 'New Season 2026',
    title: '<span class="ht-dim">Crafted</span><br><span class="ht-orange">for your</span><br><span class="ht-yellow">everyday</span>',
    sub: 'Thoughtfully curated tech accessories that fit seamlessly into modern life. Quality you can feel, style you can see.',
    btn2: 'View Lookbook',
    banner: 'Weekend<br>Sale Event',
    nl: 'JOIN THE VOLTEX FAMILY',
    cart: 'Cart',
    logo: 'Voltex'
};

/* ── Apply theme ── */
function setTheme(t) {
    document.documentElement.setAttribute('data-theme', t);
    localStorage.setItem('voltex-theme', t);
    const c = t === 'dark' ? DC : LC;

    // Update elements that exist on the current page
    const safe = (id, fn) => { const el = document.getElementById(id); if (el) fn(el); };
    safe('hTag',    el => el.textContent = c.tag);
    safe('hTitle',  el => el.innerHTML   = c.title);
    safe('hSub',    el => el.textContent = c.sub);
    safe('btn2Txt', el => el.textContent = c.btn2);
    safe('bTtl',    el => el.innerHTML   = c.banner);
    safe('nlTtl',   el => el.textContent = c.nl);
    safe('cLbl',    el => el.textContent = c.cart);
    safe('logoTxt', el => el.textContent = c.logo);
}

/* ── Init theme on page load ── */
(function initTheme() {
    const saved = localStorage.getItem('voltex-theme') || 'dark';
    document.documentElement.setAttribute('data-theme', saved);
    const tog = document.getElementById('togIn');
    if (tog && saved === 'light') tog.checked = true;
    setTheme(saved);
})();

/* ── Search overlay ── */
function openS()  { document.getElementById('sOv').classList.add('open'); setTimeout(() => document.getElementById('sIn').focus(), 80); }
function closeS() { document.getElementById('sOv').classList.remove('open'); }
document.addEventListener('keydown', e => { if (e.key === 'Escape') closeS(); });

/* ── Scroll-reveal ── */
(function initReveal() {
    const io = new IntersectionObserver(entries => {
        entries.forEach((e, i) => {
            if (e.isIntersecting) {
                setTimeout(() => e.target.classList.add('in'), i * 60);
                io.unobserve(e.target);
            }
        });
    }, { threshold: 0.06 });

    document.querySelectorAll('.reveal, .prod-card, .feat-card, .cat-card, .cag-card, .cat-stat-card')
        .forEach(el => { el.classList.add('reveal'); io.observe(el); });
})();

/* ── Image Preview Logic ── */
function initImagePreview(inputId, previewId, placeholderId, secondaryPreviewId) {
    const input = document.getElementById(inputId);
    const preview = document.getElementById(previewId);
    const placeholder = document.getElementById(placeholderId);

    if (!input || !preview) return;

    input.addEventListener('change', function () {
        const file = this.files[0];
        if (file) {
            const reader = new FileReader();

            reader.onload = function (e) {
                // Update the main preview image
                preview.src = e.target.result;
                preview.style.display = 'block';

                // Hide the placeholder UI
                if (placeholder) {
                    placeholder.style.display = 'none';
                }

                // Add a little 'pop' animation to match your Voltex theme
                preview.classList.add('reveal', 'in');
            };

            reader.readAsDataURL(file);
        } else {
            // Reset if user clears selection
            preview.src = "#";
            preview.style.display = 'none';
            if (placeholder) {
                placeholder.style.display = 'flex';
            }
        }
    });
}
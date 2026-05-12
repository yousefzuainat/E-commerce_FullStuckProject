/* ============================================================
   VOLTEX — Product Details JavaScript
   wwwroot/js/products-details.js
   ============================================================ */

(function initProductDetails() {

    /* ── Quantity stepper ── */
    const qtyNum  = document.getElementById('pdQty');
    const qtyMinus = document.getElementById('pdQtyMinus');
    const qtyPlus  = document.getElementById('pdQtyPlus');

    if (qtyNum && qtyMinus && qtyPlus) {
        qtyMinus.addEventListener('click', () => {
            const v = parseInt(qtyNum.value, 10);
            if (v > 1) qtyNum.value = v - 1;
        });
        qtyPlus.addEventListener('click', () => {
            const v = parseInt(qtyNum.value, 10);
            qtyNum.value = v + 1;
        });
        qtyNum.addEventListener('change', () => {
            const v = parseInt(qtyNum.value, 10);
            if (isNaN(v) || v < 1) qtyNum.value = 1;
        });
    }

    /* ── Thumbnail gallery switcher ── */
    const thumbs  = document.querySelectorAll('.pd-thumb-item');
    const mainImg = document.getElementById('pdMainMedia');

    if (thumbs.length && mainImg) {
        thumbs.forEach(thumb => {
            thumb.addEventListener('click', () => {
                thumbs.forEach(t => t.classList.remove('active'));
                thumb.classList.add('active');

                const src   = thumb.dataset.src;
                const emoji = thumb.dataset.emoji;

                if (src) {
                    mainImg.innerHTML = `<img src="${src}" alt="product" />`;
                } else if (emoji) {
                    mainImg.innerHTML = `<span class="pd-main-emoji">${emoji}</span>`;
                }
            });
        });
    }

    /* ── Wishlist toggle ── */
    const wishBtn = document.getElementById('pdWish');
    if (wishBtn) {
        wishBtn.addEventListener('click', () => {
            const isWished = wishBtn.classList.toggle('wished');
            wishBtn.textContent = isWished ? '♥' : '♡';
            wishBtn.style.color = isWished ? 'var(--accent)' : '';
        });
    }

})();

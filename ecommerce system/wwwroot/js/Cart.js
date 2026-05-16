// ── Add to Cart ────────────────────────────────────
async function addToCart(btn) {
    const productId = btn.dataset.productId;
    if (!productId) return;

    // Read quantity from the product-detail input
    const qtyInput = document.getElementById('pdQty');
    const quantity = qtyInput ? Math.max(1, parseInt(qtyInput.value, 10) || 1) : 1;

    btn.disabled = true;
    btn.textContent = '✓';

    try {
        const res = await fetch('/Cart/Add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            // Send the real quantity here
            body: 'productId=' + encodeURIComponent(productId) + '&quantity=' + encodeURIComponent(quantity)
        });
        const data = await res.json();
        if (data.success) {
            document.querySelectorAll('.cart-n').forEach(el => {
                el.textContent = data.cartCount;
                el.classList.add('cart-n-active');
            });
            showCartToast('Added ' + quantity + ' to cart!');
        }
    } catch (e) { }

    setTimeout(() => {
        btn.disabled = false;
    }, 1200);
}

// ── Quantity Controls ──────────────────────────────
(function () {
    const qtyInput = document.getElementById('pdQty');
    const btnMinus = document.getElementById('pdQtyMinus');
    const btnPlus = document.getElementById('pdQtyPlus');

    if (!qtyInput) return;

    const update = (delta) => {
        let val = parseInt(qtyInput.value, 10) || 1;
        val = Math.max(1, val + delta);   // never go below 1
        qtyInput.value = val;
    };

    btnMinus?.addEventListener('click', () => update(-1));
    btnPlus?.addEventListener('click', () => update(+1));

    // Optional: prevent manual entry of 0 or negatives
    qtyInput.addEventListener('change', () => {
        let val = parseInt(qtyInput.value, 10) || 1;
        qtyInput.value = Math.max(1, val);
    });
})();
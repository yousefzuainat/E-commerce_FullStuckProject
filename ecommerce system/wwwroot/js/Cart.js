// ── Add to Cart ────────────────────────────────────
async function addToCart(btn) {
    const productId = btn.dataset.productId;
    if (!productId) return;

    // Read quantity from the product-detail input
    const qtyInput = document.getElementById('pdQty');
    const quantity = qtyInput ? Math.max(1, parseInt(qtyInput.value, 10) || 1) : 1;

    // Save the original text to restore it later
    const originalText = btn.textContent;
    btn.disabled = true;
    btn.textContent = '✓';

    try {
        const res = await fetch('/Cart/Add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: 'productId=' + encodeURIComponent(productId) + '&quantity=' + encodeURIComponent(quantity)
        });

        const data = await res.json();

        if (data.success) {
            document.querySelectorAll('.cart-n').forEach(el => {
                el.textContent = data.cartCount;
                el.classList.add('cart-n-active');
            });

            // SweetAlert2 Success Toast
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: 'success',
                title: `Added ${quantity} item(s) to cart!`,
                showConfirmButton: false,
                timer: 2500,
                timerProgressBar: true
            });
        } else {
            // Server returned success: false
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: data.message || 'Could not add item to cart.',
                confirmButtonColor: '#3085d6'
            });
        }
    } catch (e) {
        // Network or parsing error
        Swal.fire({
            icon: 'error',
            title: 'Connection Error',
            text: 'Something went wrong. Please check your connection and try again.',
            confirmButtonColor: '#3085d6'
        });
    }

    setTimeout(() => {
        btn.disabled = false;
        btn.textContent = originalText; // Restores the button back to "Add to Cart"
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

    qtyInput.addEventListener('change', () => {
        let val = parseInt(qtyInput.value, 10) || 1;
        qtyInput.value = Math.max(1, val);
    });
})();
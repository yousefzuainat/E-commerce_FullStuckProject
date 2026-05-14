// ── Add to Cart ────────────────────────────────────
async function addToCart(btn) {
    const productId = btn.dataset.productId;
    if (!productId) return;

    btn.disabled = true;
    btn.textContent = '✓';

    try {
        const res = await fetch('/Cart/Add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: 'productId=' + productId + '&quantity=1'
        });
        const data = await res.json();
        if (data.success) {
            // Update badge
            document.querySelectorAll('.cart-n').forEach(el => {
                el.textContent = data.cartCount;
                el.classList.add('cart-n-active');
            });
            // Show toast
            showCartToast('Added to cart!');
        }
    } catch (e) { }

    setTimeout(() => {
        btn.disabled = false;
        btn.textContent = '+';
    }, 1200);
}

function showCartToast(msg) {
    const t = document.createElement('div');
    t.className = 'cart-toast';
    t.style.cssText = 'position:fixed;bottom:32px;right:32px;z-index:9999;padding:14px 24px;font-size:13px;letter-spacing:1px;opacity:0;transform:translateY(12px);transition:opacity .3s,transform .3s;pointer-events:none;background:var(--surface);border:1px solid var(--border);border-left:3px solid var(--accent);color:var(--text);font-family:var(--body,sans-serif);';
    t.textContent = msg;
    document.body.appendChild(t);
    requestAnimationFrame(() => { t.style.opacity = '1'; t.style.transform = 'translateY(0)'; });
    setTimeout(() => {
        t.style.opacity = '0';
        t.style.transform = 'translateY(12px)';
        setTimeout(() => t.remove(), 350);
    }, 2000);
}

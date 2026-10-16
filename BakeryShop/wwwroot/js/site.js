// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// AJAX: thêm vào giỏ không tải lại trang
document.addEventListener('submit', async function (e) {
    const form = e.target.closest('.js-add-cart');
    if (!form) return;
    e.preventDefault();

    const btn = form.querySelector('button[type=submit]');
    const oldText = btn.textContent;
    btn.disabled = true;
    btn.textContent = 'Đang thêm...';

    try {
        const res = await fetch(form.action, {
            method: 'POST',
            body: new FormData(form),           // gồm cả antiforgery token
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        // Chưa đăng nhập: server chuyển hướng sang trang đăng nhập
        if (res.redirected) { window.location.href = res.url; return; }

        const data = await res.json();
        if (data.success) {
            document.getElementById('cart-count').textContent = data.cartCount;
            btn.textContent = 'Đã thêm ✓';
            setTimeout(() => { btn.textContent = oldText; btn.disabled = false; }, 1000);
            return;
        }
        alert(data.message || 'Không thêm được sản phẩm');
    } catch (err) {
        alert('Có lỗi kết nối, vui lòng thử lại');
    }
    btn.textContent = oldText;
    btn.disabled = false;
});

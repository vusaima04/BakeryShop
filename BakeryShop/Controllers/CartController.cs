using BakeryShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Controllers
{
    [Authorize] // Bắt buộc phải đăng nhập mới vào được bất kỳ action nào trong Controller này
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cart -> Hiển thị giỏ hàng của user đang đăng nhập
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(cartItems);
        }

        // POST: Cart/Add -> Thêm sản phẩm vào giỏ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            quantity = Math.Clamp(quantity, 1, 20);
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (!await _context.Products.AnyAsync(p => p.Id == productId))
                return isAjax ? NotFound(new { success = false, message = "Sản phẩm không tồn tại" }) : NotFound();

            var userId = _userManager.GetUserId(User)!;

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

            if (existingItem != null)
                existingItem.Quantity += quantity;
            else
                _context.CartItems.Add(new CartItem { ProductId = productId, UserId = userId, Quantity = quantity });

            await _context.SaveChangesAsync();

            if (isAjax)
            {
                var count = await _context.CartItems.Where(c => c.UserId == userId).SumAsync(c => c.Quantity);
                return Json(new { success = true, cartCount = count });
            }
            return RedirectToAction("Index", "Home");
        }
        // POST: Cart/Update -> Đổi số lượng 1 dòng trong giỏ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int quantity, int delta = 0)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (item != null)
            {
                item.Quantity = Math.Clamp(quantity + delta, 1, 20);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        // POST: Cart/Remove -> Xóa 1 sản phẩm khỏi giỏ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
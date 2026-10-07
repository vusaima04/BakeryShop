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
        public async Task<IActionResult> Add(int productId)
        {
            var userId = _userManager.GetUserId(User);

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

            if (existingItem != null)
            {
                existingItem.Quantity += 1;
            }
            else
            {
                var newItem = new CartItem
                {
                    ProductId = productId,
                    UserId = userId!,
                    Quantity = 1
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        // POST: Cart/Remove -> Xóa 1 sản phẩm khỏi giỏ
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
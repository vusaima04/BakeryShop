using BakeryShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Order/Checkout -> Hiển thị form nhập thông tin giao hàng
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            return View(cartItems);
        }

        // POST: Order/PlaceOrder -> Xử lý tạo đơn hàng thật
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string deliveryAddress, string receiverPhone, DateTime receiveDate, string? note)
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var order = new Order
            {
                UserId = userId!,
                DeliveryAddress = deliveryAddress,
                ReceiverPhone = receiverPhone,
                ReceiveDate = receiveDate,
                Note = note,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                TotalAmount = cartItems.Sum(c => c.Product!.Price * c.Quantity)
            };

            foreach (var item in cartItems)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product!.Price // Snapshot giá tại thời điểm đặt hàng
                });
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems); // Xóa giỏ hàng sau khi đặt xong

            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { id = order.Id });
        }

        // GET: Order/Success -> Trang xác nhận đặt hàng thành công
        public async Task<IActionResult> Success(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return View(order);
        }
    }
}
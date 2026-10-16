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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel input)
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

            if (input.ReceiveDate.HasValue && input.ReceiveDate.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(input.ReceiveDate), "Ngày nhận không được ở quá khứ");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Input = input; // giữ lại dữ liệu khách đã nhập
                return View("Checkout", cartItems);
            }

            var order = new Order
            {
                UserId = userId!,
                DeliveryAddress = input.DeliveryAddress,
                ReceiverPhone = input.ReceiverPhone,
                ReceiveDate = input.ReceiveDate!.Value,
                Note = input.Note,
                Status = OrderStatus.Pending,
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
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { id = order.Id });
        }

        // GET: Order/Success -> Trang xác nhận đặt hàng thành công
        public async Task<IActionResult> Success(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        // GET: Order/History -> Danh sách đơn của chính user đang đăng nhập
        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // GET: Order/Details/5 -> Chi tiết 1 đơn của chính user đó
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
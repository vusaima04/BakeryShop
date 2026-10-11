using BakeryShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BakeryShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();

            var featured = await _context.Products.Take(4).ToListAsync();
            return View(featured);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        // GET: Home/Menu -> Trang liệt kê TẤT CẢ sản phẩm, nhóm theo danh mục
        public async Task<IActionResult> Menu()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();

            return View(categories);
        }

        // GET: Home/Category/5 -> Chỉ hiện sản phẩm thuộc 1 danh mục cụ thể
        public async Task<IActionResult> Category(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
    }

}

using BakeryShop.Data;
using Microsoft.AspNetCore.Localization;
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
            ViewBag.Categories = await _context.Categories.AsNoTracking()
                .Where(c => c.Name != SeedData.ComboCategory).OrderBy(c => c.Id).ToListAsync();

            ViewBag.Combos = await _context.Products.AsNoTracking()
                .Where(p => p.Category!.Name == SeedData.ComboCategory).OrderBy(p => p.Id).ToListAsync();

            var featured = await _context.Products.AsNoTracking()
                .Where(p => p.Category!.Name != SeedData.ComboCategory).Take(4).ToListAsync();
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
        // GET: Home/Detail/5 -> Trang chi tiết sản phẩm cho khách
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products.AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            // SESSION: nhớ các sản phẩm đã xem gần đây (mới nhất đứng đầu, tối đa 7)
            var viewed = (HttpContext.Session.GetString("RecentlyViewed") ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var n) ? n : 0)
                .Where(n => n > 0)
                .ToList();
            viewed.Remove(id);
            viewed.Insert(0, id);
            if (viewed.Count > 7) viewed = viewed.Take(7).ToList();
            HttpContext.Session.SetString("RecentlyViewed", string.Join(',', viewed));

            // Các sản phẩm đã xem trước đó (không tính sản phẩm đang xem)
            var recentIds = viewed.Where(v => v != id).Take(4).ToList();
            var recentProducts = await _context.Products.AsNoTracking()
                .Where(p => recentIds.Contains(p.Id)).ToListAsync();
            ViewBag.Recent = recentIds
                .Select(rid => recentProducts.FirstOrDefault(p => p.Id == rid))
                .Where(p => p != null)
                .ToList();

            // Sản phẩm cùng danh mục
            ViewBag.Related = await _context.Products.AsNoTracking()
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(4).ToListAsync();

            return View(product);
        }
        public async Task<IActionResult> Menu(string? search, int? categoryId, string? sort, int page = 1)
        {
            const int pageSize = 9;
            if (page < 1) page = 1;

            var query = _context.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search));
            if (categoryId > 0)
                query = query.Where(p => p.CategoryId == categoryId);

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name)
            };

            var total = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            if (page > totalPages) page = totalPages;

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Categories = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Sort = sort;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;

            return View(items);
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
        // GET: Home/SetLanguage?culture=en-US&returnUrl=/Home/Menu
        public IActionResult SetLanguage(string culture, string? returnUrl)
        {
            var supported = new[] { "vi-VN", "en-US" };
            if (!supported.Contains(culture)) culture = "vi-VN";

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
        }
    }

}

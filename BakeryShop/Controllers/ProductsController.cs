using BakeryShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    private readonly IWebHostEnvironment _env;

    public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }
    private async Task LoadCategoriesAsync(int? selectedId = null)
    {
        var list = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        ViewBag.Categories = new SelectList(list, "Id", "Name", selectedId);
    }
    // GET: PRODUCTS
    // GET: PRODUCTS?search=&categoryId=&page=
    public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
    {
        const int pageSize = 10;
        if (page < 1) page = 1;

        var query = _context.Products.AsNoTracking().Include(p => p.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));
        if (categoryId > 0)
            query = query.Where(p => p.CategoryId == categoryId);

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        if (page > totalPages) page = totalPages;

        var items = await query.OrderBy(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Categories = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.Total = total;
        return View(items);
    }

    // GET: PRODUCTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (product == null) return NotFound();

        return View(product);
    }

    // GET: PRODUCTS/Create
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View();
    }

    // POST: PRODUCTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,ImageUrl,Size,CategoryId")] Product product, IFormFile? imageFile)
    {
        var hasNewImage = imageFile != null && imageFile.Length > 0;
        if (hasNewImage)
        {
            if (!IsValidImage(imageFile!))
                ModelState.AddModelError("ImageUrl", "Ảnh phải là jpg/png/webp/gif và tối đa 2MB");
            else
                ModelState.Remove(nameof(Product.ImageUrl));   // link ảnh sẽ được gán sau khi form hợp lệ
        }
        if (ModelState.IsValid)
        {
            if (hasNewImage) product.ImageUrl = (await SaveImageAsync(imageFile!))!;
            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await LoadCategoriesAsync(product.CategoryId);
        return View(product);
    }
    private static bool IsValidImage(IFormFile file)
    {
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowed.Contains(ext) && file.Length <= 2 * 1024 * 1024;
    }

    // GET: PRODUCTS/Edit/5
    public async Task<IActionResult> Edit(int? id, string? returnUrl)
    {
        if (id == null) return NotFound();
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        ViewBag.ReturnUrl = returnUrl;
        await LoadCategoriesAsync(product.CategoryId);
        return View(product);
    }

    // POST: PRODUCTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
    [Bind("Id,Name,Description,Price,ImageUrl,Size,CategoryId")] Product product,
    IFormFile? imageFile, string? returnUrl)
    {
        if (id != product.Id) return NotFound();
        var oldUrl = await _context.Products.AsNoTracking()
    .Where(p => p.Id == id).Select(p => p.ImageUrl).FirstOrDefaultAsync();
        var hasNewImage = imageFile != null && imageFile.Length > 0;
        if (hasNewImage)
        {
            if (!IsValidImage(imageFile!))
                ModelState.AddModelError("ImageUrl", "Ảnh phải là jpg/png/webp/gif và tối đa 2MB");
            else
                ModelState.Remove(nameof(Product.ImageUrl));   // link ảnh sẽ được gán sau khi form hợp lệ
        }

        if (ModelState.IsValid)
        {

            try
            {
                if (hasNewImage) product.ImageUrl = (await SaveImageAsync(imageFile!))!;
                _context.Update(product);
                await _context.SaveChangesAsync();
                if (oldUrl != product.ImageUrl) await DeleteImageIfUnusedAsync(oldUrl);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == product.Id)) return NotFound();
                throw;
            }
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
        }
        ViewBag.ReturnUrl = returnUrl;
        await LoadCategoriesAsync(product.CategoryId);
        return View(product);
    }
    // GET: PRODUCTS/Delete/5
    public async Task<IActionResult> Delete(int? id, string? returnUrl)
    {
        if (id == null) return NotFound();
        var product = await _context.Products.AsNoTracking()
            .Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();

        ViewBag.ReturnUrl = returnUrl;
        return View(product);
    }
    // POST: PRODUCTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, string? returnUrl)
    {
        IActionResult Back() => Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));

        var product = await _context.Products.FindAsync(id);
        if (product == null) return Back();

        // Không xóa sản phẩm đã có trong đơn hàng để giữ lịch sử đơn
        if (await _context.OrderDetails.AnyAsync(d => d.ProductId == id))
        {
            TempData["Error"] = $"Không thể xóa '{product.Name}' vì sản phẩm đã có trong đơn hàng.";
            return Back();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        await DeleteImageIfUnusedAsync(product.ImageUrl);
        TempData["Success"] = $"Đã xóa '{product.Name}'.";
        return Back();
    }

    private bool ProductExists(int? id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
    private async Task<string?> SaveImageAsync(IFormFile file)
    {
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext) || file.Length > 2 * 1024 * 1024) return null;

        var folder = Path.Combine(_env.WebRootPath, "Images", "products");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid()}{ext}";
        using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
        await file.CopyToAsync(stream);
        return "/Images/products/" + fileName;
    }
    // Xóa file ảnh cũ nếu nó nằm trong Images/products và không còn sản phẩm nào dùng
    private async Task DeleteImageIfUnusedAsync(string? url)
    {
        if (string.IsNullOrEmpty(url) || !url.StartsWith("/Images/products/")) return;
        if (await _context.Products.AnyAsync(p => p.ImageUrl == url)) return;

        var path = Path.Combine(_env.WebRootPath, "Images", "products", Path.GetFileName(url));
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

    }
    // POST: PRODUCTS/CleanupImages
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CleanupImages()
    {
        var used = (await _context.Products.Select(p => p.ImageUrl).ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var folder = Path.Combine(_env.WebRootPath, "Images", "products");
        var removed = 0;
        if (Directory.Exists(folder))
        {
            foreach (var file in Directory.GetFiles(folder))
            {
                var isGuidName = Guid.TryParse(Path.GetFileNameWithoutExtension(file), out _);
                if (isGuidName && !used.Contains("/Images/products/" + Path.GetFileName(file)))
                {
                    System.IO.File.Delete(file);
                    removed++;
                }
            }
        }
        TempData["Success"] = $"Đã dọn {removed} ảnh không còn được dùng.";
        return RedirectToAction(nameof(Index));
    }
}

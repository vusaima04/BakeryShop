using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BakeryShop.Data;

public static class SeedData
{
    public const string ComboCategory = "Combo";

    public static async Task SeedCatalogAsync(ApplicationDbContext context, string webRootPath)
    {
        var catalog = new Dictionary<string, (string Name, string Desc, decimal Price, string Size)[]>
        {
            ["Bánh Sinh Nhật"] = new[]
            {
                ("Berry Fruit Vanilla", "Bánh mousse vị vani, quả mọng", 625000m, "14cm / 18cm"),
                ("Pink Guava", "Bánh mousse vị ổi hồng", 625000m, "14cm / 18cm"),
                ("Tropical Coconut", "Bánh mousse vị dứa dừa", 625000m, "14cm / 18cm"),
                ("Chocolate Truffle", "Bánh socola đậm vị, lớp ganache mịn", 680000m, "14cm / 18cm"),
                ("Tiramisu Cake", "Bánh tiramisu cà phê mascarpone", 650000m, "14cm / 18cm"),
                ("Matcha Red Bean", "Bánh trà xanh nhân đậu đỏ", 640000m, "14cm / 18cm"),
            },
            ["Bánh Nhỏ"] = new[]
            {
                ("Mini Tart Dâu", "Bánh tart nhỏ nhân dâu tươi", 65000m, "1 phần"),
                ("Mini Eclair", "Bánh su kem kiểu Pháp", 55000m, "1 phần"),
                ("Macaron Mix", "Hộp macaron nhiều vị", 180000m, "Hộp 6 cái"),
                ("Cupcake Vani", "Cupcake vani kem bơ", 45000m, "1 cái"),
                ("Panna Cotta Xoài", "Panna cotta sốt xoài", 60000m, "1 ly"),
                ("Bánh Su Kem Mini", "Su kem nhân custard", 12000m, "1 cái"),
            },
            ["Bánh Nướng"] = new[]
            {
                ("Bánh Mì Bơ Tỏi", "Bánh nướng giòn thơm bơ tỏi", 45000m, "1 cái"),
                ("Croissant Bơ", "Bánh sừng bò nhiều lớp, thơm bơ", 38000m, "1 cái"),
                ("Bánh Quy Bơ", "Bánh quy bơ giòn tan", 95000m, "Hộp 250g"),
                ("Pain au Chocolat", "Bánh sừng bò nhân socola", 42000m, "1 cái"),
                ("Bánh Mì Nguyên Cám", "Bánh mì nguyên cám ít đường", 55000m, "1 ổ"),
                ("Cheese Tart", "Tart phô mai nướng", 48000m, "1 cái"),
            },
        };

        foreach (var (categoryName, products) in catalog)
        {
            var category = await EnsureCategoryAsync(context, categoryName);
            foreach (var (name, desc, price, size) in products)
                await EnsureProductAsync(context, webRootPath, category.Id, name, desc, price, size, null);
        }

        // Combo & ưu đãi: sản phẩm thật, có ảnh cố định
        var combo = await EnsureCategoryAsync(context, ComboCategory);
        var combos = new[]
        {
            ("Combo Sinh Nhật", "1 bánh sinh nhật 14cm + 6 bánh nhỏ. Tiết kiệm 10% so với mua lẻ", 790000m, "Cho 6-8 người", "/Images/combo/combo-sinh-nhat.png"),
            ("Combo Trà Chiều", "4 bánh nhỏ tuyển chọn + 1 hộp macaron. Tiết kiệm 15%", 360000m, "Cho 2-3 người", "/Images/combo/combo-tra-chieu.png"),
            ("Combo Gia Đình", "1 bánh sinh nhật 18cm + 1 hộp bánh quy bơ. Tiết kiệm 10%", 690000m, "Cho 8-10 người", "/Images/combo/combo-gia-dinh.png"),
            ("Combo Quà Tặng", "Hộp quà macaron + bánh quy bơ, gói quà sẵn", 320000m, "1 hộp quà", "/Images/combo/combo-qua-tang.png"),
            ("Combo Cuối Tuần", "2 croissant + 2 pain au chocolat + 1 cheese tart", 180000m, "5 bánh", "/Images/combo/combo-cuoi-tuan.png"),
        };
        foreach (var (name, desc, price, size, image) in combos)
            await EnsureProductAsync(context, webRootPath, combo.Id, name, desc, price, size, image);

        await context.SaveChangesAsync();
    }

    private static async Task<Category> EnsureCategoryAsync(ApplicationDbContext context, string name)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == name);
        if (category == null)
        {
            category = new Category { Name = name };
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        }
        return category;
    }

    private static async Task EnsureProductAsync(ApplicationDbContext context, string webRootPath,
        int categoryId, string name, string desc, decimal price, string size, string? fixedImage)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.Name == name);
        var isNew = product == null;
        if (isNew)
        {
            product = new Product
            {
                Name = name,
                Description = desc,
                Price = price,
                Size = size,
                CategoryId = categoryId,
                ImageUrl = "https://placehold.co/400x300/bc9669/ffffff?text=" + Uri.EscapeDataString(name)
            };
            context.Products.Add(product);
        }

        // Ảnh: ưu tiên ảnh cố định (combo), sau đó tự tìm file Images/products/<slug>.jpg|png|webp
        // Chỉ thay khi sản phẩm mới hoặc đang dùng ảnh tạm placehold.co (không đè ảnh admin đã upload)
        if (isNew || product!.ImageUrl.StartsWith("https://placehold.co"))
        {
            var local = fixedImage ?? FindLocalImage(webRootPath, name);
            if (local != null) product!.ImageUrl = local;
        }
    }

    private static string? FindLocalImage(string webRootPath, string productName)
    {
        var slug = Slug(productName);
        foreach (var ext in new[] { "jpg", "jpeg", "png", "webp" })
        {
            var path = Path.Combine(webRootPath, "Images", "products", $"{slug}.{ext}");
            if (File.Exists(path)) return $"/Images/products/{slug}.{ext}";
        }
        return null;
    }

    private static string Slug(string text)
    {
        var sb = new StringBuilder();
        foreach (var c in text.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        var s = sb.ToString().Replace('đ', 'd').Replace('Đ', 'D').ToLowerInvariant();
        return Regex.Replace(s, "[^a-z0-9]+", "-").Trim('-');
    }
}
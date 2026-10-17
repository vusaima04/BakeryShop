using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Data;

public static class SeedData
{
    public static async Task SeedCatalogAsync(ApplicationDbContext context)
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
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);
            if (category == null)
            {
                category = new Category { Name = categoryName };
                context.Categories.Add(category);
                await context.SaveChangesAsync();
            }

            foreach (var (name, desc, price, size) in products)
            {
                if (await context.Products.AnyAsync(p => p.Name == name)) continue;

                context.Products.Add(new Product
                {
                    Name = name,
                    Description = desc,
                    Price = price,
                    Size = size,
                    CategoryId = category.Id,
                    ImageUrl = "https://placehold.co/400x300/bc9669/ffffff?text=" + Uri.EscapeDataString(name)
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
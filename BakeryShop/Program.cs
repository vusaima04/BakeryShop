using BakeryShop.Data;
using BakeryShop.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
builder.Services.AddTransient<IEmailSender, FakeEmailSender>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Tự tạo/cập nhật database theo các Migration có sẵn
    await context.Database.MigrateAsync();

    // Role
    foreach (var roleName in new[] { "Admin", "Customer" })
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Danh mục + sản phẩm mẫu (chỉ thêm khi chưa có danh mục nào)
    if (!await context.Categories.AnyAsync())
    {
        var birthday = new Category { Name = "Bánh Sinh Nhật" };
        var small = new Category { Name = "Bánh Nhỏ" };
        var baked = new Category { Name = "Bánh Nướng" };

        context.Products.AddRange(
            new Product { Name = "Berry Fruit Vanilla", Description = "Bánh mousse vị vani, quả mọng", Price = 625000, Size = "14cm / 18cm", Category = birthday, ImageUrl = "https://placehold.co/400x300/bc9669/ffffff?text=Berry+Fruit+Vanilla" },
            new Product { Name = "Pink Guava", Description = "Bánh mousse vị ổi hồng", Price = 625000, Size = "14cm / 18cm", Category = birthday, ImageUrl = "https://placehold.co/400x300/bc9669/ffffff?text=Pink+Guava" },
            new Product { Name = "Tropical Coconut", Description = "Bánh mousse vị dứa dừa", Price = 625000, Size = "14cm / 18cm", Category = birthday, ImageUrl = "https://placehold.co/400x300/bc9669/ffffff?text=Tropical+Coconut" },
            new Product { Name = "Mini Tart Dâu", Description = "Bánh tart nhỏ nhân dâu tươi", Price = 65000, Size = "1 phần", Category = small, ImageUrl = "https://placehold.co/400x300/9c7a52/ffffff?text=Mini+Tart+Dau" },
            new Product { Name = "Mini Eclair", Description = "Bánh su kem kiểu Pháp", Price = 55000, Size = "1 phần", Category = small, ImageUrl = "https://placehold.co/400x300/9c7a52/ffffff?text=Mini+Eclair" },
            new Product { Name = "Bánh Mì Bơ Tỏi", Description = "Bánh nướng giòn thơm bơ tỏi", Price = 45000, Size = "1 cái", Category = baked, ImageUrl = "https://placehold.co/400x300/3a3229/ffffff?text=Banh+Mi+Bo+Toi" }
        );
        await context.SaveChangesAsync();
    }

    // Tài khoản Admin mặc định (dùng để demo)
    const string adminEmail = "admin@bakeryshop.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Quản trị viên",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, "Admin@123456");
        if (result.Succeeded)
        {
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "Customer" });
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
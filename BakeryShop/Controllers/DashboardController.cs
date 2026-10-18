using BakeryShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Controllers;

[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    public DashboardController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        // Đơn bị hủy không tính vào doanh thu
        var validOrders = _context.Orders.AsNoTracking().Where(o => o.Status != OrderStatus.Cancelled);

        ViewBag.TotalRevenue = await validOrders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
        ViewBag.TotalOrders = await _context.Orders.CountAsync();
        ViewBag.TotalProducts = await _context.Products.CountAsync();
        ViewBag.TotalUsers = await _context.Users.CountAsync();

        // Số đơn theo trạng thái
        var byStatus = await _context.Orders.AsNoTracking()
            .GroupBy(o => o.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();
        ViewBag.StatusLabels = OrderStatus.All.Select(OrderStatus.Label).ToList();
        ViewBag.StatusCounts = OrderStatus.All
            .Select(s => byStatus.FirstOrDefault(x => x.Key == s)?.Count ?? 0).ToList();

        // Top 5 sản phẩm bán chạy
        var top = await _context.OrderDetails.AsNoTracking()
            .Where(d => d.Order!.Status != OrderStatus.Cancelled)
            .GroupBy(d => new { d.ProductId, d.Product!.Name })
            .Select(g => new { g.Key.Name, Qty = g.Sum(d => d.Quantity) })
            .OrderByDescending(x => x.Qty)
            .Take(5)
            .ToListAsync();
        ViewBag.TopNames = top.Select(t => t.Name).ToList();
        ViewBag.TopQty = top.Select(t => t.Qty).ToList();

        // Doanh thu 6 tháng gần nhất
        var since = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
        var raw = await validOrders.Where(o => o.CreatedAt >= since)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(o => o.TotalAmount) })
            .ToListAsync();
        var months = Enumerable.Range(0, 6).Select(i => since.AddMonths(i)).ToList();
        ViewBag.MonthLabels = months.Select(m => m.ToString("MM/yyyy")).ToList();
        ViewBag.MonthRevenue = months
            .Select(m => raw.FirstOrDefault(r => r.Year == m.Year && r.Month == m.Month)?.Total ?? 0m).ToList();

        return View();
    }
}
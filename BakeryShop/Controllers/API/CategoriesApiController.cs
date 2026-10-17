using BakeryShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryShop.Controllers.Api
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CategoriesApiController(ApplicationDbContext context) => _context = context;

        // GET api/categories
        [HttpGet]
        public async Task<ActionResult> GetAll() =>
            Ok(await _context.Categories.AsNoTracking().OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name, productCount = c.Products.Count })
                .ToListAsync());
    }
}
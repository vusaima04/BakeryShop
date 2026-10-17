using BakeryShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BakeryShop.Controllers.Api
{
    public record ProductDto(int Id, string Name, string Description, decimal Price,
                             string ImageUrl, string Size, int CategoryId, string? CategoryName);

    public class ProductInput
    {
        [Required, StringLength(150)] public string Name { get; set; } = "";
        [Required, StringLength(500)] public string Description { get; set; } = "";
        [Range(1000, 100000000)] public decimal Price { get; set; }
        [Required, StringLength(500)] public string ImageUrl { get; set; } = "";
        [Required, StringLength(50)] public string Size { get; set; } = "";
        [Range(1, int.MaxValue)] public int CategoryId { get; set; }
    }

    [ApiController]
    [Route("api/products")]
    public class ProductsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProductsApiController(ApplicationDbContext context) => _context = context;

        // GET api/products?search=&categoryId=&page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult> GetAll(string? search, int? categoryId, int page = 1, int pageSize = 10)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _context.Products.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Name.Contains(search));
            if (categoryId > 0) query = query.Where(p => p.CategoryId == categoryId);

            var total = await query.CountAsync();
            var items = await query.OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.ImageUrl,
                                            p.Size, p.CategoryId, p.Category!.Name))
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        // GET api/products/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            var p = await _context.Products.AsNoTracking().Include(x => x.Category)
                        .FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            return new ProductDto(p.Id, p.Name, p.Description, p.Price, p.ImageUrl, p.Size, p.CategoryId, p.Category?.Name);
        }

        // POST api/products  (chỉ Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> Create(ProductInput input)
        {
            if (!await _context.Categories.AnyAsync(c => c.Id == input.CategoryId))
                return BadRequest(new { message = "Danh mục không tồn tại" });

            var p = new Product
            {
                Name = input.Name,
                Description = input.Description,
                Price = input.Price,
                ImageUrl = input.ImageUrl,
                Size = input.Size,
                CategoryId = input.CategoryId
            };
            _context.Products.Add(p);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = p.Id },
                new ProductDto(p.Id, p.Name, p.Description, p.Price, p.ImageUrl, p.Size, p.CategoryId, null));
        }

        // PUT api/products/5  (chỉ Admin)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, ProductInput input)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();
            if (!await _context.Categories.AnyAsync(c => c.Id == input.CategoryId))
                return BadRequest(new { message = "Danh mục không tồn tại" });

            p.Name = input.Name; p.Description = input.Description; p.Price = input.Price;
            p.ImageUrl = input.ImageUrl; p.Size = input.Size; p.CategoryId = input.CategoryId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/products/5  (chỉ Admin)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();
            _context.Products.Remove(p);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
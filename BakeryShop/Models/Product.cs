namespace BakeryShop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;

        // Khoá ngoại (Foreign Key) trỏ tới Category
        public int CategoryId { get; set; }
        public Category? Category { get; set; }


    }
}

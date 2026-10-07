namespace BakeryShop.Models
{
    public class Category  // Category: Danh muc banh
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Vi du nhu banh sinh nhat
        public List<Product> Products { get; set; } = new();
    }
}

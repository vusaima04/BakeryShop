namespace BakeryShop.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }

        // Liên kết vớiProduct đang thêm vào giỏ 
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        // Liên kết tới User đang sở hữu giỏ hàng này
        public string UserId { get; set; } = string.Empty;
    }
}

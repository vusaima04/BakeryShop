namespace BakeryShop.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        public string ReceiverPhone { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public DateTime ReceiveDate { get; set; }
        public string? Note { get; set; }

        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Confirme

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 1 Order có nhiều OrderDetail (chi tiết từng sản phẩm)
        public List<OrderDetail> OrderDetails { get; set; } = new();

    }
}

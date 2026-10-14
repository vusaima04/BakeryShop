namespace BakeryShop.Models
{
    public static class OrderStatus
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All = { Pending, Confirmed, Delivered, Cancelled };

        // Chữ hiển thị cho người dùng
        public static string Label(string status) => status switch
        {
            Pending => "Chờ xác nhận",
            Confirmed => "Đã xác nhận",
            Delivered => "Đã giao",
            Cancelled => "Đã hủy",
            _ => status
        };

        // Màu nhãn (badge) của Bootstrap
        public static string BadgeClass(string status) => status switch
        {
            Pending => "bg-warning text-dark",
            Confirmed => "bg-info text-dark",
            Delivered => "bg-success",
            Cancelled => "bg-secondary",
            _ => "bg-light text-dark"
        };
    }
}
using System.ComponentModel.DataAnnotations;

namespace BakeryShop.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [StringLength(300, ErrorMessage = "Địa chỉ tối đa 300 ký tự")]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0")]
        [Display(Name = "Số điện thoại")]
        public string ReceiverPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày nhận hàng")]
        public DateTime? ReceiveDate { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }
    }
}
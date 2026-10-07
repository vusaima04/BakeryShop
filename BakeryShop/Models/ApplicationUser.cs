using Microsoft.AspNetCore.Identity;

namespace BakeryShop.Models
{

    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        // Sau này muốn thêm gì (địa chỉ mặc định, ngày sinh...) thì thêm ở đây
    }
}

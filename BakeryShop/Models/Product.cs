using System.ComponentModel.DataAnnotations;

namespace BakeryShop.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên bánh")]
        [StringLength(150, ErrorMessage = "Tên bánh tối đa 150 ký tự")]
        [Display(Name = "Tên bánh")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Range(1000, 100000000, ErrorMessage = "Giá phải từ 1.000 đến 100.000.000 đồng")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập link ảnh")]
        [StringLength(500, ErrorMessage = "Link ảnh tối đa 500 ký tự")]
        [Display(Name = "Link ảnh")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập kích thước")]
        [StringLength(50, ErrorMessage = "Kích thước tối đa 50 ký tự")]
        [Display(Name = "Kích thước")]
        public string Size { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
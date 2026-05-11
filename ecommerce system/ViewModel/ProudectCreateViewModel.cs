using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ecommerce_system.ViewModel
{
    public class ProudectCreateViewModel
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100,000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Display(Name = "Product Images")]
        public List<IFormFile>? Uploads { get; set; }

        [Display(Name = "Discount Percentage")]
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public decimal? DiscountPercent { get; set; }
    }
}

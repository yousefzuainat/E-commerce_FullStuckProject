using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

using System.Collections.Generic;
using ecommerce_system.Models;

namespace ecommerce_system.ViewModel
{
    public class ProudectEditViewModel
    {
        public int Id { get; set; }

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

        public string? ExistingImg { get; set; }
        
        public List<ProductImage>? ExistingImages { get; set; }

        [Display(Name = "Add Product Images")]
        public List<IFormFile>? Uploads { get; set; }

        // IDs of existing images the user wants to delete
        public List<int>? ImagesToDelete { get; set; }

        [Display(Name = "Discount Percentage")]
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public decimal? DiscountPercent { get; set; }
    }
}

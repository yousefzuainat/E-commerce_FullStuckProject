using System.ComponentModel.DataAnnotations;

namespace ecommerce_system.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        public string? Img { get; set; }

        public ICollection<Proudect>? Proudects { get; set; }
    }
}

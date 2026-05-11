using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_system.Models
{
    public class Discount
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Discount Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100 percent")]
        public decimal DiscountPercent { get; set; }

        public bool Active { get; set; } = true;

        [ForeignKey("Proudect")]
        public int ProudectId { get; set; }
        public Proudect? Proudect { get; set; }
    }
}

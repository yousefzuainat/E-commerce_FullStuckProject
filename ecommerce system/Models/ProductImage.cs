using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_system.Models
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        [ForeignKey("Proudect")]
        public int ProudectId { get; set; }
        public Proudect? Proudect { get; set; }
    }
}

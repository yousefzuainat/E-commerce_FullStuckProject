using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_system.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        [ForeignKey("Cart")]
        public int CartId { get; set; }
        public Cart? Cart { get; set; }

        [ForeignKey("Proudect")]
        public int ProudectId { get; set; }
        public Proudect? Proudect { get; set; }
    }
}

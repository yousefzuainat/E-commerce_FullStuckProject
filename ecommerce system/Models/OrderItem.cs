using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_system.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public order? Order { get; set; }

        [ForeignKey("Proudect")]
        public int ProudectId { get; set; }
        public Proudect? Proudect { get; set; }
        public object? CreatedAt { get; internal set; }
    }
}

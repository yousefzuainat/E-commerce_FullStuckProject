using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_system.Models
{
    public class order
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        [Required]
        [Range(0, 1000000)]
        public decimal tootal_amount { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public AppliactionUser? User { get; set; }

        public payment? Payment { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

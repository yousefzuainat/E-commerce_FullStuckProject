using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_system.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Comment is required")]
        [StringLength(1000)]
        public string comment { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("Proudect")]
        public int ProudectId { get; set; }
        public Proudect? Proudect { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public AppliactionUser? User { get; set; }
    }
}

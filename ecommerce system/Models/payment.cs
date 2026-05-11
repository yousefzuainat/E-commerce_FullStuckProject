using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_system.Models
{
    public class payment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, 1000000)]
        public int amount { get; set; }

        public DateTime transction_data { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public order? Order { get; set; }
    }
}

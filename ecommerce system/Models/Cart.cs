using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_system.Models
{
    public class Cart
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public AppliactionUser? User { get; set; }

        public ICollection<CartItem>? CartItems { get; set; }
    }
}

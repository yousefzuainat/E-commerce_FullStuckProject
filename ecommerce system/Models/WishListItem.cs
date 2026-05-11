using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_system.Models
{
    public class WishListItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("WishList")]
        public int WishListId { get; set; }
        public WishList? WishList { get; set; }

        [ForeignKey("Proudect")]
        public int ProudectId { get; set; }
        public Proudect? Proudect { get; set; }
    }
}

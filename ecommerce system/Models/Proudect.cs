using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_system.Models
{
    public class Proudect
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [Column("Descrption")]
        public string Description { get; set; }  


        public string? Img { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100,000")]
        public decimal Price { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; } 
        
        public Category? Category { get; set; }

        public ICollection<Review>? Reviews { get; set; }
        public ICollection<ProductImage>? Images { get; set; }
        public ICollection<Discount>? Discounts { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }
        public ICollection<WishListItem>? WishListItems { get; set; }
    }
}

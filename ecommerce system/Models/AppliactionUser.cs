using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ecommerce_system.Models
{
    public class AppliactionUser : IdentityUser
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        public string FullName { get; set; }

        [StringLength(100)]
        public string? Address { get; set; }

        public ICollection<order>? Orders { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public Cart? Cart { get; set; }
        public WishList? WishList { get; set; }
    }
}

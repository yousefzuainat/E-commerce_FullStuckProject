namespace ecommerce_system.Models
{
    public class Testimonials
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public AppliactionUser User { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public bool IsApproved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}

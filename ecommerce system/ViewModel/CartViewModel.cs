using System.Collections.Generic;

namespace ecommerce_system.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(i => i.LineTotal);
        public decimal Shipping => Subtotal >= 75 ? 0 : 9.99m;
        public decimal Total => Subtotal + Shipping;
        public int TotalQuantity => Items.Sum(i => i.Quantity);
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Img { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => Price * Quantity;
    }

    // Stored in session as JSON
    public class SessionCartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

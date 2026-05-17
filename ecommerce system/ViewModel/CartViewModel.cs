using System.Collections.Generic;

namespace ecommerce_system.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();

        public decimal Subtotal => Items.Sum(i => i.LineTotal);

        public decimal TotalSavings => Items.Sum(i => i.SavingsAmount);

        public decimal Shipping => 0m;
        public decimal Total => Subtotal;
        public int TotalQuantity => Items.Sum(i => i.Quantity);
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Img { get; set; } = "";
        public decimal Price { get; set; }

        public decimal DiscountPercent { get; set; } = 0;

        public int Quantity { get; set; }

        public decimal DiscountedPrice => DiscountPercent > 0
            ? Price * (1 - DiscountPercent / 100m)
            : Price;

        public decimal LineTotal => DiscountedPrice * Quantity;

        public decimal SavingsAmount => (Price - DiscountedPrice) * Quantity;

        public bool HasDiscount => DiscountPercent > 0;
    }

    public class SessionCartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

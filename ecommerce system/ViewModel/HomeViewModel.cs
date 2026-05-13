using System;
using System.Collections.Generic;

namespace ecommerce_system.ViewModels
{
    public class HomeViewModel
    {
        public HeroStats Stats { get; set; }
        public HeroFeature HeroDrop { get; set; }
        public List<CategoryCard> Categories { get; set; }
        public List<ProductCard> FeaturedProducts { get; set; }
        public FlashSale FlashSale { get; set; }
        public List<string> PartnerBrands { get; set; }
    }

    public class HeroStats
    {
        public int ProductCount { get; set; }
        public int SatisfactionRate { get; set; }
        public int BrandCount { get; set; }
        public string ProductCountFormatted => ProductCount >= 1000
            ? (ProductCount / 1000.0).ToString("0.0") + "K"
            : ProductCount.ToString();
    }

    public class HeroFeature
    {

        public string Name { get; set; }
        public decimal Price { get; set; }
        public string PriceFormatted => $"${Price:0}";
    }

    public class CategoryCard
    {
        public int CategoryId { get; set; }
        public string Img { get; set; }   // emoji mapped from Name
        public int ItemCount { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
    }

    public class ProductCard
    {

        public int ProductId { get; set; }
        public string Img { get; set; }
        public string BadgeClass { get; set; }   // "", "new", "sale"
        public string BadgeText { get; set; }    // "", "New", "−30%"
        public string Brand { get; set; }
        public string Name { get; set; }
        public int Rating { get; set; }
        public string ReviewCount { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
    }

    public class FlashSale
    {
        public string TitleLine1 { get; set; }
        public string TitleLine2 { get; set; }
        public string Subtitle { get; set; }
        public DateTime EndTime { get; set; }
    }
}
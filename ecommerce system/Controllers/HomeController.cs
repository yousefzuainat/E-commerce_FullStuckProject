using ecommerce_system.Data;
using ecommerce_system.Models;
using ecommerce_system.ViewModel;
using ecommerce_system.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ecommerce_system.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            /* ── 1. Stats (live from DB) ── */
            var totalProducts = await _context.proudects.CountAsync();

            // Satisfaction = average of ALL review ratings converted to %
            var satisfactionRate = 98;
            try
            {
                var avgRating = await _context.review.AverageAsync(r => (double?)r.Rating) ?? 4.9;
                satisfactionRate = (int)Math.Round((avgRating / 5.0) * 100);
            }
            catch { /* no reviews yet → keep 98 fallback */ }

            // Brands = distinct first words of product names
            var productNames = await _context.proudects.Select(p => p.Name).ToListAsync();
            var distinctBrands = productNames
                .Select(n => n?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
                .Where(b => !string.IsNullOrEmpty(b))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            /* ── 2. Hero drop = newest arrival ── */
            var newest = await _context.proudects
                .Include(p => p.Category)
                .Include(p => p.Discounts)
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            var heroDrop = newest != null
                ? MapHero(newest)
                : new HeroFeature { Name = "Sony WH-1000XM6", Price = 349 };

            /* ── 3. Categories with live counts ── */
            var categories = await _context.categories
                .Include(c => c.Proudects)               // your nav is spelled Proudects
                .Select(c => new CategoryCard
                {
                    Name = c.Name,
                    Slug = c.Name.ToLowerInvariant().Replace(" ", "-"),
                    ItemCount = c.Proudects.Count,
                    Icon = NameToEmoji(c.Name)
                })
                .ToListAsync();

            /* ── 4. Featured = newest 8 (no IsFeatured needed) ── */
            var featuredRaw = await _context.proudects
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Discounts)
                .Include(p => p.Reviews)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToListAsync();

            var featured = featuredRaw
                .Select((p, idx) => MapProduct(p, idx))
                .ToList();

            /* ── 5. Flash sale = product with best active discount ── */
            var saleProduct = await _context.proudects
                .Include(p => p.Category)
                .Include(p => p.Discounts)
                .Where(p => p.Discounts.Any(d => d.Active))
                .OrderByDescending(p => p.Discounts
                    .Where(d => d.Active)
                    .Max(d => d.DiscountPercent))
                .FirstOrDefaultAsync();

            var flashSale = saleProduct != null
                ? new FlashSale
                {
                    TitleLine1 = "FLASH SALE",
                    TitleLine2 = (saleProduct.Category?.Name ?? "SELECT GEAR").ToUpperInvariant(),
                    Subtitle = $"Up to {(int)saleProduct.Discounts.Where(d => d.Active).Max(d => d.DiscountPercent):0}% off {ExtractBrand(saleProduct.Name)} {saleProduct.Name}.",
                    EndTime = DateTime.Now.AddHours(8).AddMinutes(42).AddSeconds(17)
                }
                : new FlashSale
                {
                    TitleLine1 = "FLASH SALE",
                    TitleLine2 = "GAMING GEAR",
                    Subtitle = "Up to 40% off select gaming peripherals. Level up before the deal expires.",
                    EndTime = DateTime.Now.AddHours(8).AddMinutes(42).AddSeconds(17)
                };

            /* ── 6. Partner brands ── */
            var brands = productNames
                .Select(n => ExtractBrand(n))
                .Where(b => !string.IsNullOrEmpty(b))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(b => b)
                .Take(8)
                .ToList();

            /* ── Assemble ── */
            var vm = new HomeViewModel
            {
                Stats = new HeroStats
                {
                    ProductCount = totalProducts,
                    SatisfactionRate = satisfactionRate,
                    BrandCount = distinctBrands
                },
                HeroDrop = heroDrop,
                Categories = categories,
                FeaturedProducts = featured.Any() ? featured : GetFallbackProducts(),
                FlashSale = flashSale,
                PartnerBrands = brands.Any() ? brands : GetFallbackBrands()
            };

            return View(vm);
        }

        /* ═══════ Mapping helpers ═══════ */

        private static HeroFeature MapHero(Proudect p)
        {
            var bestDiscount = p.Discounts?.Where(d => d.Active).MaxBy(d => d.DiscountPercent);
            var price = p.Price;
            if (bestDiscount != null)
                price = p.Price * (1 - bestDiscount.DiscountPercent / 100m);

            return new HeroFeature { Name = p.Name, Price = price };
        }

        private static ProductCard MapProduct(Proudect p, int index)
        {
            var reviews = p.Reviews ?? new List<Review>();
            var reviewCount = reviews.Count;
            var avgRating = reviews.Any() ? (int)Math.Round(reviews.Average(r => r.Rating)) : 5;

            var bestDiscount = p.Discounts?.Where(d => d.Active).MaxBy(d => d.DiscountPercent);

            var currentPrice = p.Price;
            decimal? oldPrice = null;
            string badgeClass = "";
            string badgeText = "";

            if (bestDiscount != null)
            {
                oldPrice = p.Price;
                currentPrice = p.Price * (1 - bestDiscount.DiscountPercent / 100m);
                badgeClass = "sale";
                badgeText = $"-{(int)bestDiscount.DiscountPercent}%";
            }
            else if (index < 2)          // first 2 newest get "New"
            {
                badgeClass = "new";
                badgeText = "New";
            }

            return new ProductCard
            {
                Icon = NameToEmoji(p.Category?.Name),
                Brand = ExtractBrand(p.Name),
                Name = p.Name,
                Rating = avgRating,
                ReviewCount = FormatCount(reviewCount),
                Price = currentPrice,
                OldPrice = oldPrice,
                BadgeClass = badgeClass,
                BadgeText = badgeText
            };
        }

        private static string ExtractBrand(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            return fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
        }

        private static string NameToEmoji(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName)) return "📦";
            var n = categoryName.ToLowerInvariant();
            if (n.Contains("audio") || n.Contains("headphone")) return "🎧";
            if (n.Contains("charg") || n.Contains("power")) return "⚡";
            if (n.Contains("game")) return "🎮";
            if (n.Contains("work") || n.Contains("computer") || n.Contains("pc")) return "💻 ⌨️";
            if (n.Contains("mobile") || n.Contains("phone")) return "📱";
            if (n.Contains("camera") || n.Contains("photo")) return "📷";
            return "📦";
        }

        private static string FormatCount(int count) => count switch
        {
            >= 1000 => (count / 1000.0).ToString("0.0") + "K",
            _ => count.ToString()
        };

        private static List<string> GetFallbackBrands() => new()
        {
            "Apple", "Sony", "Logitech", "Razer", "Samsung", "Anker", "Bose", "Keychron"
        };

        private static List<ProductCard> GetFallbackProducts() => new()
        {
            new() { Icon="🎧", BadgeClass="new",  BadgeText="New",    Brand="Sony",     Name="WH-1000XM6 Wireless Noise Cancelling",     Rating=5, ReviewCount="412",  Price=349 },
            new() { Icon="⌨️", BadgeClass="hot",  BadgeText="Hot",    Brand="Keychron", Name="Q3 Max QMK Wireless Mechanical Keyboard",  Rating=4, ReviewCount="238",  Price=199 },
            new() { Icon="🖱️", BadgeClass="sale", BadgeText="−30%",   Brand="Logitech", Name="MX Master 3S Performance Wireless Mouse",  Rating=5, ReviewCount="1.2K", Price=69,  OldPrice=99 },
            new() { Icon="⚡", BadgeClass="new",  BadgeText="New",    Brand="Anker",    Name="Prime 240W GaN Desktop Charger",           Rating=5, ReviewCount="89",   Price=89 },
            new() { Icon="🎮", BadgeClass="",     BadgeText="",       Brand="Razer",    Name="DeathAdder V3 HyperSpeed Gaming Mouse",    Rating=5, ReviewCount="674",  Price=99 },
            new() { Icon="🖥️", BadgeClass="sale", BadgeText="−20%",   Brand="BenQ",     Name="ScreenBar Halo Monitor Light + Backlight", Rating=4, ReviewCount="307",  Price=159, OldPrice=199 },
            new() { Icon="📱", BadgeClass="",     BadgeText="",       Brand="Apple",    Name="MagSafe Duo Charger 15W",                Rating=5, ReviewCount="2.1K", Price=129 },
            new() { Icon="🎙️", BadgeClass="hot",  BadgeText="Hot",    Brand="Blue",     Name="Yeti X Professional USB Microphone",       Rating=5, ReviewCount="881",  Price=149 }
        };
    }

}
using ecommerce_system.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ecommerce_system.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppliactionUser>
    {
        // 1. CONSTRUCTOR (Keep this to fix the parameterless constructor error)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 2. DB SETS
        public DbSet<Proudect> proudects { get; set; }
        public DbSet<Category> categories { get; set; }
        public DbSet<order> orders { get; set; }
        public DbSet<WishList> wishList { get; set; }
        public DbSet<payment> payment { get; set; }
        public DbSet<Review> review { get; set; }
        public DbSet<Cart> cart { get; set; }
        public DbSet<Discount> discounts { get; set; }
        public DbSet<ProductImage> productImages { get; set; }
        public DbSet<OrderItem> orderItems { get; set; }
        public DbSet<CartItem> cartItems { get; set; }
        public DbSet<WishListItem> wishListItems { get; set; }
        public DbSet<Testimonials> Testimonials { get; set; }

        // 3. MODEL CONFIGURATION
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // This is MANDATORY. It configures the Identity tables.
            base.OnModelCreating(modelBuilder);

            // Fix the Decimal precision warnings
            modelBuilder.Entity<Proudect>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");
            // ── THE ENUM STRING FIX ──
            // Forces EF Core to save/load OrderStatus as a String matching your DB database text values
            modelBuilder.Entity<order>()
                .Property(o => o.Status)
                .HasConversion<string>(); 
            
            modelBuilder.Entity<Discount>()
                .Property(d => d.DiscountPercent)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(o => o.UnitPrice)
                .HasColumnType("decimal(18,2)");

            // Resolve the Discriminator conflict for Identity
            modelBuilder.Entity<IdentityUser>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue("IdentityUser");

            modelBuilder.Entity<AppliactionUser>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue("AppliactionUser");
            modelBuilder.Entity<order>().Ignore(o => o.CreatedAt);
            modelBuilder.Entity<OrderItem>().Ignore(oi => oi.CreatedAt);
            // Fix for SQL Server versions (DateTime compatibility)
            var dateTimeOffsetConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset, DateTime>(
                v => v.DateTime,
                v => new DateTimeOffset(v));

            var nullableDateTimeOffsetConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset?, DateTime?>(
                v => v.HasValue ? v.Value.DateTime : default(DateTime?),
                v => v.HasValue ? new DateTimeOffset(v.Value) : default(DateTimeOffset?));
            modelBuilder.Entity<Proudect>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("datetime");
                    }
                    else if (property.ClrType == typeof(DateTimeOffset))
                    {
                        property.SetValueConverter(dateTimeOffsetConverter);
                        property.SetColumnType("datetime");
                    }
                    else if (property.ClrType == typeof(DateTimeOffset?))
                    {
                        property.SetValueConverter(nullableDateTimeOffsetConverter);
                        property.SetColumnType("datetime");
                    }
                }
            }
        }
    }
}
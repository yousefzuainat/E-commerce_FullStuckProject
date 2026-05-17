using ecommerce_system.Data;
using ecommerce_system.Models;
using ecommerce_system.ViewModels;
using ecommerce_system.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_system.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppliactionUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<AppliactionUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ─────────────────────────────────────────────
        // GET /Cart  — show cart page
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var vm = await BuildCartViewModelAsync();
            return View(vm);
        }

        // ─────────────────────────────────────────────
        // POST /Cart/Add  — add item (called via AJAX)
        // ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult Add(int productId, int quantity = 1)
        {
            SessionCartService.AddItem(HttpContext, productId, quantity);
            var total = SessionCartService.GetTotalQuantity(HttpContext);
            return Json(new { success = true, cartCount = total });
        }

        // ─────────────────────────────────────────────
        // POST /Cart/Update  — change quantity via AJAX
        // ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Update(int productId, int quantity)
        {
            SessionCartService.UpdateItem(HttpContext, productId, quantity);
            var vm = await BuildCartViewModelAsync();
            var item = vm.Items.FirstOrDefault(i => i.ProductId == productId);
            return Json(new
            {
                success = true,
                cartCount = vm.TotalQuantity,
                subtotal = vm.Subtotal.ToString("F2"),
                savings = vm.TotalSavings.ToString("F2"),
                shipping = vm.Shipping.ToString("F2"),
                total = vm.Total.ToString("F2"),
                lineTotal = item?.LineTotal.ToString("F2") ?? "0.00",
                discountPercent = item?.DiscountPercent ?? 0,
                discountedPrice = item?.DiscountedPrice.ToString("F2") ?? "0.00",
                savingsAmount = item?.SavingsAmount.ToString("F2") ?? "0.00",
                hasDiscount = item?.HasDiscount ?? false
            });
        }

        // ─────────────────────────────────────────────
        // POST /Cart/Remove  — remove item via AJAX
        // ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            SessionCartService.RemoveItem(HttpContext, productId);
            var vm = await BuildCartViewModelAsync();
            return Json(new
            {
                success = true,
                cartCount = vm.TotalQuantity,
                subtotal = vm.Subtotal.ToString("F2"),
                savings = vm.TotalSavings.ToString("F2"),
                shipping = vm.Shipping.ToString("F2"),
                total = vm.Total.ToString("F2"),
                isEmpty = vm.Items.Count == 0
            });
        }

        // ─────────────────────────────────────────────
        // GET /Cart/Count  — badge refresh via AJAX
        // ─────────────────────────────────────────────
        [HttpGet]
        public IActionResult Count()
        {
            var count = SessionCartService.GetTotalQuantity(HttpContext);
            return Json(new { cartCount = count });
        }

        // ─────────────────────────────────────────────
        // POST /Cart/Checkout — requires login, saves to DB
        // ─────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        public IActionResult Checkout()
        {
            var sessionItems = SessionCartService.GetCart(HttpContext);

            if (!sessionItems.Any())
                return RedirectToAction(nameof(Index));

            return RedirectToAction("Index", "Checkout");
        }

        // ─────────────────────────────────────────────
        // Helper: hydrate session IDs → full view model
        // ─────────────────────────────────────────────
        private async Task<CartViewModel> BuildCartViewModelAsync()
        {
            var sessionItems = SessionCartService.GetCart(HttpContext);
            var vm = new CartViewModel();

            if (!sessionItems.Any())
                return vm;

            var ids = sessionItems.Select(i => i.ProductId).ToList();

            // Load products WITH their active discounts
            var products = await _context.proudects
                .Include(p => p.Discounts)
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            foreach (var si in sessionItems)
            {
                var p = products.FirstOrDefault(x => x.Id == si.ProductId);
                if (p == null) continue;

                // Pick the best active discount from the Discounts table
                var bestDiscount = p.Discounts?
                    .Where(d => d.Active)
                    .OrderByDescending(d => d.DiscountPercent)
                    .FirstOrDefault();

                vm.Items.Add(new CartItemViewModel
                {
                    ProductId      = p.Id,
                    Name           = p.Name,
                    Brand          = p.Name.Split(' ').FirstOrDefault() ?? "",
                    Img            = p.Img ?? "",
                    Price          = p.Price,
                    DiscountPercent = bestDiscount?.DiscountPercent ?? 0,
                    Quantity       = si.Quantity
                });
            }

            return vm;
        }
    }
}

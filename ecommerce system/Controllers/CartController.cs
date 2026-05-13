using ecommerce_system.Data;
using ecommerce_system.Models;
using ecommerce_system.Services;
using ecommerce_system.ViewModels;
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
            SessionCartService.AddItem(HttpContext.Session, productId, quantity);
            var total = SessionCartService.GetTotalQuantity(HttpContext.Session);
            return Json(new { success = true, cartCount = total });
        }

        // ─────────────────────────────────────────────
        // POST /Cart/Update  — change quantity via AJAX
        // ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Update(int productId, int quantity)
        {
            SessionCartService.UpdateItem(HttpContext.Session, productId, quantity);
            var vm = await BuildCartViewModelAsync();
            return Json(new
            {
                success = true,
                cartCount = vm.TotalQuantity,
                subtotal = vm.Subtotal.ToString("F2"),
                shipping = vm.Shipping.ToString("F2"),
                total = vm.Total.ToString("F2"),
                lineTotal = vm.Items.FirstOrDefault(i => i.ProductId == productId)?.LineTotal.ToString("F2") ?? "0.00"
            });
        }

        // ─────────────────────────────────────────────
        // POST /Cart/Remove  — remove item via AJAX
        // ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            SessionCartService.RemoveItem(HttpContext.Session, productId);
            var vm = await BuildCartViewModelAsync();
            return Json(new
            {
                success = true,
                cartCount = vm.TotalQuantity,
                subtotal = vm.Subtotal.ToString("F2"),
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
            var count = SessionCartService.GetTotalQuantity(HttpContext.Session);
            return Json(new { cartCount = count });
        }

        // ─────────────────────────────────────────────
        // POST /Cart/Checkout — requires login, saves to DB
        // ─────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var sessionItems = SessionCartService.GetCart(HttpContext.Session);

            if (!sessionItems.Any())
                return RedirectToAction(nameof(Index));

            // Find or create the user's DB cart
            var dbCart = await _context.cart
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (dbCart == null)
            {
                dbCart = new Cart { UserId = userId, CartItems = new List<CartItem>() };
                _context.cart.Add(dbCart);
                await _context.SaveChangesAsync();
            }

            // Merge session items into DB cart
            foreach (var sessionItem in sessionItems)
            {
                var existing = dbCart.CartItems!
                    .FirstOrDefault(ci => ci.ProudectId == sessionItem.ProductId);

                if (existing != null)
                    existing.Quantity += sessionItem.Quantity;
                else
                    dbCart.CartItems!.Add(new CartItem
                    {
                        CartId = dbCart.Id,
                        ProudectId = sessionItem.ProductId,
                        Quantity = sessionItem.Quantity
                    });
            }

            await _context.SaveChangesAsync();

            // Clear the session cart
            SessionCartService.ClearCart(HttpContext.Session);

            // Redirect to checkout page (create later) or order summary
            return RedirectToAction("Index", "Checkout");
        }

        // ─────────────────────────────────────────────
        // Helper: hydrate session IDs → full view model
        // ─────────────────────────────────────────────
        private async Task<CartViewModel> BuildCartViewModelAsync()
        {
            var sessionItems = SessionCartService.GetCart(HttpContext.Session);
            var vm = new CartViewModel();

            if (!sessionItems.Any())
                return vm;

            var ids = sessionItems.Select(i => i.ProductId).ToList();
            var products = await _context.proudects
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            foreach (var si in sessionItems)
            {
                var p = products.FirstOrDefault(x => x.Id == si.ProductId);
                if (p == null) continue;

                vm.Items.Add(new CartItemViewModel
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Brand = p.Name.Split(' ').FirstOrDefault() ?? "",
                    Img = p.Img ?? "",
                    Price = p.Price,
                    Quantity = si.Quantity
                });
            }

            return vm;
        }
    }
}

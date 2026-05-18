using ecommerce_system.Data;
using ecommerce_system.Models;
using ecommerce_system.ViewModels;
using ecommerce_system.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ecommerce_system.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppliactionUser> _userManager;
        private readonly SignInManager<AppliactionUser> _signInManager;

        public CartController(ApplicationDbContext context,
                              UserManager<AppliactionUser> userManager,
                              SignInManager<AppliactionUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET /Cart — Show main cart view page
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Index()
        {
            await EnsureCartSyncedAsync();
            var vm = await BuildCartViewModelAsync();
            return View(vm);
        }

        // POST /Cart/Add — Add item (called via product detail card AJAX)
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            await EnsureCartSyncedAsync();
            SessionCartService.AddItem(HttpContext, productId, quantity);
            await SyncSessionToDatabaseAsync();

            var total = SessionCartService.GetTotalQuantity(HttpContext);

            return Json(new { success = true, cartCount = total });
        }

        // POST /Cart/Update — Step counter alteration adjustments via AJAX
        [HttpPost]
        public async Task<IActionResult> Update(int productId, int quantity)
        {
            await EnsureCartSyncedAsync();
            SessionCartService.UpdateItem(HttpContext, productId, quantity);
            await SyncSessionToDatabaseAsync();

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

        // POST /Cart/Remove — Remove item line row via AJAX
        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            await EnsureCartSyncedAsync();
            SessionCartService.RemoveItem(HttpContext, productId);
            await SyncSessionToDatabaseAsync();

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

        // GET /Cart/Count — Header navbar layout numeric badge updater
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Count()
        {
            await EnsureCartSyncedAsync();
            var count = SessionCartService.GetTotalQuantity(HttpContext);
            return Json(new { cartCount = count });
        }

        // POST /Cart/Checkout — Secure order staging transfer
        [HttpPost]
        [Authorize]
        public IActionResult Checkout()
        {
            var sessionItems = SessionCartService.GetCart(HttpContext);

            if (!sessionItems.Any())
                return RedirectToAction(nameof(Index));

            return RedirectToAction("Index", "Checkout");
        }

        // POST /Cart/Logout — Secure user disconnect sequence
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Clear custom service session storage variables
            SessionCartService.ClearCart(HttpContext);

            // Wipe default primitive session records
            HttpContext.Session.Clear();

            // Perform underlying cookie authentication drops
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        // ── STATE-ISOLATED SYNCHRONIZATION ENGINE ──
        private async Task EnsureCartSyncedAsync()
        {
            string sessionUserId = HttpContext.Session.GetString("CartUserId");
            string currentUserId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;

            // STATE 1: DETECT UNEXPECTED LOGOUT OR COOKIE EXSPIRATION
            if (!string.IsNullOrEmpty(sessionUserId) && string.IsNullOrEmpty(currentUserId))
            {
                SessionCartService.ClearCart(HttpContext);
                HttpContext.Session.Clear();
                return;
            }

            // STATE 2: DETECT PROFILE ACCOUNT SWITCHING
            if (!string.IsNullOrEmpty(sessionUserId) && !string.IsNullOrEmpty(currentUserId) && sessionUserId != currentUserId)
            {
                SessionCartService.ClearCart(HttpContext);
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("CartUserId", currentUserId);
                HttpContext.Session.Remove("CartSynced");
                sessionUserId = currentUserId;
            }

            // STATE 3: DETECT VALID ACTIVE USER ACCESS & LOGGED-IN AUTO-MERGE
            if (currentUserId != null)
            {
                if (HttpContext.Session.GetString("CartUserId") != currentUserId)
                {
                    HttpContext.Session.SetString("CartUserId", currentUserId);
                }

                if (HttpContext.Session.GetString("CartSynced") != "true")
                {
                    var dbCart = await _context.cart
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.UserId == currentUserId);

                    var sessionItems = SessionCartService.GetCart(HttpContext);

                    if (dbCart != null && dbCart.CartItems != null)
                    {
                        foreach (var dbItem in dbCart.CartItems)
                        {
                            var existingSessionItem = sessionItems.FirstOrDefault(si => si.ProductId == dbItem.ProudectId);

                            if (existingSessionItem == null)
                            {
                                // Bring remote DB tracking keys straight into context memory
                                SessionCartService.AddItem(HttpContext, dbItem.ProudectId, dbItem.Quantity);
                            }
                            else
                            {
                                // FIX: Take the highest isolated value to prevent items from doubling up on login
                                int targetQty = Math.Max(existingSessionItem.Quantity, dbItem.Quantity);
                                SessionCartService.UpdateItem(HttpContext, dbItem.ProudectId, targetQty);
                            }
                        }
                    }

                    // Seal sync validation layer access markers
                    HttpContext.Session.SetString("CartSynced", "true");

                    // Immediately write our clean state values back down to database persistence
                    await SyncSessionToDatabaseAsync();
                }
            }
        }

        // ── EF TRACKING-SAFE PERSISTENCE ENGINE ──
        private async Task SyncSessionToDatabaseAsync()
        {
            if (User.Identity?.IsAuthenticated != true) return;

            var userId = _userManager.GetUserId(User);
            if (userId == null) return;

            var sessionItems = SessionCartService.GetCart(HttpContext);

            var dbCart = await _context.cart
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (sessionItems.Any())
            {
                if (dbCart == null)
                {
                    dbCart = new Cart
                    {
                        UserId = userId,
                        CartItems = new List<CartItem>()
                    };
                    _context.cart.Add(dbCart);
                }
                else
                {
                    // FIX: Drop sub-children references safely instead of removing tracked root context trees
                    if (dbCart.CartItems != null)
                    {
                        _context.cartItems.RemoveRange(dbCart.CartItems);
                        dbCart.CartItems.Clear();
                    }
                    else
                    {
                        dbCart.CartItems = new List<CartItem>();
                    }
                }

                // Append runtime memory objects straight into the tracked collection
                foreach (var si in sessionItems)
                {
                    dbCart.CartItems.Add(new CartItem
                    {
                        ProudectId = si.ProductId,
                        Quantity = si.Quantity
                    });
                }
            }
            else
            {
                // If everything gets cleared, we can clean up the remaining empty tables
                if (dbCart != null && dbCart.CartItems != null)
                {
                    _context.cartItems.RemoveRange(dbCart.CartItems);
                    _context.cart.Remove(dbCart);
                }
            }

            await _context.SaveChangesAsync();
        }

        // ── FULL STRUCT COMPILATION ENGINE ──
        private async Task<CartViewModel> BuildCartViewModelAsync()
        {
            var sessionItems = SessionCartService.GetCart(HttpContext);
            var vm = new CartViewModel();

            if (!sessionItems.Any())
                return vm;

            var ids = sessionItems.Select(i => i.ProductId).ToList();

            var products = await _context.proudects
                .Include(p => p.Discounts)
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            foreach (var si in sessionItems)
            {
                var p = products.FirstOrDefault(x => x.Id == si.ProductId);
                if (p == null) continue;

                var bestDiscount = p.Discounts?
                    .Where(d => d.Active)
                    .OrderByDescending(d => d.DiscountPercent)
                    .FirstOrDefault();

                vm.Items.Add(new CartItemViewModel
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Brand = p.Name.Split(' ').FirstOrDefault() ?? "",
                    Img = p.Img ?? "",
                    Price = p.Price,
                    DiscountPercent = bestDiscount?.DiscountPercent ?? 0,
                    Quantity = si.Quantity
                });
            }

            return vm;
        }
    }
}
using ecommerce_system.Data;
using ecommerce_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace ecommerce_system.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppliactionUser> _userManager;

        public CheckoutController(ApplicationDbContext context, UserManager<AppliactionUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _context.cart
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Proudect)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var cart = await _context.cart
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Proudect)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + "/Checkout/Success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "/Checkout/Cancel",
                CustomerEmail = user?.Email
            };

            decimal totalAmount = 0;

            foreach (var item in cart.CartItems)
            {
                if (item.Proudect != null)
                {
                    totalAmount += item.Quantity * item.Proudect.Price;
                    var sessionListItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Proudect.Price * 100), // Stripe expects amounts in cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Proudect.Name.ToString()
                            }
                        },
                        Quantity = item.Quantity
                    };
                    options.LineItems.Add(sessionListItem);
                }
            }

            var service = new SessionService();
            Session session = service.Create(options);

            TempData["SessionId"] = session.Id;

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public async Task<IActionResult> Success(string session_id)
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _context.cart
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Proudect)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            decimal totalAmount = cart.CartItems.Sum(ci => ci.Quantity * (ci.Proudect?.Price ?? 0));

            var order = new order
            {
                UserId = userId,
                tootal_amount = totalAmount,
                Status = "Paid",
                Name = User.Identity?.Name ?? "Customer",
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in cart.CartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProudectId = item.ProudectId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Proudect?.Price ?? 0
                });
            }

            _context.orders.Add(order);

            // Create Payment record
            var payment = new payment
            {
                amount = (int)totalAmount,
                Status = "Success",
                transction_data = DateTime.Now,
                Order = order
            };

            _context.payment.Add(payment);

            _context.cartItems.RemoveRange(cart.CartItems);
            
            await _context.SaveChangesAsync();

            TempData["success"] = "Payment successful! Your order has been placed.";
            return RedirectToAction("Index", "UserOrders");
        }

        public IActionResult Cancel()
        {
            TempData["error"] = "Payment was cancelled.";
            return RedirectToAction("Index");
        }
    }
}

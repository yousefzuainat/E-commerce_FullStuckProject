using ecommerce_system.Data;
using ecommerce_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce_system.Services;
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
            var sessionItems = SessionCartService.GetCart(HttpContext);

            if (!sessionItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem>()
            };

            foreach (var item in sessionItems)
            {
                var product = await _context.proudects
                    .Include(p => p.Discounts)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);
                    
                if (product != null)
                {
                    var bestDiscount = product.Discounts?
                        .Where(d => d.Active)
                        .OrderByDescending(d => d.DiscountPercent)
                        .FirstOrDefault();

                    if (bestDiscount != null)
                    {
                        product.Price = product.Price - (product.Price * bestDiscount.DiscountPercent / 100m);
                    }

                    cart.CartItems.Add(new CartItem
                    {
                        ProudectId = product.Id,
                        Quantity = item.Quantity,
                        Proudect = product
                    });
                }
            }

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment()
        {
            var userId = _userManager.GetUserId(User);
            var sessionItems = SessionCartService.GetCart(HttpContext);

            if (!sessionItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var domain = $"{Request.Scheme}://{Request.Host}";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + "/Checkout/Success",
                CancelUrl = domain + "/Checkout/Index",
            };

            decimal totalAmount = 0;

            foreach (var item in sessionItems)
            {
                var product = await _context.proudects
                    .Include(p => p.Discounts)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product != null)
                {
                    var price = product.Price;
                    var bestDiscount = product.Discounts?
                        .Where(d => d.Active)
                        .OrderByDescending(d => d.DiscountPercent)
                        .FirstOrDefault();

                    if (bestDiscount != null)
                    {
                        price = price - (price * bestDiscount.DiscountPercent / 100m);
                    }

                    totalAmount += item.Quantity * price;

                    options.LineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(price * 100), // Stripe expects amount in cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = product.Name,
                            },
                        },
                        Quantity = item.Quantity,
                    });
                }
            }
            


            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Append("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public async Task<IActionResult> Success()
        {
            var userId = _userManager.GetUserId(User);
            var sessionItems = SessionCartService.GetCart(HttpContext);

            if (!sessionItems.Any())
            {
                // Safety check: ensure this controller name matches your client-side orders handler
                return RedirectToAction("Index", "UserOrers");
            }

            decimal subtotal = 0;
            var order = new order
            {
                UserId = userId,
                Status = OrderStatus.Paid, // FIX: Direct assignment of the enum (Removed .ToString())
                Name = User.Identity?.Name ?? "Customer"
            };

            var orderItemsList = new List<OrderItem>();

            foreach (var item in sessionItems)
            {
                var product = await _context.proudects
                    .Include(p => p.Discounts)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product != null)
                {
                    var price = product.Price;
                    var bestDiscount = product.Discounts?
                        .Where(d => d.Active)
                        .OrderByDescending(d => d.DiscountPercent)
                        .FirstOrDefault();

                    if (bestDiscount != null)
                    {
                        price = price - (price * bestDiscount.DiscountPercent / 100m);
                    }

                    subtotal += item.Quantity * price;

                    orderItemsList.Add(new OrderItem
                    {
                        ProudectId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = price
                    });
                }
            }

            decimal totalAmount = subtotal;
            order.tootal_amount = totalAmount;

            // Save order header first to generate the Order ID
            _context.orders.Add(order);
            await _context.SaveChangesAsync();

            // Batch add items linked to the newly generated Order ID
            foreach (var orderItem in orderItemsList)
            {
                orderItem.OrderId = order.Id;
                _context.Add(orderItem);
            }
            await _context.SaveChangesAsync(); // Saved out of loop for higher database performance

            var payment = new payment
            {
                amount = (int)totalAmount,
                Status = "Success",
                transction_data = DateTime.Now,
                Order = order
            };

            _context.payment.Add(payment);
            await _context.SaveChangesAsync();

            // Flush out temporary cache items
            SessionCartService.ClearCart(HttpContext);

            TempData["success"] = "Payment successful! Your order has been placed via Stripe.";

            // CRITICAL ROUTING CHECK: If your customer-facing history controller is 
            // named OrdersController, change "UserOrders" to "Orders" to avoid a 404!
            return RedirectToAction("Index", "UserOrders");
        }
    }
}

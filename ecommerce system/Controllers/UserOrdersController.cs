using ecommerce_system.Data;
using ecommerce_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ecommerce_system.Controllers
{
    [Authorize]
    public class UserOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppliactionUser> _userManager;

        public UserOrdersController(ApplicationDbContext context, UserManager<AppliactionUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            
            var orders = await _context.orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Proudect)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Id) 
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var order = await _context.orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Proudect)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (order == null) return NotFound();

            return View(order);
        }

        public async Task<IActionResult> Invoice(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var order = await _context.orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Proudect)
                .Include(o => o.Payment)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (order == null) return NotFound();

            return View(order);
        }
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.orders
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Proudect) // Get product info (Img, Name)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}

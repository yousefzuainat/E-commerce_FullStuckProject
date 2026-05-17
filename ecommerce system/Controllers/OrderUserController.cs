using ecommerce_system.Data;
using ecommerce_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_system.Controllers
{
    [Area("Admin")]
    public class OrderUserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderUserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? userId)
        {
            var query = _context.orders
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(o => o.UserId == userId);
            }

            ViewBag.UserId = new SelectList(
                await _context.Users.ToListAsync(),
                "Id",
                "UserName",
                userId
            );

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Proudect)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.orders.FindAsync(id);

            if (order == null) return NotFound();

            order.Status = status;

            await _context.SaveChangesAsync();

            TempData["success"] = "Order status updated successfully!";

            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.orders.FindAsync(id);

            if (order == null) return NotFound();

            ViewData["UserId"] = new SelectList(
                _context.Users,
                "Id",
                "UserName",
                order.UserId
            );

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Name,tootal_amount,Status,UserId")] order order)
        {
            if (id != order.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);

                    await _context.SaveChangesAsync();

                    TempData["info"] = "Order updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.orders.Any(e => e.Id == order.Id))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(
                _context.Users,
                "Id",
                "UserName",
                order.UserId
            );

            return View(order);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.orders.FindAsync(id);

            if (order != null)
            {
                _context.orders.Remove(order);

                await _context.SaveChangesAsync();

                TempData["success"] = "Order deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
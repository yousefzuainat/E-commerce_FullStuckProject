using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ecommerce_system.Data;
using ecommerce_system.Models;
using Microsoft.AspNetCore.Authorization;

namespace ecommerce_system.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/OrdersAdmin
        public async Task<IActionResult> Index(string userId)
        {
            var applicationDbContext = _context.orders.Include(o => o.User).AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                applicationDbContext = applicationDbContext.Where(o => o.UserId == userId);
            }

            ViewBag.UserId = new SelectList(_context.Users, "Id", "FullName", userId);
            
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.orders
                .Include(o => o.User)
                .Include(o => o.OrderItems) 
                .ThenInclude(oi => oi.Proudect)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Direct assignment works cleanly now that they match types
            order.Status = status;

            await _context.SaveChangesAsync();
            TempData["success"] = $"Order #{order.Id} status updated to {order.Status}.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            
            ViewBag.UserId = new SelectList(_context.Users, "Id", "FullName", order.UserId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,tootal_amount,Status,UserId")] order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["success"] = $"Order #{order.Id} updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.UserId = new SelectList(_context.Users, "Id", "FullName", order.UserId);
            return View(order);
        }

        private bool OrderExists(int id)
        {
            return _context.orders.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.orders.FindAsync(id);
            if (order != null)
            {
                _context.orders.Remove(order);
            }
            
            await _context.SaveChangesAsync();
            TempData["success"] = $"Order #{id} deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}

using ecommerce_system.Data;
using ecommerce_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_system.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TestimonialsAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestimonialsAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        /* ── Index: all testimonials ── */
        public async Task<IActionResult> Index()
        {
            var all = await _context.Testimonials
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(all);
        }

        /* ── Approve ── */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var t = await _context.Testimonials.FindAsync(id);
            if (t == null) return NotFound();

            t.IsApproved = true;
            await _context.SaveChangesAsync();

            TempData["success"] = "Testimonial approved and is now live on the homepage.";
            return RedirectToAction(nameof(Index));
        }

        /* ── Reject / Delete ── */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var t = await _context.Testimonials.FindAsync(id);
            if (t == null) return NotFound();

            _context.Testimonials.Remove(t);
            await _context.SaveChangesAsync();

            TempData["success"] = "Testimonial removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}
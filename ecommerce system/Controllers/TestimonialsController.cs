using ecommerce_system.Data;
using ecommerce_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_system.Controllers
{
    public class TestimonialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppliactionUser> _userManager;

        public TestimonialsController(ApplicationDbContext context,
                                      UserManager<AppliactionUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /* ═══════════════════════════════════════════════════════════════
           WRITE — GET
           Passes the existing testimonial (or null) into the view.
           The view handles both "write" and "already submitted" states.
        ═══════════════════════════════════════════════════════════════ */
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Write()
        {
            var userId = _userManager.GetUserId(User);
            var existing = await _context.Testimonials
                               .FirstOrDefaultAsync(t => t.UserId == userId);

            return View(existing);   // null → show write form; not-null → show status + edit/delete
        }

        /* ═══════════════════════════════════════════════════════════════
           WRITE — POST (first-time submission)
        ═══════════════════════════════════════════════════════════════ */
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Write(string content, int rating)
        {
            var userId = _userManager.GetUserId(User);
            var existing = await _context.Testimonials
                               .FirstOrDefaultAsync(t => t.UserId == userId);

            if (existing != null)
            {
                TempData["info"] = "You already have a testimonial. You can edit it below.";
                return RedirectToAction(nameof(Write));
            }

            if (string.IsNullOrWhiteSpace(content) || content.Trim().Length < 20)
            {
                TempData["error"] = "Please write at least 20 characters.";
                return View((Testimonials?)null);
            }

            if (rating < 1 || rating > 5)
            {
                TempData["error"] = "Please select a star rating.";
                return View((Testimonials?)null);
            }

            _context.Testimonials.Add(new Testimonials
            {
                UserId = userId!,
                Content = content.Trim(),
                Rating = rating,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["success"] = "Thanks! Your testimonial has been submitted and is pending admin approval.";
            return RedirectToAction(nameof(Write));
        }

        /* ═══════════════════════════════════════════════════════════════
           EDIT — POST (update content + rating, reset to pending)
        ═══════════════════════════════════════════════════════════════ */
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content, int rating)
        {
            var userId = _userManager.GetUserId(User);
            var t = await _context.Testimonials.FindAsync(id);

            if (t == null) return NotFound();
            if (t.UserId != userId) return Forbid();

            if (string.IsNullOrWhiteSpace(content) || content.Trim().Length < 20)
            {
                TempData["error"] = "Please write at least 20 characters.";
                return RedirectToAction(nameof(Write));
            }

            if (rating < 1 || rating > 5)
            {
                TempData["error"] = "Please select a valid star rating.";
                return RedirectToAction(nameof(Write));
            }

            t.Content = content.Trim();
            t.Rating = rating;
            t.IsApproved = false;   // edited → goes back to pending
            await _context.SaveChangesAsync();

            TempData["success"] = "Your testimonial has been updated and is pending re-approval.";
            return RedirectToAction(nameof(Write));
        }

        /* ═══════════════════════════════════════════════════════════════
           DELETE — POST (owner deletes; allows writing a new one)
        ═══════════════════════════════════════════════════════════════ */
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var t = await _context.Testimonials.FindAsync(id);

            if (t == null) return NotFound();
            if (t.UserId != userId) return Forbid();

            _context.Testimonials.Remove(t);
            await _context.SaveChangesAsync();

            TempData["success"] = "Your testimonial has been deleted. You can write a new one anytime.";
            return RedirectToAction(nameof(Write));
        }

        /* ═══════════════════════════════════════════════════════════════
           ADMIN — Approve / Reject
        ═══════════════════════════════════════════════════════════════ */
        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var t = await _context.Testimonials.FindAsync(id);
            if (t == null) return NotFound();
            t.IsApproved = true;
            await _context.SaveChangesAsync();
            TempData["success"] = "Testimonial approved and is now live.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var t = await _context.Testimonials.FindAsync(id);
            if (t == null) return NotFound();
            _context.Testimonials.Remove(t);
            await _context.SaveChangesAsync();
            TempData["success"] = "Testimonial removed.";
            return RedirectToAction("Index", "Home");
        }
    }
}

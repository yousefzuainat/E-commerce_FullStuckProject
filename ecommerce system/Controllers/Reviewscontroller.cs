using ecommerce_system.Data;
using ecommerce_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_system.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppliactionUser> _userManager;

        public ReviewsController(ApplicationDbContext context, UserManager<AppliactionUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //// POST: /Reviews/Submit
        //[HttpPost]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Submit(int productId, int rating, string comment)
        //{
        //    // 1. Validate the product exists
        //    var product = await _context.proudects.FindAsync(productId);
        //    if (product == null)
        //        return NotFound();

        //    // 2. Validate inputs
        //    if (rating < 1 || rating > 5)
        //    {
        //        TempData["error"] = "Rating must be between 1 and 5.";
        //        return RedirectToAction("Details", "Proudects", new { id = productId });
        //    }

        //    if (string.IsNullOrWhiteSpace(comment) || comment.Length > 1000)
        //    {
        //        TempData["error"] = "Comment is required and must be under 1000 characters.";
        //        return RedirectToAction("Details", "Proudects", new { id = productId });
        //    }

        //    // 3. Get current user
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //        return Unauthorized();

        //    // 4. Prevent duplicate review from same user on same product
        //    bool alreadyReviewed = await _context.review
        //        .AnyAsync(r => r.ProudectId == productId && r.UserId == user.Id);

        //    if (alreadyReviewed)
        //    {
        //        TempData["error"] = "You have already reviewed this product.";
        //        return RedirectToAction("Details", "Proudects", new { id = productId });
        //    }

        //    // 5. Save review
        //    var review = new Review
        //    {
        //        ProudectId = productId,
        //        UserId = user.Id,
        //        Rating = rating,
        //        comment = comment.Trim(),
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    _context.review.Add(review);
        //    await _context.SaveChangesAsync();

        //    TempData["success"] = "Your review has been submitted. Thank you!";
        //    return RedirectToAction("Details", "Proudects", new { id = productId });
        //}

        //// POST: /Reviews/Delete/5
        //[HttpPost]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var review = await _context.review.FindAsync(id);
        //    if (review == null)
        //        return NotFound();

        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //        return Unauthorized();

        //    // Only the review author or an Admin can delete
        //    bool isAdmin = User.IsInRole("Admin");
        //    if (review.UserId != user.Id && !isAdmin)
        //        return Forbid();

        //    int productId = review.ProudectId;
        //    _context.review.Remove(review);
        //    await _context.SaveChangesAsync();

        //    TempData["info"] = "Review deleted.";
        //    return RedirectToAction("Details", "Proudects", new { id = productId });
        //}

        /* ═══════════════════════════════════════════════════
           SUBMIT  (POST) — one review per user per product
        ═══════════════════════════════════════════════════ */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int productId, int rating, string comment)
        {
            var userId = _userManager.GetUserId(User);

            // Guard: already reviewed this product
            bool alreadyReviewed = await _context.review
                .AnyAsync(r => r.ProudectId == productId && r.UserId == userId);

            if (alreadyReviewed)
            {
                TempData["info"] = "You have already reviewed this product. You can edit or delete your existing review.";
                return RedirectToAction("Details", "Proudects", new { id = productId });
            }

            if (rating < 1 || rating > 5)
            {
                TempData["error"] = "Please select a star rating.";
                return RedirectToAction("Details", "Proudects", new { id = productId });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["error"] = "Please write a comment before submitting.";
                return RedirectToAction("Details", "Proudects", new { id = productId });
            }

            var review = new Review
            {
                ProudectId = productId,
                UserId = userId!,
                Rating = rating,
                comment = comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.review.Add(review);
            await _context.SaveChangesAsync();

            TempData["success"] = "Your review has been posted!";
            return RedirectToAction("Details", "Proudects", new { id = productId });
        }

        /* ═══════════════════════════════════════════════════
           EDIT  (POST) — owner only
        ═══════════════════════════════════════════════════ */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int rating, string comment)
        {
            var userId = _userManager.GetUserId(User);
            var review = await _context.review.FindAsync(id);

            if (review == null)
                return NotFound();

            // Only the owner can edit (admin cannot edit on behalf)
            if (review.UserId != userId)
            {
                TempData["error"] = "You can only edit your own reviews.";
                return RedirectToAction("Details", "Proudects", new { id = review.ProudectId });
            }

            if (rating < 1 || rating > 5)
            {
                TempData["error"] = "Please select a valid star rating.";
                return RedirectToAction("Details", "Proudects", new { id = review.ProudectId });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["error"] = "Review comment cannot be empty.";
                return RedirectToAction("Details", "Proudects", new { id = review.ProudectId });
            }

            review.Rating = rating;
            review.comment = comment.Trim();
            // Note: CreatedAt is intentionally NOT updated — keeps the original date

            await _context.SaveChangesAsync();

            TempData["success"] = "Your review has been updated.";
            return RedirectToAction("Details", "Proudects", new { id = review.ProudectId });
        }

        /* ═══════════════════════════════════════════════════
           DELETE  (POST) — owner or Admin
        ═══════════════════════════════════════════════════ */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var review = await _context.review.FindAsync(id);

            if (review == null)
                return NotFound();

            bool isOwner = review.UserId == userId;
            bool isAdmin = User.IsInRole("Admin");

            if (!isOwner && !isAdmin)
            {
                TempData["error"] = "You don't have permission to delete this review.";
                return RedirectToAction("Details", "Proudects", new { id = review.ProudectId });
            }

            var productId = review.ProudectId;
            _context.review.Remove(review);
            await _context.SaveChangesAsync();

            TempData["success"] = "Review deleted.";
            return RedirectToAction("Details", "Proudects", new { id = productId });
        }
    }
}
using ecommerce_system.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ecommerce_system.Models;

namespace ecommerce_system.Controllers
{
    [Authorize]
    public class WishListItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WishListItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: WishListItems/AddToWishlist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Json(new { success = false, message = "Unauthorized" });

            // 1. Get or Create the WishList header
            var userWishlist = await _context.wishList
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (userWishlist == null)
            {
                userWishlist = new WishList { UserId = userId };
                _context.wishList.Add(userWishlist);
                await _context.SaveChangesAsync();
            }

            // 2. Check if item already exists
            var exists = await _context.wishListItems
                .AnyAsync(w => w.ProudectId == productId && w.WishListId == userWishlist.Id);

            if (!exists)
            {
                var newItem = new WishListItem
                {
                    ProudectId = productId,
                    WishListId = userWishlist.Id
                };
                _context.wishListItems.Add(newItem);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Added to wishlist" });
            }

            return Json(new { success = true, message = "Already in wishlist" });
        }

        // GET: WishListItems
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Load wishlist items and "reach through" to get product details
            var items = await _context.wishListItems
                .Include(w => w.Proudect)
                    .ThenInclude(p => p!.Category)     // Load Category
                .Include(w => w.Proudect)
                    .ThenInclude(p => p!.Discounts)    // Load Discounts for badge/price
                .Include(w => w.Proudect)
                    .ThenInclude(p => p!.Reviews)      // Load Reviews for stars
                .Where(w => w.WishList.UserId == userId)
                .ToListAsync();

            return View(items);
        }


        // GET: WishListItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wishListItem = await _context.wishListItems
                .Include(w => w.Proudect)
                .Include(w => w.WishList)
                .FirstOrDefaultAsync(m => m.Id == id && m.WishList.UserId == userId);

            if (wishListItem == null)
            {
                return NotFound();
            }

            return View(wishListItem);
        }

        // GET: WishListItems/Create
        public IActionResult Create()
        {
            ViewData["ProudectId"] = new SelectList(_context.proudects, "Id", "Description");
            ViewData["WishListId"] = new SelectList(_context.wishList, "Id", "Id");
            return View();
        }

        // POST: WishListItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,WishListId,ProudectId")] WishListItem wishListItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(wishListItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProudectId"] = new SelectList(_context.proudects, "Id", "Description", wishListItem.ProudectId);
            ViewData["WishListId"] = new SelectList(_context.wishList, "Id", "Id", wishListItem.WishListId);
            return View(wishListItem);
        }

        // GET: WishListItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wishListItem = await _context.wishListItems
                .FirstOrDefaultAsync(m => m.Id == id && m.WishList.UserId == userId);

            if (wishListItem == null)
            {
                return NotFound();
            }
            ViewData["ProudectId"] = new SelectList(_context.proudects, "Id", "Description", wishListItem.ProudectId);
            ViewData["WishListId"] = new SelectList(_context.wishList, "Id", "Id", wishListItem.WishListId);
            return View(wishListItem);
        }

        // POST: WishListItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,WishListId,ProudectId")] WishListItem wishListItem)
        {
            if (id != wishListItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wishListItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WishListItemExists(wishListItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProudectId"] = new SelectList(_context.proudects, "Id", "Description", wishListItem.ProudectId);
            ViewData["WishListId"] = new SelectList(_context.wishList, "Id", "Id", wishListItem.WishListId);
            return View(wishListItem);
        }

        // GET: WishListItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wishListItem = await _context.wishListItems
                .Include(w => w.Proudect)
                .Include(w => w.WishList)
                .FirstOrDefaultAsync(m => m.Id == id && m.WishList.UserId == userId);

            if (wishListItem == null)
            {
                return NotFound();
            }

            return View(wishListItem);
        }

        // POST: WishListItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wishListItem = await _context.wishListItems
                .FirstOrDefaultAsync(m => m.Id == id && m.WishList.UserId == userId);

            if (wishListItem != null)
            {
                _context.wishListItems.Remove(wishListItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool WishListItemExists(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return _context.wishListItems.Any(e => e.Id == id && e.WishList.UserId == userId);
        }
    }
}
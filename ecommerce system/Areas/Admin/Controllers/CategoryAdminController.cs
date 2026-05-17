using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce_system.Data;
using ecommerce_system.Models;

namespace ecommerce_system.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchTerm) 
        {
            IQueryable<Category> query = _context.categories.Include(c => c.Proudects);
            //نبني Query :  using IQueryable

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm));
            }

            ViewBag.SearchTerm = searchTerm;

            ViewBag.TotalCategories = await _context.categories.CountAsync(); 

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.categories 
                .Include(c => c.Proudects)
                    .ThenInclude(p => p.Images)
                        .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        public IActionResult Create()
        {
            return View(); //  open page بس 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Category category, IFormFile? imageFile) //just bind the id , name 
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("Img", "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed.");
                        return View(category);
                    }

                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Categories");
                    Directory.CreateDirectory(folder);
                    var fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    var filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    category.Img = fileName;
                }

                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["success"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Img")] Category category, IFormFile? imageFile)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("Img", "Only image files are allowed.");
                            return View(category);
                        }

                        // Delete old image from disk
                        var existingCategory = await _context.categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                        if (existingCategory != null && !string.IsNullOrEmpty(existingCategory.Img))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Categories", existingCategory.Img);
                            if (System.IO.File.Exists(oldImagePath))
                                System.IO.File.Delete(oldImagePath);
                        }

                        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Categories");
                        Directory.CreateDirectory(folder);
                        var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        var filePath = Path.Combine(folder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        category.Img = fileName;
                    }
                    else
                    {
                        _context.Entry(category).Property(x => x.Img).IsModified = false;
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["info"] = "Category updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.categories.Any(e => e.Id == category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.categories
                .Include(c => c.Proudects)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.categories.FindAsync(id);
            if (category != null)
            {
                if (!string.IsNullOrEmpty(category.Img))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Categories", category.Img);
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                _context.categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

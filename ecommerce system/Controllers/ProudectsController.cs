using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ecommerce_system.Data;
using ecommerce_system.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using ecommerce_system.ViewModel;

namespace ecommerce_system.Controllers
{
    public class ProudectsController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // GET: Proudects
        public async Task<IActionResult> Index(int? categoryId)
        {
            // Start with all products including category info
            IQueryable<Proudect> query = _context.proudects.Include(p => p.Category);

            // If a categoryId is provided, filter the list
            if (categoryId != null)
            {
                query = query.Where(p => p.CategoryId == categoryId);

                // Optional: Pass the category name to the view to show in a heading
                var category = await _context.categories.FindAsync(categoryId);
                ViewBag.FilteredCategory = category?.Name;
            }

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proudect = await _context.proudects
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (proudect == null)
            {
                return NotFound();
            }

            return View(proudect);
        }

        // GET: Proudects/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.categories, dataValueField: "Id", dataTextField: "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProudectCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var proudect = new Proudect
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    Images = [],
                    Discounts = []
                };

                // Handle main image for backwards compatibility if needed, or just set it to the first image.
                string? firstImgPath = null;

                if (model.Uploads != null && model.Uploads.Count > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    foreach (var file in model.Uploads)
                    {
                        if (file.Length > 0)
                        {
                            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                            if (!allowedExtensions.Contains(extension))
                            {
                                ModelState.AddModelError("Uploads", "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed.");
                                ViewData["CategoryId"] = new SelectList(_context.categories, "Id", "Name", model.CategoryId);
                                return View(model);
                            }

                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products", fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var imgUrl = "/images/products/" + fileName;
                            proudect.Images.Add(new ProductImage { ImageUrl = imgUrl });
                            
                            firstImgPath ??= imgUrl;
                        }
                    }
                }

                proudect.Img = firstImgPath; // Optional: still set the main Img string if you want.

                if (model.DiscountPercent.HasValue && model.DiscountPercent.Value > 0)
                {
                    proudect.Discounts.Add(new Discount 
                    { 
                        Name = "Default Discount", 
                        DiscountPercent = model.DiscountPercent.Value,
                        Active = true
                    });
                }

                _context.Add(proudect);
                await _context.SaveChangesAsync();
                TempData["success"] = "Product created successfully";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.categories, dataValueField: "Id", dataTextField: "Name", selectedValue: model.CategoryId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proudect = await _context.proudects
                .Include(p => p.Images)
                .Include(p => p.Discounts)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (proudect == null)
            {
                return NotFound();
            }

            var activeDiscount = proudect.Discounts?.FirstOrDefault(d => d.Active);

            var model = new ProudectEditViewModel
            {
                Id = proudect.Id,
                Name = proudect.Name,
                Description = proudect.Description,
                Price = proudect.Price,
                CategoryId = proudect.CategoryId,
                ExistingImg = proudect.Img,
                ExistingImages = proudect.Images?.ToList() ?? new List<ProductImage>(),
                DiscountPercent = activeDiscount?.DiscountPercent
            };

            ViewData["CategoryId"] = new SelectList(_context.categories, "Id", "Name", proudect.CategoryId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProudectEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var proudect = await _context.proudects
                        .Include(p => p.Images)
                        .Include(p => p.Discounts)
                        .FirstOrDefaultAsync(p => p.Id == model.Id);
                        
                    if (proudect == null)
                    {
                        return NotFound();
                    }

                    proudect.Name = model.Name;
                    proudect.Description = model.Description;
                    proudect.Price = model.Price;
                    proudect.CategoryId = model.CategoryId;

                    // Update Discount
                    var activeDiscount = proudect.Discounts?.FirstOrDefault(d => d.Active);
                    if (model.DiscountPercent.HasValue && model.DiscountPercent.Value > 0)
                    {
                        if (activeDiscount != null)
                        {
                            activeDiscount.DiscountPercent = model.DiscountPercent.Value;
                        }
                        else
                        {
                            proudect.Discounts ??= [];
                            proudect.Discounts.Add(new Discount 
                            { 
                                Name = "Default Discount", 
                                DiscountPercent = model.DiscountPercent.Value,
                                Active = true
                            });
                        }
                    }
                    else if (activeDiscount != null)
                    {
                        activeDiscount.Active = false; // or remove it entirely
                    }

                    // Delete selected images
                    if (model.ImagesToDelete != null && model.ImagesToDelete.Count > 0)
                    {
                        var imagesToRemove = proudect.Images?
                            .Where(img => model.ImagesToDelete.Contains(img.Id))
                            .ToList() ?? [];

                        foreach (var img in imagesToRemove)
                        {
                            // Delete file from disk
                            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                            if (System.IO.File.Exists(physicalPath))
                                System.IO.File.Delete(physicalPath);

                            _context.productImages.Remove(img);
                        }

                        // Update main Img if it was deleted
                        if (!string.IsNullOrEmpty(proudect.Img) && imagesToRemove.Any(i => i.ImageUrl == proudect.Img))
                        {
                            proudect.Img = proudect.Images?
                                .FirstOrDefault(i => !model.ImagesToDelete.Contains(i.Id))?.ImageUrl;
                        }
                    }

                    // Handle new images
                    if (model.Uploads != null && model.Uploads.Count > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        foreach (var file in model.Uploads)
                        {
                            if (file.Length > 0)
                            {
                                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                                if (!allowedExtensions.Contains(extension))
                                {
                                    ModelState.AddModelError("Uploads", "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed.");
                                    model.ExistingImages = proudect.Images?.ToList() ?? new List<ProductImage>();
                                    ViewData["CategoryId"] = new SelectList(_context.categories, "Id", "Name", model.CategoryId);
                                    return View(model);
                                }
                            }
                        }

                        proudect.Images ??= [];
                        
                        foreach (var file in model.Uploads)
                        {
                            if (file.Length > 0)
                            {
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products", fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                var imgUrl = "/images/products/" + fileName;
                                proudect.Images.Add(new ProductImage { ImageUrl = imgUrl });
                                
                                // Update main image if we didn't have one
                                if (string.IsNullOrEmpty(proudect.Img))
                                {
                                    proudect.Img = imgUrl;
                                }
                            }
                        }
                    }

                    _context.Update(proudect);
                    await _context.SaveChangesAsync();
                    TempData["info"] = "Product updated successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProudectExists(model.Id))
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
            TempData["error"] = "Please fix the errors below.";
            ViewData["CategoryId"] = new SelectList(_context.categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proudect = await _context.proudects
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (proudect == null)
            {
                return NotFound();
            }

            return View(proudect);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proudect = await _context.proudects.FindAsync(id);
            if (proudect != null)
            {
                _context.proudects.Remove(proudect);
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Product deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        private bool ProudectExists(int id)
        {
            return _context.proudects.Any(e => e.Id == id);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ecommerce_system.Data;
using ecommerce_system.Models;
using ecommerce_system.ViewModel;

namespace ecommerce_system.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/ProductsAdmin
        public async Task<IActionResult> Index(int? categoryId, string? searchTerm)
        {
            IQueryable<Proudect> query = _context.proudects
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Discounts);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            ViewBag.Categories = new SelectList(_context.categories, "Id", "Name", categoryId);
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.TotalProducts = await _context.proudects.CountAsync();

            return View(await query.ToListAsync());
        }

        // GET: Admin/ProductsAdmin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.proudects
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Discounts)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Admin/ProductsAdmin/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.categories, "Id", "Name");
            return View();
        }

        // POST: Admin/ProductsAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProudectCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Step 1: Save product without images or discounts
                var product = new Proudect
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId
                };

                // Handle image file uploads (save files to disk first)
                var uploadedImages = new List<string>();
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

                            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                            Directory.CreateDirectory(folder);
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(folder, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            uploadedImages.Add("/images/products/" + fileName);
                        }
                    }
                }

                product.Img = uploadedImages.FirstOrDefault();

                _context.proudects.Add(product);
                await _context.SaveChangesAsync();

                // Step 2: Insert images via raw SQL (avoids MERGE)
                foreach (var imgUrl in uploadedImages)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO ProductImages (ImageUrl, ProudectId) VALUES (@p0, @p1)",
                        imgUrl, product.Id);
                }

                // Step 3: Insert discount via raw SQL (avoids MERGE)
                if (model.DiscountPercent.HasValue && model.DiscountPercent.Value > 0)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO Discounts (Name, DiscountPercent, Active, ProudectId) VALUES (@p0, @p1, @p2, @p3)",
                        "Default Discount", model.DiscountPercent.Value, true, product.Id);
                }

                TempData["success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // GET: Admin/ProductsAdmin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.proudects
                .Include(p => p.Images)
                .Include(p => p.Discounts)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var activeDiscount = product.Discounts?.FirstOrDefault(d => d.Active);

            var model = new ProudectEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                ExistingImg = product.Img,
                ExistingImages = product.Images?.ToList() ?? new List<ProductImage>(),
                DiscountPercent = activeDiscount?.DiscountPercent
            };

            ViewData["CategoryId"] = new SelectList(_context.categories, "Id", "Name", product.CategoryId);
            return View(model);
        }

        // POST: Admin/ProductsAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProudectEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _context.proudects
                        .Include(p => p.Images)
                        .Include(p => p.Discounts)
                        .FirstOrDefaultAsync(p => p.Id == model.Id);

                    if (product == null) return NotFound();

                    product.Name = model.Name;
                    product.Description = model.Description;
                    product.Price = model.Price;
                    product.CategoryId = model.CategoryId;

                    // Update Discount
                    var activeDiscount = product.Discounts?.FirstOrDefault(d => d.Active);
                    bool needNewDiscount = false;
                    if (model.DiscountPercent.HasValue && model.DiscountPercent.Value > 0)
                    {
                        if (activeDiscount != null)
                        {
                            activeDiscount.DiscountPercent = model.DiscountPercent.Value;
                        }
                        else
                        {
                            needNewDiscount = true;
                        }
                    }
                    else if (activeDiscount != null)
                    {
                        activeDiscount.Active = false;
                    }

                    // Delete selected images
                    if (model.ImagesToDelete != null && model.ImagesToDelete.Count > 0)
                    {
                        var imagesToRemove = product.Images?
                            .Where(img => model.ImagesToDelete.Contains(img.Id))
                            .ToList() ?? [];

                        foreach (var img in imagesToRemove)
                        {
                            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                            if (System.IO.File.Exists(physicalPath))
                                System.IO.File.Delete(physicalPath);
                            product.Images!.Remove(img);
                            _context.productImages.Remove(img);
                        }

                        if (!string.IsNullOrEmpty(product.Img) && imagesToRemove.Any(i => i.ImageUrl == product.Img))
                        {
                            product.Img = product.Images?
                                .FirstOrDefault()?.ImageUrl;
                        }
                    }

                    // Save updates and deletions (no inserts = no MERGE)
                    await _context.SaveChangesAsync();

                    // Add new discount via raw SQL to avoid MERGE
                    if (needNewDiscount)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "INSERT INTO Discounts (Name, DiscountPercent, Active, ProudectId) VALUES (@p0, @p1, @p2, @p3)",
                            "Default Discount", model.DiscountPercent!.Value, true, product.Id);
                    }

                    // Add new images via raw SQL to avoid MERGE
                    if (model.Uploads != null && model.Uploads.Count > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        string? firstNewImgUrl = null;

                        foreach (var file in model.Uploads)
                        {
                            if (file.Length > 0)
                            {
                                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                                if (!allowedExtensions.Contains(extension))
                                {
                                    // Reload images for the view
                                    var reloadedProduct = await _context.proudects
                                        .Include(p => p.Images)
                                        .FirstOrDefaultAsync(p => p.Id == model.Id);
                                    model.ExistingImages = reloadedProduct?.Images?.ToList() ?? new List<ProductImage>();
                                    ViewData["CategoryId"] = new SelectList(_context.categories, "Id", "Name", model.CategoryId);
                                    ModelState.AddModelError("Uploads", "Only image files are allowed.");
                                    return View(model);
                                }

                                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                                Directory.CreateDirectory(folder);
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var filePath = Path.Combine(folder, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                var imgUrl = "/images/products/" + fileName;

                                await _context.Database.ExecuteSqlRawAsync(
                                    "INSERT INTO ProductImages (ImageUrl, ProudectId) VALUES (@p0, @p1)",
                                    imgUrl, product.Id);

                                if (firstNewImgUrl == null)
                                    firstNewImgUrl = imgUrl;
                            }
                        }

                        // Update main image if needed
                        if (string.IsNullOrEmpty(product.Img) && firstNewImgUrl != null)
                        {
                            product.Img = firstNewImgUrl;
                            await _context.SaveChangesAsync();
                        }
                    }

                    TempData["info"] = "Product updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.proudects.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // GET: Admin/ProductsAdmin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.proudects
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Admin/ProductsAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.proudects
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                // Delete image files from disk
                if (product.Images != null)
                {
                    foreach (var img in product.Images)
                    {
                        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(physicalPath))
                            System.IO.File.Delete(physicalPath);
                    }
                }
                _context.proudects.Remove(product);
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

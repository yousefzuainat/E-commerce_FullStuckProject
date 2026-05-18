
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

        public async Task<IActionResult> Index(int? categoryId, string? searchTerm)
        {
            IQueryable<Proudect> query = _context.proudects
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Discounts);

            if (categoryId.HasValue) //  اعمل فلتر 
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            ViewBag.Categories = new SelectList(_context.categories, dataValueField: "Id", "Name", categoryId); // Dropdown 
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.TotalProducts = await _context.proudects.CountAsync();

            return View(await query.ToListAsync());
        }

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

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.categories, "Id", "Name"); //dropdown
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProudectCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = new Proudect
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId
                    //بدون الصوره و الخصم 
                };

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

                bool isFirst = true;
                foreach (var imgUrl in uploadedImages)
                {
                    _context.productImages.Add(new ProductImage
                    {
                        ImageUrl = imgUrl,
                        ProudectId = product.Id,
                        IsMain = isFirst 
                    });
                    isFirst = false;
                }

                if (model.DiscountPercent.HasValue && model.DiscountPercent.Value > 0)
                {
                    _context.discounts.Add(new Discount
                    {
                        Name = "Default Discount",
                        DiscountPercent = model.DiscountPercent.Value,
                        Active = true,
                        ProudectId = product.Id
                    });
                }

                await _context.SaveChangesAsync();

                TempData["success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

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
                ExistingImg = product.Img, // Main image URL 
                ExistingImages = product.Images?.ToList() ?? new List<ProductImage>(),
                DiscountPercent = activeDiscount?.DiscountPercent
            };

            ViewData["CategoryId"] = new SelectList(_context.categories, "Id", "Name", product.CategoryId);
            return View(model);
        }

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

                    // حذف الصور المحددة
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

                        // إذا حُذفت الصورة الرئيسية، اجعل أول صورة متبقية هي الرئيسية
                        if (imagesToRemove.Any(i => i.IsMain))
                        {
                            var firstRemaining = product.Images?.FirstOrDefault();
                            if (firstRemaining != null)
                            {
                                firstRemaining.IsMain = true;
                                product.Img = firstRemaining.ImageUrl;
                            }
                            else
                            {
                                product.Img = null;
                            }
                        }
                        else if (!string.IsNullOrEmpty(product.Img) && imagesToRemove.Any(i => i.ImageUrl == product.Img))
                        {
                            product.Img = product.Images?.FirstOrDefault()?.ImageUrl;
                        }
                    }

                    // تغيير الصورة الرئيسية إذا اختار الأدمن صورة موجودة
                    if (model.MainImageId.HasValue && model.MainImageId.Value > 0)
                    {
                        var allImages = product.Images?.ToList() ?? new List<ProductImage>();
                        foreach (var img in allImages)
                        {
                            img.IsMain = (img.Id == model.MainImageId.Value);
                        }
                        var newMain = allImages.FirstOrDefault(i => i.Id == model.MainImageId.Value);
                        if (newMain != null)
                            product.Img = newMain.ImageUrl;
                    }

                    await _context.SaveChangesAsync();

                    if (needNewDiscount)
                    {
                        _context.discounts.Add(new Discount
                        {
                            Name = "Default Discount",
                            DiscountPercent = model.DiscountPercent!.Value,
                            Active = true,
                            ProudectId = product.Id
                        });
                        await _context.SaveChangesAsync();
                    }

                    if (model.Uploads != null && model.Uploads.Count > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        bool hasExistingMain = product.Images?.Any(i => i.IsMain) == true;
                        bool isFirstUpload = !hasExistingMain;
                        string? firstNewImgUrl = null;

                        foreach (var file in model.Uploads)
                        {
                            if (file.Length > 0)
                            {
                                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                                if (!allowedExtensions.Contains(extension))
                                {
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
                                var isMain = isFirstUpload; // أول صورة مرفوعة جديدة تكون رئيسية إن لم تكن هناك رئيسية

                                _context.productImages.Add(new ProductImage
                                {
                                    ImageUrl = imgUrl,
                                    ProudectId = product.Id,
                                    IsMain = isMain
                                });
                                await _context.SaveChangesAsync();

                                if (firstNewImgUrl == null)
                                    firstNewImgUrl = imgUrl;

                                isFirstUpload = false; // بعد الأولى، بقية الصور ليست رئيسية
                            }
                        }

                        // إذا لم يكن هناك صورة رئيسية، اضبط أول صورة جديدة
                        if (!hasExistingMain && firstNewImgUrl != null)
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.proudects
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                if (product.Images != null)
                {
                    foreach (var img in product.Images)
                    {
                        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(physicalPath))
                            System.IO.File.Delete(physicalPath);
                    }
                }

                if (!string.IsNullOrEmpty(product.Img))
                {
                    var mainImgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.Img.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(mainImgPath))
                        System.IO.File.Delete(mainImgPath);
                }

                _context.proudects.Remove(product);
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

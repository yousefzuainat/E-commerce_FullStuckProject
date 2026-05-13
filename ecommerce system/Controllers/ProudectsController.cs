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
    public class ProudectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProudectsController(ApplicationDbContext context)
        {
            _context = context;
        }

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
                string? imgPath = null;
                if (model.Uploads != null && model.Uploads.Count > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Uploads[0].FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Uploads[0].CopyToAsync(stream);
                    }

                    imgPath = "/images/products/" + fileName;
                }

                var proudect = new Proudect
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    Img = imgPath
                };

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

            var proudect = await _context.proudects.FindAsync(id);
            if (proudect == null)
            {
                return NotFound();
            }

            var model = new ProudectEditViewModel
            {
                Id = proudect.Id,
                Name = proudect.Name,
                Description = proudect.Description,
                Price = proudect.Price,
                CategoryId = proudect.CategoryId,
                ExistingImg = proudect.Img
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
                    var proudect = await _context.proudects.FindAsync(model.Id);
                    if (proudect == null)
                    {
                        return NotFound();
                    }

                    proudect.Name = model.Name;
                    proudect.Description = model.Description;
                    proudect.Price = model.Price;
                    proudect.CategoryId = model.CategoryId;

                    if (model.Uploads != null && model.Uploads.Count > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Uploads[0].FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Uploads[0].CopyToAsync(stream);
                        }

                        proudect.Img = "/images/products/" + fileName;
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

        //public async Task<IActionResult> Deals()
        //{
        //    var dealsProducts = await _context.proudects;
        //     return View();

        //}
    }
}

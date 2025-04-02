using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderAutomation.Data;
using OrderAutomation.Models;

namespace OrderAutomation.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try 
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(m => m.Id == id);
                    
                if (category == null)
                {
                    return NotFound();
                }
                
                if (category.Name == "Без категории")
                {
                    ModelState.AddModelError(string.Empty, "Категорию 'Без категории' нельзя удалить, так как она используется системой по умолчанию.");
                    return View(category);
                }
                
                if (category.Products != null && category.Products.Any())
                {
                    var defaultCategory = await _context.Categories
                        .Where(c => c.Name == "Без категории" && c.Id != id)
                        .FirstOrDefaultAsync();
                    
                    if (defaultCategory == null)
                    {
                        defaultCategory = new Category { Name = "Без категории", Description = "Автоматически созданная категория для продуктов без категории" };
                        _context.Categories.Add(defaultCategory);
                        await _context.SaveChangesAsync();
                    }
                    
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            foreach (var product in category.Products)
                            {
                                var freshProduct = await _context.Products.FindAsync(product.Id);
                                if (freshProduct != null)
                                {
                                    freshProduct.CategoryId = defaultCategory.Id;
                                    await _context.SaveChangesAsync();
                                }
                            }
                            
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            ModelState.AddModelError(string.Empty, $"Ошибка при обновлении продуктов: {ex.Message}");
                            return View(category);
                        }
                    }
                }
                
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(m => m.Id == id);
                    
                if (category == null)
                {
                    return NotFound();
                }
                
                ModelState.AddModelError(string.Empty, $"Произошла ошибка: {ex.Message}");
                return View(category);
            }
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
} 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrderAutomation.Data;
using OrderAutomation.Models;

namespace OrderAutomation.Controllers
{
    public class ProductReceiptsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductReceiptsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProductReceipts
        public async Task<IActionResult> Index()
        {
            var productReceipts = await _context.ProductReceipts
                .Include(p => p.Product)
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.ReceiptDate)
                .ToListAsync();
            return View(productReceipts);
        }

        // GET: ProductReceipts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productReceipt = await _context.ProductReceipts
                .Include(p => p.Product)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productReceipt == null)
            {
                return NotFound();
            }

            return View(productReceipt);
        }

        // GET: ProductReceipts/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name");
            
            var productReceipt = new ProductReceipt
            {
                ReceiptDate = DateTime.Now,
                InvoiceNumber = GenerateInvoiceNumber()
            };
            
            return View(productReceipt);
        }

        // POST: ProductReceipts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductId,ReceiptDate,Quantity,SupplierId,InvoiceNumber,PurchasePrice,Notes")] ProductReceipt productReceipt)
        {
            if (string.IsNullOrEmpty(productReceipt.InvoiceNumber))
            {
                productReceipt.InvoiceNumber = GenerateInvoiceNumber();
                ModelState.Remove("InvoiceNumber");
            }

            if (ModelState.IsValid)
            {
                _context.Add(productReceipt);
                
                var product = await _context.Products.FindAsync(productReceipt.ProductId);
                if (product != null)
                {
                    product.StockQuantity += productReceipt.Quantity;
                    _context.Update(product);
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", productReceipt.ProductId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", productReceipt.SupplierId);
            return View(productReceipt);
        }

        // GET: ProductReceipts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productReceipt = await _context.ProductReceipts.FindAsync(id);
            if (productReceipt == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", productReceipt.ProductId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", productReceipt.SupplierId);
            return View(productReceipt);
        }

        // POST: ProductReceipts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,ReceiptDate,Quantity,SupplierId,InvoiceNumber,PurchasePrice,Notes")] ProductReceipt productReceipt, int originalQuantity)
        {
            if (id != productReceipt.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalReceipt = await _context.ProductReceipts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    
                    _context.Update(productReceipt);
                    
                    if (originalReceipt != null && originalReceipt.Quantity != productReceipt.Quantity)
                    {
                        var product = await _context.Products.FindAsync(productReceipt.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity = product.StockQuantity - originalReceipt.Quantity + productReceipt.Quantity;
                            _context.Update(product);
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductReceiptExists(productReceipt.Id))
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
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", productReceipt.ProductId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", productReceipt.SupplierId);
            return View(productReceipt);
        }

        // GET: ProductReceipts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productReceipt = await _context.ProductReceipts
                .Include(p => p.Product)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productReceipt == null)
            {
                return NotFound();
            }

            return View(productReceipt);
        }

        // POST: ProductReceipts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productReceipt = await _context.ProductReceipts.FindAsync(id);
            if (productReceipt != null)
            {
                var product = await _context.Products.FindAsync(productReceipt.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= productReceipt.Quantity;
                    _context.Update(product);
                }
                
                _context.ProductReceipts.Remove(productReceipt);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductReceiptExists(int id)
        {
            return _context.ProductReceipts.Any(e => e.Id == id);
        }

        private string GenerateInvoiceNumber()
        {
            string prefix = "ПТ-" + DateTime.Now.ToString("yyyyMMdd");
            var latestReceipt = _context.ProductReceipts
                .Where(r => r.InvoiceNumber != null && r.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(r => r.InvoiceNumber)
                .FirstOrDefault();
                
            int number = 1;
            if (latestReceipt != null && 
                latestReceipt.InvoiceNumber.Length > prefix.Length + 1 && 
                int.TryParse(latestReceipt.InvoiceNumber.Substring(prefix.Length + 1), out int lastNumber))
            {
                number = lastNumber + 1;
            }
            
            return $"{prefix}-{number:D3}";
        }

        // GET: ProductReceipts/CreateMultiple
        public IActionResult CreateMultiple()
        {
            ViewData["Products"] = new SelectList(_context.Products, "Id", "Name");
            ViewData["Suppliers"] = new SelectList(_context.Suppliers, "Id", "Name");
            
            return View();
        }

        // POST: ProductReceipts/CreateMultiple
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMultiple(int[] productIds, int[] quantities, int supplierId, DateTime receiptDate, string? invoiceNumber, decimal? purchasePrice, string? notes)
        {
            if (productIds == null || quantities == null || productIds.Length == 0 || productIds.Length != quantities.Length)
            {
                TempData["Error"] = "Необходимо указать хотя бы один товар и его количество";
                ViewData["Products"] = new SelectList(_context.Products, "Id", "Name");
                ViewData["Suppliers"] = new SelectList(_context.Suppliers, "Id", "Name");
                return View();
            }

            if (string.IsNullOrEmpty(invoiceNumber))
            {
                invoiceNumber = GenerateInvoiceNumber();
            }

            for (int i = 0; i < productIds.Length; i++)
            {
                if (productIds[i] > 0 && quantities[i] > 0)
                {
                    var productReceipt = new ProductReceipt
                    {
                        ProductId = productIds[i],
                        Quantity = quantities[i],
                        SupplierId = supplierId,
                        ReceiptDate = receiptDate,
                        InvoiceNumber = invoiceNumber,
                        PurchasePrice = purchasePrice,
                        Notes = notes
                    };

                    _context.Add(productReceipt);
                    
                    var product = await _context.Products.FindAsync(productIds[i]);
                    if (product != null)
                    {
                        product.StockQuantity += quantities[i];
                        _context.Update(product);
                    }
                }
            }
            
            await _context.SaveChangesAsync();
            TempData["Success"] = "Товары успешно оприходованы";
            return RedirectToAction(nameof(Index));
        }
    }
} 
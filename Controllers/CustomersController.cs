using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderAutomation.Data;
using OrderAutomation.Models;

namespace OrderAutomation.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Customers.ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyName,ContactName,Position,Phone,Email,Address,INN,KPP")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName,ContactName,Position,Phone,Email,Address,INN,KPP")] Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
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
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (customer != null)
            {
                if (customer.Orders != null)
                {
                    foreach (var order in customer.Orders)
                    {
                        if (order.OrderItems != null)
                        {
                            foreach (var item in order.OrderItems.Where(i => i.Product != null))
                            {
                                if (item.Product != null)
                                {
                                    item.Product.StockQuantity += item.Quantity;
                                    _context.Update(item.Product);
                                }
                            }
                            _context.OrderItems.RemoveRange(order.OrderItems);
                        }
                    }   
                    
                    _context.Orders.RemoveRange(customer.Orders);
                }
                
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
        
        // POST: Customers/CreateOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            
            if (customer == null)
            {
                return NotFound();
            }
            
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                CreatedDate = DateTime.Now,
                Status = OrderStatus.New,
                CustomerId = id,
                TotalAmount = 0
            };
            
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Создан новый заказ №{order.OrderNumber} для клиента {customer.CompanyName}";
            
            return RedirectToAction("Edit", "Orders", new { id = order.Id });
        }
        
        private string GenerateOrderNumber()
        {
            string prefix = DateTime.Now.ToString("yyyyMMdd");
            var latestOrder = _context.Orders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefault();
                
            int number = 1;
            if (latestOrder != null && int.TryParse(latestOrder.OrderNumber.Substring(prefix.Length), out int lastNumber))
            {
                number = lastNumber + 1;
            }
            
            return $"{prefix}{number:D4}";
        }
    }
} 
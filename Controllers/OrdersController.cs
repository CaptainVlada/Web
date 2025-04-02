using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrderAutomation.Data;
using OrderAutomation.Models;
using System.Text;

namespace OrderAutomation.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index(string searchString, string statusFilter, DateTime? dateFrom, DateTime? dateTo)
        {
            var orders = _context.Orders
                .Include(o => o.Customer)
                .AsQueryable();
                
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatusFilter"] = statusFilter;
            ViewData["CurrentDateFrom"] = dateFrom?.ToString("yyyy-MM-dd");
            ViewData["CurrentDateTo"] = dateTo?.ToString("yyyy-MM-dd");
            
            ViewData["NewOrdersCount"] = await _context.Orders.CountAsync(o => o.Status == OrderStatus.New);
            ViewData["ProcessingOrdersCount"] = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Processing);
            ViewData["CompletedOrdersCount"] = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Completed);
            ViewData["CanceledOrdersCount"] = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Canceled);
            ViewData["TotalOrdersCount"] = await _context.Orders.CountAsync();
            
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => 
                    o.OrderNumber.Contains(searchString) || 
                    o.Customer.CompanyName.Contains(searchString)
                );
            }
            
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<OrderStatus>(statusFilter, out var status))
            {
                orders = orders.Where(o => o.Status == status);
            }
            
            if (dateFrom.HasValue)
            {
                orders = orders.Where(o => o.CreatedDate >= dateFrom.Value);
            }
            
            if (dateTo.HasValue)
            {
                orders = orders.Where(o => o.CreatedDate <= dateTo.Value.AddDays(1)); // Включая выбранную дату до конца дня
            }
            
            return View(await orders.OrderByDescending(o => o.CreatedDate).ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/PrintInvoice/5
        public async Task<IActionResult> PrintInvoice(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            
            int itemCount = order.OrderItems?.Count ?? 0;
            TempData["Debug"] = $"Загружено товаров: {itemCount}";

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CompanyName");
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrderNumber,Status,CompletedDate,Notes,CustomerId")] Order order)
        {
            order.OrderNumber = GenerateOrderNumber();
            ModelState.Remove("OrderNumber");
            
            order.CreatedDate = DateTime.Now;
            ModelState.Remove("CreatedDate");
            
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = order.Id });
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CompanyName", order.CustomerId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            
            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Canceled)
            {
                TempData["Error"] = "Невозможно редактировать завершенный или отмененный заказ";
                return RedirectToAction("Details", new { id = order.Id });
            }
            
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CompanyName", order.CustomerId);
            ViewData["Products"] = new SelectList(_context.Products, "Id", "Name");
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderNumber,CreatedDate,Status,CompletedDate,Notes,CustomerId,TotalAmount")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }
            
            var existingOrder = await _context.Orders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (existingOrder != null && (existingOrder.Status == OrderStatus.Completed || existingOrder.Status == OrderStatus.Canceled))
            {
                TempData["Error"] = "Невозможно редактировать завершенный или отмененный заказ";
                return RedirectToAction("Details", new { id = order.Id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CompanyName", order.CustomerId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (order != null)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.StockQuantity += item.Quantity;
                        _context.Update(item.Product);
                    }
                }
                
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Orders/AddOrderItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrderItem(int orderId, int productId, int quantity)
        {
            if (orderId <= 0 || productId <= 0 || quantity <= 0)
            {
                return BadRequest();
            }
            
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }
            
            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Canceled)
            {
                TempData["Error"] = "Невозможно добавлять товары в завершенный или отмененный заказ";
                return RedirectToAction("Details", new { id = orderId });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            if (product.StockQuantity < quantity)
            {
                TempData["Error"] = $"Недостаточное количество продукта '{product.Name}' на складе. Доступно: {product.StockQuantity}";
                return RedirectToAction("Edit", new { id = orderId });
            }
            
            var orderItem = new OrderItem
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price
            };

            _context.OrderItems.Add(orderItem);
            
            product.StockQuantity -= quantity;
            _context.Update(product);
            
            order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            
            if (order != null)
            {
                order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
                
                if (order.Status == OrderStatus.New)
                {
                    order.Status = OrderStatus.Processing;
                }
                
                _context.Update(order);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Товар '{product.Name}' добавлен в заказ в количестве {quantity} шт.";
            
            return RedirectToAction("Edit", new { id = orderId });
        }

        // POST: Orders/RemoveOrderItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveOrderItem(int id)
        {
            var orderItem = await _context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (orderItem == null)
            {
                return NotFound();
            }
            
            if (orderItem.Order != null && (orderItem.Order.Status == OrderStatus.Completed || orderItem.Order.Status == OrderStatus.Canceled))
            {
                TempData["Error"] = "Невозможно удалить товары из завершенного или отмененного заказа";
                return RedirectToAction("Details", new { id = orderItem.OrderId });
            }

            int orderId = orderItem.OrderId;
            
            if (orderItem.Product != null)
            {
                orderItem.Product.StockQuantity += orderItem.Quantity;
                _context.Update(orderItem.Product);
            }
            
            _context.OrderItems.Remove(orderItem);
            
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            
            if (order != null)
            {
                order.TotalAmount = order.OrderItems.Where(oi => oi.Id != id).Sum(oi => oi.Quantity * oi.UnitPrice);
                _context.Update(order);
            }

            await _context.SaveChangesAsync();
            
            return RedirectToAction("Edit", new { id = orderId });
        }

        // POST: Orders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (order == null)
            {
                return NotFound();
            }

            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                TempData["Warning"] = "Заказ пуст. Нет товаров для возврата на склад.";
                order.Status = OrderStatus.Canceled;
                _context.Update(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                {
                    item.Product.StockQuantity += item.Quantity;
                    _context.Update(item.Product);
                }
            }
            
            order.Status = OrderStatus.Canceled;
            _context.Update(order);
            
            await _context.SaveChangesAsync();
            TempData["Success"] = "Заказ успешно отменен, товары возвращены на склад.";
            
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
        
        // POST: Orders/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (order == null)
            {
                return NotFound();
            }

            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                TempData["Warning"] = "Невозможно завершить пустой заказ. Добавьте товары в заказ.";
                return RedirectToAction("Edit", new { id = order.Id });
            }
            
            order.Status = OrderStatus.Completed;
            order.CompletedDate = DateTime.Now;
            _context.Update(order);
            
            await _context.SaveChangesAsync();
            TempData["Success"] = "Заказ успешно завершен.";
            
            return RedirectToAction(nameof(Index));
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
        
        // POST: Orders/Repeat/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Repeat(int id)
        {
            var originalOrder = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (originalOrder == null)
            {
                return NotFound();
            }
            
            var newOrder = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                CreatedDate = DateTime.Now,
                Status = OrderStatus.New,
                CustomerId = originalOrder.CustomerId,
                TotalAmount = 0
            };
            
            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();
            
            bool allItemsAvailable = true;
            var unavailableItems = new List<string>();
            
            if (originalOrder.OrderItems != null && originalOrder.OrderItems.Any())
            {
                foreach (var item in originalOrder.OrderItems)
                {
                    if (item.Product == null || item.ProductId == null)
                        continue;
                        
                    var currentProduct = await _context.Products.FindAsync(item.ProductId);
                    
                    if (currentProduct == null)
                    {
                        unavailableItems.Add($"{item.Product.Name} (товар больше не существует)");
                        allItemsAvailable = false;
                        continue;
                    }
                    
                    if (currentProduct.StockQuantity < item.Quantity)
                    {
                        unavailableItems.Add($"{currentProduct.Name} (доступно: {currentProduct.StockQuantity}, требуется: {item.Quantity})");
                        allItemsAvailable = false;
                        continue;
                    }
                    
                    var newOrderItem = new OrderItem
                    {
                        OrderId = newOrder.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = currentProduct.Price
                    };
                    
                    _context.OrderItems.Add(newOrderItem);
                    
                    currentProduct.StockQuantity -= item.Quantity;
                    _context.Update(currentProduct);
                    
                    newOrder.TotalAmount += newOrderItem.Quantity * newOrderItem.UnitPrice;
                }
                
                _context.Update(newOrder);
                await _context.SaveChangesAsync();
            }
            
            if (!allItemsAvailable)
            {
                TempData["Warning"] = $"Заказ создан, но не все товары доступны на складе. Недоступные товары: {string.Join(", ", unavailableItems)}";
            }
            else if (newOrder.TotalAmount > 0)
            {
                TempData["Success"] = "Заказ успешно повторен. Товары добавлены в новый заказ.";
            }
            else
            {
                TempData["Warning"] = "Создан пустой заказ. В исходном заказе не было товаров или они недоступны.";
            }
            
            return RedirectToAction("Edit", new { id = newOrder.Id });
        }

        // GET: Orders/ExportToCsv
        public async Task<IActionResult> ExportToCsv(string statusFilter = null)
        {
            var orders = _context.Orders
                .Include(o => o.Customer)
                .AsQueryable();
                
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<OrderStatus>(statusFilter, out var status))
            {
                orders = orders.Where(o => o.Status == status);
            }
            
            var ordersList = await orders.OrderByDescending(o => o.CreatedDate).ToListAsync();
            
            var csv = new StringBuilder();
            
            csv.AppendLine("Номер заказа;Дата;Клиент;Статус;Сумма");
            
            foreach (var order in ordersList)
            {
                string statusName = order.Status.ToString();
                
                csv.AppendLine($"{order.OrderNumber};{order.CreatedDate:dd.MM.yyyy};{order.Customer?.CompanyName ?? "Не указан"};{statusName};{order.TotalAmount:N2}");
            }
            
            byte[] bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            
            return File(bytes, "text/csv; charset=utf-8", $"orders_{DateTime.Now:yyyyMMdd}.csv");
        }

        // GET: Orders/GetProductInfo/5
        [HttpGet]
        public async Task<IActionResult> GetProductInfo(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            var priceStr = product.Price.ToString(System.Globalization.CultureInfo.InvariantCulture);
            
            return Json(new {
                name = product.Name,
                sku = product.SKU,
                price = decimal.Parse(priceStr, System.Globalization.CultureInfo.InvariantCulture),
                stock = product.StockQuantity
            });
        }
    }
} 
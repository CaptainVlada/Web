using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderAutomation.Data;
using OrderAutomation.Models;
using System.Diagnostics;

namespace OrderAutomation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["TotalProducts"] = await _context.Products.CountAsync();
            ViewData["TotalCustomers"] = await _context.Customers.CountAsync();
            ViewData["TotalOrders"] = await _context.Orders.CountAsync();
            
            ViewData["RecentOrders"] = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.CreatedDate)
                .Take(10)
                .ToListAsync();
            
            ViewData["RecentCustomers"] = await _context.Customers
                .OrderByDescending(c => c.Id)
                .Take(10)
                .ToListAsync();
            
            ViewData["RecentReceipts"] = await _context.ProductReceipts
                .Include(pr => pr.Product)
                .Include(pr => pr.Supplier)
                .OrderByDescending(pr => pr.ReceiptDate)
                .Take(10)
                .ToListAsync();
            
            ViewData["LowStockProducts"] = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity < 50)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToListAsync();
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

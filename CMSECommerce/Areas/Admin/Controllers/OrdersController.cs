using CMSECommerce.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMSECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize("Admin")]
    public class OrdersController(DataContext context) : Controller
    {
        private readonly DataContext _context = context;

        public async Task<IActionResult> Index()
        {
            List<Order> orders = await _context.Orders.OrderByDescending(x => x.Id).ToListAsync();

            return View(orders);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShippedStatus(int id, bool shipped)
        {
            Order order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id);

            order.Shipped = shipped;

            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["success"] = "The order has been modified!";

            return RedirectToAction("Index");
        }
    }
}
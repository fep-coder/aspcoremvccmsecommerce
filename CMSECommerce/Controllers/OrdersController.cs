using CMSECommerce.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CMSECommerce.Controllers
{
    public class OrdersController(
                        DataContext context,
                        SignInManager<IdentityUser> signInManager) : Controller
    {
        private readonly DataContext _context = context;
        private SignInManager<IdentityUser> _signInManager = signInManager;

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart");

            Order order = new Order { UserName = User.Identity.Name, GrandTotal = cart.Sum(x => x.Price * x.Quantity) };

            _context.Add(order);
            await _context.SaveChangesAsync();

            int id = order.Id;

            foreach (var item in cart)
            {
                OrderDetail orderDetail = new()
                {
                    OrderId = id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Image = item.Image
                };

                _context.Add(orderDetail);
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Index");

        }
    }
}

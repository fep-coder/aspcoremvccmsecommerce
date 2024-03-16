using CMSECommerce.Infrastructure;
using CMSECommerce.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMSECommerce.Controllers
{
    public class AccountController(
            DataContext dataContext,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager) : Controller
    {
        private DataContext _context = dataContext;
        private UserManager<IdentityUser> _userManager = userManager;
        private SignInManager<IdentityUser> _signInManager = signInManager;

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var username = User.Identity.Name;

            List<Order> orders = await _context.Orders.OrderByDescending(x => x.Id).Where(x => x.UserName == username).ToListAsync();

            return View(orders);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                IdentityUser newUser = new() { UserName = user.UserName, Email = user.Email };

                IdentityResult result = await _userManager.CreateAsync(newUser, user.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, "Customer");

                    TempData["success"] = "You have registered successfully!";

                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(user);
        }

        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.UserName, loginVM.Password, false, false);

                if (result.Succeeded)
                {
                    return Redirect(loginVM.ReturnUrl ?? "/");
                }

                ModelState.AddModelError("", "Invalid username or password");
            }

            return View(loginVM);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager?.SignOutAsync();

            return Redirect("/");
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            Order order = await _context.Orders.Where(x => x.Id == id).FirstOrDefaultAsync();

            List<OrderDetail> orderDetails = await _context.OrderDetails.Where(x => x.OrderId == id).ToListAsync();

            return View(new OrderDetailsViewModel { Order = order, OrderDetails = orderDetails });
        }
    }
}

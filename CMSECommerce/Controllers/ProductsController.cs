using CMSECommerce.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMSECommerce.Controllers
{
    public class ProductsController(
                            DataContext context,
                            IWebHostEnvironment webHostEnvironment) : Controller
    {
        private readonly DataContext _context = context;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

        public async Task<IActionResult> Index(string slug = "", int p = 1)
        {
            ViewBag.CategorySlug = slug;
            int pageSize = 3;
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;

            if (slug == "")
            {
                ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Products.Count() / pageSize);

                List<Product> products =
                            await _context.Products
                            .Include(x => x.Category)
                            .OrderByDescending(x => x.Id)
                            .Skip((p - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

                return View(products);
            }

            Category category = await _context.Categories.Where(x => x.Slug == slug).FirstOrDefaultAsync();

            if (category == null) return RedirectToAction("Index");

            var productsByCategory = _context.Products.Where(x => x.CategoryId == category.Id);


            ViewBag.TotalPages = (int)Math.Ceiling((decimal)productsByCategory.Count() / pageSize);

            return View(await productsByCategory
                                .Include(x => x.Category)
                                .OrderByDescending(x => x.Id)
                                .Skip((p - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync());

        }

        public async Task<IActionResult> Product(string slug = "")
        {
            Product product = await _context.Products
                                    .Where(x => x.Slug == slug)
                                    .FirstOrDefaultAsync();

            if (product == null) return RedirectToAction("Index");

            string galleryDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/gallery/" + product.Id.ToString());

            if (Directory.Exists(galleryDir))
            {
                product.GalleryImages = Directory.EnumerateFiles(galleryDir).Select(x => Path.GetFileName(x));
            }

            return View(product);
        }
    }


}
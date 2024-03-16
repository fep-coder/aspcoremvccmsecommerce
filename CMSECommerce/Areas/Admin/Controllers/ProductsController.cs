using CMSECommerce.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMSECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize("Admin")]
    public class ProductsController(
                    DataContext context,
                    IWebHostEnvironment webHostEnvironment) : Controller
    {
        private readonly DataContext _context = context;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

        public async Task<IActionResult> Index(int categoryId = 0, int p = 1)
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", categoryId.ToString());

            ViewBag.SelectedCategory = categoryId.ToString();
            int pageSize = 3;
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;

            Category category = await _context.Categories
                                .Where(x => x.Id == categoryId)
                                .FirstOrDefaultAsync();

            if (category == null)
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

            var productsByCategory = _context.Products.Where(x => x.CategoryId == categoryId);

            ViewBag.TotalPages = (int)Math.Ceiling((decimal)productsByCategory.Count() / pageSize);

            return View(await productsByCategory
                                .Include(x => x.Category)
                                .OrderByDescending(x => x.Id)
                                .Skip((p - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync());

        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.ToLower().Replace(" ", "-");

                var slug = await _context.Products.FirstOrDefaultAsync(x => x.Slug == product.Slug);

                if (slug != null)
                {
                    ModelState.AddModelError("", "That product already exists!");
                    return View(product);
                }

                string imageName;

                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");

                    imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;

                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);

                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                }
                else
                {
                    imageName = "noimage.png";
                }

                product.Image = imageName;

                _context.Add(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "The product has been added!";

                return RedirectToAction("Index");
            }

            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            Product product = await _context.Products.FindAsync(id);

            if (product == null) { return NotFound(); }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/gallery/" + id.ToString());

            if (Directory.Exists(uploadsDir))
            {
                product.GalleryImages = Directory.EnumerateFiles(uploadsDir).Select(x => Path.GetFileName(x));
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.ToLower().Replace(" ", "-");

                var slug = await _context.Products.Where(x => x.Id != product.Id).FirstOrDefaultAsync(x => x.Slug == product.Slug);

                if (slug != null)
                {
                    ModelState.AddModelError("", "That product already exists!");
                    return View(product);
                }

                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");

                    if (!string.Equals(product.Image, "noimage.png"))
                    {
                        string oldImagePath = Path.Combine(
                                        uploadsDir, product.Image
                                    );
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;

                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new(filePath, FileMode.Create);

                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();

                    product.Image = imageName;
                }

                _context.Update(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "The product has been added!";

                return RedirectToAction("Edit", new { product.Id });
            }

            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            Product product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                TempData["Error"] = "The product does not exist!";
            }
            else
            {
                if (!string.Equals(product.Image, "noimage.png"))
                {
                    string productImage = Path.Combine(_webHostEnvironment.WebRootPath, "media/products/" + product.Image);

                    if (System.IO.File.Exists(productImage))
                    {
                        System.IO.File.Delete(productImage);
                    }
                }

                string gallery = Path.Combine(_webHostEnvironment.WebRootPath, "media/gallery/" + product.Id.ToString());

                if (Directory.Exists(gallery))
                {
                    Directory.Delete(gallery, true);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "The product has been deleted!";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadImages(int id)
        {
            var files = HttpContext.Request.Form.Files;

            if (files.Any())
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/gallery/" + id.ToString());

                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                foreach (var file in files)
                {
                    string imageName = Guid.NewGuid().ToString() + "_" + file.FileName;

                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new(filePath, FileMode.Create);

                    await file.CopyToAsync(fs);
                    fs.Close();
                }

                return Ok();
            }

            return View();
        }

        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "media/gallery/" + id.ToString() + "/" + imageName);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
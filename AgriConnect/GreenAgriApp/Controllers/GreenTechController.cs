using Microsoft.AspNetCore.Mvc;
using GreenAgriApp.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace GreenAgriApp.Controllers
{
    public class GreenTechController : Controller
    {
        private readonly GreenAgriDbContext _db;

        public GreenTechController(GreenAgriDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            //check session and redirect if not logged in
            var name = HttpContext.Session.GetString("Name");
            if (string.IsNullOrEmpty(name)) return RedirectToAction("Login", "Account");

            ViewBag.UserName = name;
            return View();
        }

        [HttpGet]
        public IActionResult RequestCategory()
        {
            //show request category form
            return View();
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            //load approved greentech categories
            ViewBag.Categories = _db.Categories.Where(c => c.Status == "Approved" && c.RequestorRole == "GreenTech").ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(string name, string description, int quantity, int categoryId, IFormFile imageFile)
        {
            //check session
            var userId = HttpContext.Session.GetString("UID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            //convert image to byte array
            byte[] imageData = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await imageFile.CopyToAsync(ms);
                    imageData = ms.ToArray();
                }
            }

            //create and save product
            var product = new GreenTechProduct
            {
                UserId = userId,
                UserName = HttpContext.Session.GetString("Name"),
                Name = name,
                Description = description,
                Quantity = quantity,
                CategoryId = categoryId,
                Image = imageData,
                DatePosted = DateTime.Now
            };

            _db.GreenTechProducts.Add(product);
            await _db.SaveChangesAsync();

            TempData["Success"] = "GreenTech product added successfully!";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult MyProducts()
        {
            //get current user's products
            var userId = HttpContext.Session.GetString("UID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var products = _db.GreenTechProducts
                            .Where(p => p.UserId == userId)
                            .OrderByDescending(p => p.DatePosted)
                            .ToList();

     ViewBag.Categories = _db.Categories.Where(c => c.Status == "Approved" && c.RequestorRole == "GreenTech").ToList();


            return View(products);
        }

        [HttpGet]
        public IActionResult ManageProduct(int id)
        {
            //validate ownership
            var userId = HttpContext.Session.GetString("UID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var product = _db.GreenTechProducts.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (product == null)
                return NotFound();

            ViewBag.Categories = _db.Categories.Where(c => c.Status == "Approved" && c.RequestorRole == "GreenTech").ToList();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(int id, string name, string description, int quantity, int categoryId, IFormFile imageFile)
        {
            //update existing product
            var userId = HttpContext.Session.GetString("UID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var product = _db.GreenTechProducts.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (product == null)
                return NotFound();

            product.Name = name;
            product.Description = description;
            product.Quantity = quantity;
            product.CategoryId = categoryId;

            if (imageFile != null && imageFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await imageFile.CopyToAsync(ms);
                    product.Image = ms.ToArray();
                }
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction("MyProducts");
        }

        [HttpGet]
        public IActionResult DeleteProduct(int id)
        {
            //delete product
            var userId = HttpContext.Session.GetString("UID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var product = _db.GreenTechProducts.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (product == null)
                return NotFound();

            _db.GreenTechProducts.Remove(product);
            _db.SaveChanges();

            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction("MyProducts");
        }

        [HttpPost]
        public IActionResult RequestCategory(string name)
        {
            //submit category request
            if (string.IsNullOrWhiteSpace(name))
            {
                ViewBag.Error = "Category name cannot be empty.";
                return View();
            }

            var category = new Category
            {
                Name = name,
                Status = "Pending",
                RequestorRole = "GreenTech"
            };

            _db.Categories.Add(category);
            _db.SaveChanges();

            TempData["Success"] = "Category requested successfully. Awaiting approval.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult CommunityBlog()
        {
            //list all blog posts
            var posts = _db.BlogPosts.OrderByDescending(p => p.DatePosted).ToList();
            return View(posts);
        }

        [HttpPost]
        public async Task<IActionResult> PostToBlog(string message, IFormFile imageFile)
        {
            //add new blog post
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction("CommunityBlog");
            }

            string imagePath = null;

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, imageFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                imagePath = "/uploads/" + imageFile.FileName;
            }

            var post = new BlogPost
            {
                UserId = HttpContext.Session.GetString("UID"),
                UserName = HttpContext.Session.GetString("Name"),
                Message = message,
                ImagePath = imagePath,
                DatePosted = DateTime.Now,
                IsViolation = false
            };

            _db.BlogPosts.Add(post);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Post added successfully.";
            return RedirectToAction("CommunityBlog");
        }

        [HttpGet]
        public IActionResult EducationalResources()
        {
            //show all educational resources
            var resources = _db.Resources.OrderByDescending(r => r.UploadDate).ToList();
            return View(resources);
        }
    }
}

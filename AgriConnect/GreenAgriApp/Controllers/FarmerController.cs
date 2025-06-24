using Microsoft.AspNetCore.Mvc;
using GreenAgriApp.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GreenAgriApp.Services;

namespace GreenAgriApp.Controllers
{
    public class FarmerController : Controller
    {
        private readonly GreenAgriDbContext _db;
        private readonly MailService _mailService;

        public FarmerController(GreenAgriDbContext db, MailService mailService)
        {
            _db = db;
            _mailService = mailService;
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            //get approved categories for dropdown
            ViewBag.Categories = _db.Categories.Where(c => c.Status == "Approved" && c.RequestorRole == "Farmer").ToList();
            return View();
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            //redirect to login if not authenticated
            var name = HttpContext.Session.GetString("Name");
            if (string.IsNullOrEmpty(name))
                return RedirectToAction("Login", "Account");

            ViewBag.FarmerName = name;
            return View();
        }

        [HttpGet]
        public IActionResult MyProducts()
        {
            //get current user's products
            var userId = HttpContext.Session.GetString("UID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var products = _db.Products
                            .Where(p => p.UserId == userId)
                            .OrderByDescending(p => p.DatePosted)
                            .ToList();
            ViewBag.Categories = _db.Categories.Where(c => c.Status == "Approved" && c.RequestorRole == "Farmer").ToList();

            return View(products);
        }

        [HttpGet]
        public IActionResult ManageProduct(int id)
        {
            //check user owns this product
            var userId = HttpContext.Session.GetString("UID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var product = _db.Products.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (product == null)
                return NotFound();

            ViewBag.Categories = _db.Categories.Where(c => c.RequestorRole == "Farmer").ToList();
            return View(product);
        }

        [HttpGet]
        public IActionResult RequestCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(string name, string description, int quantity, int categoryId, IFormFile imageFile)
        {
            //upload new product
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UID")))
                return RedirectToAction("Login", "Account");

            byte[] imageData = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await imageFile.CopyToAsync(ms);
                    imageData = ms.ToArray();
                }
            }

            var product = new Product
            {
                UserId = HttpContext.Session.GetString("UID"),
                UserName = HttpContext.Session.GetString("Name"),
                Name = name,
                Description = description,
                Quantity = quantity,
                CategoryId = categoryId,
                Image = imageData,
                DatePosted = DateTime.Now
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Product added successfully!";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> RequestCategory(string name)
        {
            //submit new category request
            if (string.IsNullOrWhiteSpace(name))
            {
                ViewBag.Error = "Category name is required.";
                return View();
            }

            var existing = _db.Categories.FirstOrDefault(c => c.Name == name);
            if (existing != null)
            {
                ViewBag.Error = "Category already exists or is pending approval.";
                return View();
            }

            var category = new Category
            {
                Name = name,
                Status = "Pending",
                RequestorRole = "Farmer"
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Category request submitted for approval.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(int id, string name, string description, int quantity, int categoryId, IFormFile imageFile)
        {
            //update existing product
            var userId = HttpContext.Session.GetString("UID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var product = _db.Products.FirstOrDefault(p => p.Id == id && p.UserId == userId);
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
            //delete user product
            var userId = HttpContext.Session.GetString("UID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var product = _db.Products.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (product == null)
                return NotFound();

            _db.Products.Remove(product);
            _db.SaveChanges();

            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction("MyProducts");
        }

        [HttpGet]
        public IActionResult CommunityBlog()
        {
            //show all posts
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
            //show all resources
            var resources = _db.Resources.OrderByDescending(r => r.UploadDate).ToList();
            return View(resources);
        }

        [HttpGet]
        public IActionResult GreenEnergyMarketplace()
        {
            //list greentech products
            var products = _db.GreenTechProducts
                            .Include(p => p.Category)
                            .OrderByDescending(p => p.DatePosted)
                            .ToList();

            return View(products);
        }

        [HttpGet]
        public IActionResult SendEnquiry(int id)
        {
            //prepare enquiry form
            var product = _db.GreenTechProducts.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            ViewBag.Product = product;
            return View();
        }

        [HttpPost]
        public IActionResult SendEnquiry(int productId, string fullName, string contactNumber, string email, string message)
        {
            //send enquiry email
            var product = _db.GreenTechProducts.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return NotFound();

            var greenTechUser = _db.Users.FirstOrDefault(u => u.Id == product.UserId);
            if (greenTechUser == null)
                return NotFound();

            var body = $@"
                <h2>New Enquiry for {product.Name}</h2>
                <p><strong>From:</strong> {fullName}</p>
                <p><strong>Contact Number:</strong> {contactNumber}</p>
                <p><strong>Email:</strong> {email}</p>
                <p><strong>Message:</strong> {message}</p>
            ";

            var success = _mailService.SendMail(greenTechUser.Email, $"Enquiry for {product.Name}", body);

            if (success)
                TempData["Success"] = "Your enquiry was sent successfully.";
            else
                TempData["Error"] = "Failed to send enquiry. Please try again later.";

            return RedirectToAction("GreenEnergyMarketplace");
        }
    }
}

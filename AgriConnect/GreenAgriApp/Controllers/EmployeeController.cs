using Microsoft.AspNetCore.Mvc;
using GreenAgriApp.Models;
using GreenAgriApp.Models.ViewModels;
using GreenAgriApp.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenAgriApp.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly FirebaseService _firebase;
        private readonly GreenAgriDbContext _db;

        public EmployeeController(FirebaseService firebase, GreenAgriDbContext db)
        {
            _firebase = firebase;
            _db = db;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            var name = HttpContext.Session.GetString("Name");
            if (string.IsNullOrEmpty(name)) return RedirectToAction("Login", "Account"); //redirect if not logged in
            ViewBag.EmployeeName = name;
            return View(); //show employee dashboard
        }

        [HttpGet]
        public IActionResult AddFarmer()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized(); //only employees allowed

            return View(); //show add farmer form
        }

        [HttpGet]
        public IActionResult AddGreenTech()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            return View(); //show add greentech form
        }

        [HttpGet]
        public IActionResult UploadResource()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            return View(); //show upload resource form
        }

        [HttpGet]
        public IActionResult MonitorBlog()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var posts = _db.BlogPosts.OrderByDescending(p => p.DatePosted).ToList(); //get all posts
            return View(posts); //show blog moderation view
        }

        [HttpGet]
        public IActionResult ManageCategories()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var categories = _db.Categories.OrderBy(c => c.Status).ToList(); //list all categories
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddFarmer(RegisterUserViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            if (!ModelState.IsValid)
                return View(model); //return if form is invalid

            var existingUser = _db.Users.FirstOrDefault(u => u.Email == model.Email && u.IsActive); //check if email already exists
            if (existingUser != null)
            {
                ViewBag.Error = "This email is already registered and active.";
                return View(model);
            }

            try
            {
                var tempPassword = Guid.NewGuid().ToString("N").Substring(0, 12) + "!"; //generate temp password
                var auth = await _firebase.RegisterAsync(model.Email, tempPassword); //register firebase user
                var uid = auth.User.LocalId;

                var emailSent = await _firebase.SendPasswordResetEmailAsync(model.Email); //send reset email
                if (!emailSent)
                {
                    ViewBag.Error = "User created, but failed to send password setup email.";
                    return View(model);
                }

                var user = new User
                {
                    Id = uid,
                    FullName = model.FullName,
                    Email = model.Email,
                    Role = "Farmer",
                    DateRegistered = DateTime.Now,
                    IsActive = true
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync(); //save to db

                TempData["Success"] = $"Farmer {model.FullName} added successfully! A secure password setup email has been sent to {model.Email}.";
                return RedirectToAction("Dashboard");
            }
            catch (ApplicationException ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddGreenTech(RegisterUserViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            if (!ModelState.IsValid)
                return View(model);

            var existingUser = _db.Users.FirstOrDefault(u => u.Email == model.Email && u.IsActive);
            if (existingUser != null)
            {
                ViewBag.Error = "This email is already registered and active.";
                return View(model);
            }

            try
            {
                var tempPassword = Guid.NewGuid().ToString("N").Substring(0, 12) + "!";
                var auth = await _firebase.RegisterAsync(model.Email, tempPassword);
                var uid = auth.User.LocalId;

                var emailSent = await _firebase.SendPasswordResetEmailAsync(model.Email);
                if (!emailSent)
                {
                    ViewBag.Error = "User created, but failed to send password setup email.";
                    return View(model);
                }

                var user = new User
                {
                    Id = uid,
                    FullName = model.FullName,
                    Email = model.Email,
                    Role = "GreenTech",
                    DateRegistered = DateTime.Now,
                    IsActive = true
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                TempData["Success"] = $"GreenTech user {model.FullName} added successfully! A secure password setup email has been sent to {model.Email}.";
                return RedirectToAction("Dashboard");
            }
            catch (ApplicationException ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ViewUserProducts(string id)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var user = _db.Users.FirstOrDefault(u => u.Id == id); //get user by id
            if (user == null) return NotFound();

            List<Product> products;

            if (user.Role == "Farmer")
            {
                products = _db.Products.Include(p => p.Category).Where(p => p.UserId == id).ToList();
            }
            else if (user.Role == "GreenTech")
            {
                products = _db.GreenTechProducts.Include(p => p.Category)
                    .Where(p => p.UserId == id)
                    .Select(gt => new Product
                    {
                        Id = gt.Id,
                        Name = gt.Name,
                        Description = gt.Description,
                        Quantity = gt.Quantity,
                        DatePosted = gt.DatePosted,
                        Image = gt.Image,
                        Category = gt.Category,
                        UserName = user.FullName
                    }).ToList();
            }
            else
            {
                products = new List<Product>();
            }

            ViewBag.UserName = user.FullName;
            return View("UserProductsGrouped", products); //render grouped view
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var users = _db.Users.Where(u => u.Role == "Farmer" || u.Role == "GreenTech")
                                 .OrderBy(u => u.FullName).ToList();
            return View(users);
        }

        [HttpGet]
        public IActionResult ViewResource(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var resource = _db.Resources.FirstOrDefault(r => r.Id == id);
            if (resource == null) return NotFound();

            return View(resource);
        }

        [HttpGet]
        public IActionResult ManageResources()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var resources = _db.Resources.OrderByDescending(r => r.UploadDate).ToList();
            return View(resources);
        }

        [HttpGet]
        public IActionResult AddBlogPost()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            return View();
        }

        [HttpGet]
        public IActionResult ViewUser(string id)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult UpdateUser(User model)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var user = _db.Users.FirstOrDefault(u => u.Id == model.Id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            _db.SaveChanges();

            TempData["Success"] = $"User {user.FullName}'s details have been updated successfully.";
            return RedirectToAction("ViewUser", new { id = user.Id });
        }

        [HttpPost]
        public async Task<IActionResult> SendPasswordReset(string email)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var emailSent = await _firebase.SendPasswordResetEmailAsync(email);
            TempData["Success"] = emailSent 
                ? $"Password reset link sent to {email}."
                : $"Failed to send password reset link to {email}.";

            return RedirectToAction("ListUsers");
        }

        [HttpGet]
        public IActionResult DeleteUser(string id)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _db.Users.Remove(user);
                _db.SaveChanges();
                TempData["Success"] = $"User {user.FullName} deleted successfully.";
            }

            return RedirectToAction("ListUsers");
        }

        [HttpPost]
        public async Task<IActionResult> UploadResource(Resource model, IFormFile file)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Please select a file to upload.";
                return View(model);
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder); //create folder if it doesn't exist
            }

            var filePath = Path.Combine(uploadsFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream); //save file to server
            }

            model.FilePath = "/uploads/" + file.FileName;
            model.UploadDate = DateTime.Now;

            _db.Resources.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Resource uploaded successfully!";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult DeleteResource(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var resource = _db.Resources.FirstOrDefault(r => r.Id == id);
            if (resource == null)
                return NotFound();

            var physicalFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", resource.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalFilePath))
            {
                System.IO.File.Delete(physicalFilePath); //delete file from server
            }

            _db.Resources.Remove(resource);
            _db.SaveChanges();

            TempData["Success"] = "Resource deleted successfully.";
            return RedirectToAction("ManageResources");
        }

        [HttpPost]
        public IActionResult MarkPostAsViolation(int postId)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var post = _db.BlogPosts.FirstOrDefault(p => p.Id == postId);
            if (post == null)
                return NotFound();

            post.IsViolation = true;
            post.ViolationNote = "This post was removed due to community guideline violation.";
            _db.SaveChanges();

            TempData["Success"] = "Post marked as violation successfully.";
            return RedirectToAction("MonitorBlog");
        }

        [HttpPost]
        public async Task<IActionResult> AddBlogPost(string message, IFormFile imageFile)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

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
            return RedirectToAction("MonitorBlog");
        }

        [HttpPost]
        public IActionResult RestorePost(int postId)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var post = _db.BlogPosts.FirstOrDefault(p => p.Id == postId);
            if (post == null)
                return NotFound();

            post.IsViolation = false;
            post.ViolationNote = null;
            _db.SaveChanges();

            TempData["Success"] = "Post was restored successfully.";
            return RedirectToAction("MonitorBlog");
        }

        [HttpPost]
        public IActionResult UpdateCategoryStatus([FromBody] Category model)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var category = _db.Categories.FirstOrDefault(c => c.Id == model.Id);
            if (category == null)
                return NotFound();

            category.Status = model.Status;
            _db.SaveChanges();

            return Content("Status updated successfully.");
        }

        [HttpGet]
        public IActionResult ViewAllFarmerProducts()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var products = _db.Products.Include(p => p.Category)
                                       .OrderBy(p => p.Category.Name)
                                       .ToList();
            return View(products);
        }

        [HttpGet]
        public IActionResult ManageFarmerCategories()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var categories = _db.Categories.Where(c => c.RequestorRole == "Farmer")
                                           .OrderBy(c => c.Status)
                                           .ToList();

            ViewBag.Title = "Manage Farmer Categories";
            return View("ManageCategories", categories);
        }

        [HttpGet]
        public IActionResult ManageGreenTechCategories()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var categories = _db.Categories.Where(c => c.RequestorRole == "GreenTech")
                                           .OrderBy(c => c.Status)
                                           .ToList();

            ViewBag.Title = "Manage GreenTech Categories";
            return View("ManageCategories", categories);
        }

        [HttpGet]
        public IActionResult DeleteCategory(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var category = _db.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
                return NotFound();

            _db.Categories.Remove(category);
            _db.SaveChanges();

            TempData["Success"] = $"Category '{category.Name}' deleted.";
            return RedirectToAction("ManageFarmerCategories");
        }

        [HttpPost]
        public IActionResult UpdateResource(Resource model)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            var resource = _db.Resources.FirstOrDefault(r => r.Id == model.Id);
            if (resource == null)
                return NotFound();

            resource.Title = model.Title;
            resource.Description = model.Description;
            _db.SaveChanges();

            TempData["Success"] = $"Resource '{resource.Title}' updated successfully.";
            return RedirectToAction("ViewResource", new { id = resource.Id });
        }
    }
}

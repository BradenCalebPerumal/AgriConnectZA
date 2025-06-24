using Microsoft.AspNetCore.Mvc;
using GreenAgriApp.Models.ViewModels;
using GreenAgriApp.Services;
using GreenAgriApp.Models; 
using Microsoft.EntityFrameworkCore;

namespace GreenAgriApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly FirebaseService _firebase;
        private readonly GreenAgriDbContext _db;

        public AccountController(FirebaseService firebase, GreenAgriDbContext db)
        {
            _firebase = firebase;
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(); //show login view
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model); //return if input is invalid

            var auth = await _firebase.SignInAsync(model.Email, model.Password); //sign in via firebase
            if (auth == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View(model); //invalid firebase credentials
            }

            var uid = auth.User.LocalId; //get firebase user id
            var user = await _db.Users.FindAsync(uid); //fetch user from db

            if (user == null)
            {
                ViewBag.Error = "Account not found.";
                return View(model); //user not found in db
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Your account has been deactivated. Contact the administrator.";
                return View(model); //account is inactive
            }

            //save session data
            HttpContext.Session.SetString("UID", uid);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Name", user.FullName);
            HttpContext.Session.SetString("IsActive", user.IsActive.ToString().ToLower());
            TempData["Success"] = $"Welcome back, {user.FullName}!";

            //role-based redirection
            switch (user.Role)
            {
                case "Employee":
                    return RedirectToAction("Dashboard", "Employee"); //go to employee dashboard
                case "Farmer":
                    return RedirectToAction("Dashboard", "Farmer"); //go to farmer dashboard
                case "GreenTech":
                    return RedirectToAction("Dashboard", "GreenTech"); //go to greentech dashboard
                default:
                    ViewBag.Error = "Role not recognized.";
                    return View(model); //invalid role
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); //remove all session values
            return RedirectToAction("Login", "Account"); //redirect to login
        }
[HttpGet]
public IActionResult ForgotPassword()
{
    return View();
}

[HttpPost]
public async Task<IActionResult> ForgotPassword(string email)
{
    if (string.IsNullOrWhiteSpace(email))
    {
        TempData["Error"] = "Email is required.";
        return RedirectToAction("ForgotPassword");
    }

    var success = await _firebase.SendPasswordResetEmailAsync(email);

    if (success)
        TempData["Success"] = "Password reset link sent. Please check your email.";
    else
        TempData["Error"] = "Failed to send reset link. Please check your email address and try again.";

    return RedirectToAction("ForgotPassword");
}

    }
}

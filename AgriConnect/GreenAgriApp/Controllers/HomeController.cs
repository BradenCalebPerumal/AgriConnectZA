using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GreenAgriApp.Models;
using GreenAgriApp.Services;

namespace GreenAgriApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly GreenAgriDbContext _db;
    private readonly MailService _mailService;

    public HomeController(GreenAgriDbContext db, ILogger<HomeController> logger, MailService mailService)
    {
        _db = db;
        _logger = logger;
        _mailService = mailService;
    }

    //show home page
    public IActionResult Index()
    {
        return View();
    }

    //show terms of use page
    public IActionResult TermsOfUse()
    {
        return View();
    }

    //show privacy policy page
    public IActionResult Privacy()
    {
        return View();
    }

    //show public enquiry form
    [HttpGet]
    public IActionResult PublicEnquiry()
    {
        return View();
    }

    //show error page
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    //handle public enquiry form submission
    [HttpPost]
    public IActionResult PublicEnquiry(string name, string contactNumber, string email, string roleInterest, string message)
    {
        var body = $@"
            <h2>New Enquiry</h2>
            <p><strong>Name:</strong> {name}</p>
            <p><strong>Contact Number:</strong> {contactNumber}</p>
            <p><strong>Email:</strong> {email}</p>
            <p><strong>Interested In:</strong> {roleInterest}</p>
            <p><strong>Message:</strong> {message}</p>
        ";

        var success = _mailService.SendMail("agriconnectza@gmail.com", "New Enquiry", body);

        ViewBag.Message = success ? "Your enquiry has been sent successfully." : "Failed to send enquiry. Please try again later.";
        return View();
    }

    //show contact us page
    [HttpGet]
    public IActionResult Contact()
    {
        return View();
    }

    //show about page
    [HttpGet]
    public IActionResult About()
    {
        return View();
    }

    //show educational resources to the public
    [HttpGet]
    public IActionResult EduResource()
    {
        var resources = _db.Resources.OrderByDescending(r => r.UploadDate).ToList();
        return View(resources);
    }

    //handle contact form submission
    [HttpPost]
    public IActionResult Contact(string name, string contactNumber, string email, string message)
    {
        var body = $@"
            <h2>New Contact Enquiry</h2>
            <p><strong>Name:</strong> {name}</p>
            <p><strong>Contact Number:</strong> {contactNumber}</p>
            <p><strong>Email:</strong> {email}</p>
            <p><strong>Message:</strong> {message}</p>
        ";

        var success = _mailService.SendMail("agriconnectza@gmail.com", "New Contact Enquiry Received", body);

        ViewBag.Message = success
            ? "Your message has been sent successfully. We will get back to you shortly."
            : "Failed to send your message. Please try again later.";

        return View();
    }
}

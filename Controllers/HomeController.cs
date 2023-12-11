using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailService;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IEmailSender emailService, IConfiguration configuration)
        {
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
        }
        [HttpGet]
        public IActionResult Landing(string? swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;
            return View(); 
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult AdmintoIndex()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Bug404()
        {
            return View();
        }

        public IActionResult Bug500()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactMe(string? name, string? email, string? comments)
        {
            string? swalMessage = string.Empty;

            if (ModelState.IsValid)
            {
                try
                {
                    string? contactMeEmail = _configuration["ContactMeEmail"] ?? Environment.GetEnvironmentVariable("ContactMeEmail");
                    await _emailService.SendEmailAsync(contactMeEmail!, $"TrackerBarrel Contact From {name}- {email}", comments!);
                    swalMessage = "Email sent successfully!";
                }
                catch (Exception)
                {

                    throw;
                }

            } else
            {

                swalMessage = "Error: Unable to send email.";
            }
            return RedirectToAction("Landing", new { swalMessage });
        }
    }
}
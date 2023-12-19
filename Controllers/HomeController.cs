using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Services;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


namespace BugTracker.Controllers
{
    public class HomeController : BTBaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailService;
        private readonly IConfiguration _configuration;
        private readonly IBTRolesService _rolesService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTNotificationService _notificationService;

        public HomeController(ILogger<HomeController> logger, IEmailSender emailService, IConfiguration configuration, IBTRolesService rolesService, UserManager<BTUser> userManager, IBTNotificationService notificationService)
        {
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
            _rolesService = rolesService;
            _userManager = userManager;
            _notificationService = notificationService;
        }
        [HttpGet]
        public IActionResult Landing(string? swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;
            return View(); 
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            List<BTUser> admins = await _rolesService.GetUsersInRoleAsync("Admin", _companyId);
            List<BTUser> pMs = await _rolesService.GetUsersInRoleAsync("ProjectManager", _companyId);
            List<BTUser> devs = await _rolesService.GetUsersInRoleAsync("Developer", _companyId);
            List<BTUser> subs = await _rolesService.GetUsersInRoleAsync("Submitter", _companyId);

            ViewBag.Admins = admins;
            ViewBag.PMs = pMs;
            ViewBag.Devs = devs;
            ViewBag.Subs = subs;

            BTUser? bTUser = await _userManager.GetUserAsync(User);
            List<Notification> notifications = await _notificationService.GetNotificationsByUserIdAsync(bTUser?.Id);
            ViewBag.Notifications = notifications;

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
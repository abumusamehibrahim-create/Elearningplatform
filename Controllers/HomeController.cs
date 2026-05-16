using ELearningPlatform.Data;
using ELearningPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace ELearningPlatform.Controllers
{
    public class HomeController : BaseController
    {
        //private readonly ApplicationDbContext _db;

        private readonly UserManager<ApplicationUser> _userManager;


        public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager) : base(db)
        {
            // _db = db;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            try { 
            var courses = await _context.Courses
                .Where(c => c.IsActive)
                .Include(c => c.Videos)
                .ToListAsync();
            return View(courses);
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }
               public IActionResult LicenseInfo()
        {
            var license = _context.Licenses.FirstOrDefault();
            if (license == null)
                return View("NoLicense");

            var daysLeft = (license.ExpirationDate - DateTime.UtcNow).Days;

            var model = new LicenseDashboardViewModel
            {
                ClientName = license.ClientName,
                Domain = license.Domain,
                LicenseKey = license.LicenseKey,
                ExpirationDate = license.ExpirationDate,
                IsActive = license.IsActive,
                DaysLeft = daysLeft,
                Status = license.IsActive
                    ? (license.ExpirationDate < DateTime.UtcNow ? "Expired" :
                       daysLeft <= 30 ? "ExpiringSoon" : "Active")
                    : "Disabled"
            };

            return View(model);
        }


        //================================================================
        public async Task<IActionResult> CourseDetails(int id)
        {
            try
            {
                var course = await _context.Courses
                .Include(c => c.Videos)
                .FirstOrDefaultAsync(c => c.Id == id);

                if (course == null)
                    return NotFound();

                var user = await _userManager.GetUserAsync(User);

                bool isEnrolled = false;
                bool isPaid = false;

                if (user != null)
                {
                    // Check if user is enrolled
                    isEnrolled = _context.UserCourses.Any(x => x.UserId == user.Id && x.CourseId == id);

                    // Check if user paid for THIS course
                    var lastPayment = _context.Payments
         .Where(p => p.UserId == user.Id && p.CourseId == id)
         .OrderByDescending(p => p.Id)
         .FirstOrDefault();

                    isPaid = lastPayment != null && lastPayment.Status == "Approved";

                }

                ViewBag.IsEnrolled = isEnrolled;
                ViewBag.IsPaid = isPaid;

                return View(course);
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }
        public IActionResult WorksheetView(int videoId)
        {
           try{ var video = _context.Videos
                .Where(v => v.Id == videoId)
                .Select(v => new WorksheetViewModel
                {
                    VideoId = v.Id,
                    Title = v.Title,
                    WorksheetFiles = v.WorksheetFiles,
                    WorksheetItems = v.WorksheetItems
                })
                .FirstOrDefault();

            if (video == null)
                return NotFound();

            return View(video);
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }
    }
}


        


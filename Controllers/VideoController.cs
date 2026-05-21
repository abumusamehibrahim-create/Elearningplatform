 using ELearningPlatform.Data;
using ELearningPlatform.Models;
using ELearningPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Crmf;
using Stripe;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ELearningPlatform.Controllers
{
    [Authorize]
    public class VideoController : BaseController
    {
        // private readonly ApplicationDbContext _db;
        private readonly ILogger<VideoController> _logger;
        private readonly AzureVideoManager _videoManager;

        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly VideoAccessService _accessService;

        public VideoController(ApplicationDbContext db,
            IWebHostEnvironment env, IConfiguration config,
            UserManager<ApplicationUser> userManager,
            VideoAccessService accessService) : base(db)
        {
            // _db = db;
            _env = env;
            _userManager = userManager;
            _accessService = accessService;
            _videoManager = new AzureVideoManager(config);

        }

        public async Task<IActionResult> Index(int courseId)
        {
            try{var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var course = await _context.Courses
                .Include(c => c.Videos.OrderBy(v => v.OrderNumber))
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return NotFound();

            bool hasAccess = _accessService.UserHasAccess(user.Id, courseId);
            if (!hasAccess)
                return RedirectToAction("Checkout", "Payment", new { courseId });

            ViewBag.Course = course;
            return View(course.Videos.ToList());
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }

        //=========================================================
        //===================================================================SafeReadFile
        //correct fix: detect the MIME type based on file extension
        private string GetVideoMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();

            return ext switch
            {
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".mkv" => "video/x-matroska",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                ".wmv" => "video/x-ms-wmv",
                ".flv" => "video/x-flv",
                ".m4v" => "video/x-m4v",
                ".3gp" => "video/3gpp",
                ".ts" => "video/mp2t",
                _ => "application/octet-stream"
            };
        }
        //Add a safe file‑read helper (protects video, photo, PDF, anything)
        private byte[] SafeReadFile(string path)
        {
            try
            {
                if (!System.IO.File.Exists(path))
                    return null;

                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }//===========================================================================================


        //==================================================================================

        public async Task<IActionResult> Watch(int videoId)
        {
            try { 
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var video = await _context.Videos
                .Include(v => v.Course)
                .FirstOrDefaultAsync(v => v.Id == videoId);

            if (video == null) return NotFound();

            //  if (!video.IsFree && !_accessService.UserHasAccess(user.Id, video.CourseId))
            //return RedirectToAction("Checkout", "Payment", new { courseId = video.CourseId });

            return View(video);
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }

        }//==========================================azurestreaming
        [HttpGet("Video/Stream")]
        public async Task<IActionResult> StreamAzure(int videoId)
        {
            var video = await _context.Videos.FindAsync(videoId);
            if (video == null)
                return NotFound();

            var stream = await _videoManager.StreamVideoAsync(video.FileName);

            return File(stream, "video/mp4", enableRangeProcessing: true);
        }


        //=========================================================================================

       
    }
}


    



    
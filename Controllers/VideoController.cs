 using ELearningPlatform.Data;
using ELearningPlatform.Models;
using ELearningPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Crmf;
using Stripe;

namespace ELearningPlatform.Controllers
{
    [Authorize]
    public class VideoController : BaseController
    {
        // private readonly ApplicationDbContext _db;
        private readonly ILogger<VideoController> _logger;

        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly VideoAccessService _accessService;

        public VideoController(ApplicationDbContext db,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            VideoAccessService accessService) : base(db)
        {
            // _db = db;
            _env = env;
            _userManager = userManager;
            _accessService = accessService;
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

        }

        //=========================================================================================
        [Authorize]
        public async Task<IActionResult> Stream(int videoId)
        {
            var video = await _context.Videos.FindAsync(videoId);
            if (video == null)
                return NotFound();

            using var client = new HttpClient();
            var stream = await client.GetStreamAsync(video.FileName);

            return File(stream, "video/mp4", enableRangeProcessing: true);
        }


        [Authorize]
        [HttpGet]
        [Route("Video/Stream")]
        public async Task<IActionResult> StreamLocal(int videoId)
        {
            try
            {
                Console.WriteLine($"\n========== STREAM DEBUG ==========");
                Console.WriteLine($"1️⃣ Requested VideoId: {videoId}");

                var user = await _userManager.GetUserAsync(User);
                Console.WriteLine($"2️⃣ User: {user?.UserName ?? "NULL"}");

                if (user == null)
                {
                    Console.WriteLine($"❌ User is null - Returning Unauthorized");
                    return Unauthorized();
                }

                var video = await _context.Videos.FindAsync(videoId);
                var videoIdx = _context.Videos.FirstOrDefault(v => v.Id == videoId);

                Console.WriteLine($"3️⃣ Video Found: {video != null}");

                if (video != null)
                {
                    Console.WriteLine($"   - Title: {video.Title}");
                    Console.WriteLine($"   - FileName: {video.FileName}");
                    Console.WriteLine($"   - IsFree: {video.IsFree}");
                }

                if (video == null)
                {
                    Console.WriteLine($"❌ Video not found in database");
                    return NotFound("الفيديو غير موجود");
                }

                // Check access
               //   if (!video.IsFree && !_accessService.UserHasAccess(user.Id, video.CourseId))
                //{
                // Console.WriteLine($"❌ No access to this video");
               //      return Forbid();
               //  }

                // Build file path
                var protectedFolder = Path.Combine(_env.ContentRootPath, "ProtectedVideos");
                var videoPath = Path.Combine(protectedFolder, video.FileName);

                Console.WriteLine($"4️⃣ Protected Folder: {protectedFolder}");
                Console.WriteLine($"5️⃣ Full Path: {videoPath}");
                Console.WriteLine($"6️⃣ File Exists: {System.IO.File.Exists(videoPath)}");

                if (!System.IO.File.Exists(videoPath))
                {
                    Console.WriteLine($"❌ FILE NOT FOUND!");

                    if (Directory.Exists(protectedFolder))
                    {
                        var allFiles = Directory.GetFiles(protectedFolder);
                        Console.WriteLine($"📁 Files in ProtectedVideos folder ({allFiles.Length} files):");
                        foreach (var file in allFiles)
                        {
                            Console.WriteLine($"   - {Path.GetFileName(file)}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"❌ ProtectedVideos FOLDER DOES NOT EXIST!");
                    }

                    Console.WriteLine($"================================\n");
                    return NotFound($"File not found: {video.FileName}");
                }

                Console.WriteLine($"✅ FILE FOUND - Opening stream");

                var stream = System.IO.File.OpenRead(videoPath);
                // var streamUrl = Url.Action("Stream", "Video", new { videoId =video.Id });

                var streamUrl = $"/Video/Stream/{videoIdx.Id}";
                Console.WriteLine($"✅ Stream opened successfully");
                Console.WriteLine($"================================\n");
                var mime = GetVideoMimeType(videoPath);
                return File(stream, mime, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EXCEPTION: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.WriteLine($"================================\n");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

       
        //            <!--<source src="@streamUrl" type="@GetMimeType(Model.FileName)">-->

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

        [AllowAnonymous]
        [HttpGet("Video/TestStream")]
        public IActionResult TestStream()
        {
            return Content("Server is working!");
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

        //🔥 Unified Stream Action (Auto Logic)
        public async Task<IActionResult> AoutStream(int id)
        {
            var video = _context.Videos.FirstOrDefault(v => v.Id == id);
            if (video == null)
                return NotFound("Video not found.");

            // 1. Try Bunny Stream first (if enabled)
            if (video.UseBunny)
            {
                var bunnyUrl = AoutBuildBunnyUrl(video);
                if (!string.IsNullOrEmpty(bunnyUrl))
                    return await StreamFromCloud(bunnyUrl);
            }

            // 2. Try Paid Cloud Server (if exists)
            var cloudUrl = BuildPaidCloudUrl(video);
            if (!string.IsNullOrEmpty(cloudUrl))
                return await StreamFromCloud(cloudUrl);

            // 3. Fallback → Local Streaming
            return await StreamLocal(video.Id);
/*
 if we want to use AoutStreaming in Watch.cshtml
            < video controls >
    < source src = "/Video/AoutStream/@Model.Id" type = "video/mp4" />
</ video >
*/


        }
        //What happens when user watches:AoutStream → detects no Bunny, no Cloud → uses StreamLocalAsync
        //===============================================================================
        private Task<IActionResult> StreamLocalAsync(Video video)
        {
            var protectedFolder = Path.Combine(Directory.GetCurrentDirectory(), "ProtectedVideos");
            var videoPath = Path.Combine(protectedFolder, video.FileName);

            if (!System.IO.File.Exists(videoPath))
                return Task.FromResult<IActionResult>(NotFound("Local video not found."));

            var stream = new FileStream(videoPath, FileMode.Open, FileAccess.Read);

            return Task.FromResult<IActionResult>(
                File(stream, "video/mp4", enableRangeProcessing: true)
            );
        }
        //===========================================================================

        //You NEVER call:StreamLocalAsync/StreamFromCloud/AoutBuildBunnyUrl/BuildPaidCloudUrl
        //=========================================================================================StreamCloud
        // -------------------------------------------------------------
        // STREAM FROM CLOUD (Bunny, AWS, DigitalOcean, Vimeo, Any CDN)
        // -------------------------------------------------------------
        public async Task<IActionResult> StreamFromCloud(int id)
        {
            // 1. Get video from database
            var video = _context.Videos.FirstOrDefault(v => v.Id == id);
            if (video == null)
                return NotFound("Video not found.");

            // 2. Determine the cloud URL based on your model
            string? cloudUrl = BuildCloudUrl(video);

            if (string.IsNullOrEmpty(cloudUrl))
                return NotFound("Cloud streaming URL is missing.");

            // 3. Create HttpClient to fetch the remote video stream
            using var httpClient = new HttpClient();

            // We request only headers first → enables streaming (not full download)
            var response = await httpClient.GetAsync(
                cloudUrl,
                HttpCompletionOption.ResponseHeadersRead
            );

            if (!response.IsSuccessStatusCode)
                return NotFound("Cloud server refused the request.");

            // 4. Get the video stream from the cloud server
            var stream = await response.Content.ReadAsStreamAsync();

            // 5. Return the stream to the browser with range support
            return File(stream, "video/mp4", enableRangeProcessing: true);

            /*StreamCloud()
This is your main controller action.

Loads video from DB

Builds the correct cloud URL

Streams it using HttpClient

Supports seeking (range requests)

Works with any cloud provider*/
        



        }

        //============================================================================================endstreamCloud
        //=================================================================================method for streamcloud help
        // -------------------------------------------------------------
        // BUILDS THE CLOUD URL BASED ON YOUR VIDEO MODEL
        // -------------------------------------------------------------
        private string? BuildCloudUrl(Video video)
        {//This method decides which cloud server to use:
            //It builds the Bunny HLS URL:
            //https://{hostname}/{libraryId}/{videoId}/play.m3u8
            //✔ If you store a direct cloud URL  Example: AWS S3 pre‑signed URL stored in Description.
            //✔ If you use your own paid server
            //https://your-paid-server.com/videos/{FileName}
            //Just change the URL builder.

            // 1. If using Bunny Stream
            if (video.UseBunny &&
                !string.IsNullOrEmpty(video.BunnyVideoId) &&
                !string.IsNullOrEmpty(video.BunnyLibraryId) &&
                !string.IsNullOrEmpty(video.BunnyCDNHostname))
            {
                // Bunny HLS streaming URL
                return $"https://{video.BunnyCDNHostname}/{video.BunnyLibraryId}/{video.BunnyVideoId}/play.m3u8";
            }

            // 2. If you store a direct cloud URL (AWS S3, DigitalOcean, Vimeo, etc.)
            if (!string.IsNullOrEmpty(video.Description) && video.Description.StartsWith("http"))
            {
                // Example: Description holds the cloud URL
                return video.Description;
            }

            // 3. If you want to support your own paid server:
            // Example: https://myserver.com/videos/{FileName}
            string paidServerBaseUrl = "https://your-paid-server.com/videos/";
            if (!string.IsNullOrEmpty(video.FileName))
            {
                return paidServerBaseUrl + video.FileName;
            }

            // No cloud URL found
            return null;

        }//================================================================================
      
        //=========================================================
       
        //==========================================================
        private string? AoutBuildBunnyUrl(Video video)
        {
            if (!video.UseBunny ||
                string.IsNullOrEmpty(video.BunnyVideoId) ||
                string.IsNullOrEmpty(video.BunnyLibraryId) ||
                string.IsNullOrEmpty(video.BunnyCDNHostname))
                return null;

            return $"https://{video.BunnyCDNHostname}/{video.BunnyLibraryId}/{video.BunnyVideoId}/play.m3u8";
        }//========================================
        private string? BuildPaidCloudUrl(Video video)
        {
            // Example: You store the cloud URL in Description
            if (!string.IsNullOrEmpty(video.Description) && video.Description.StartsWith("http"))
                return video.Description;

            // Example: Your own paid server
            string paidServerBaseUrl = "https://your-paid-server.com/videos/";
            if (!string.IsNullOrEmpty(video.FileName))
                return paidServerBaseUrl + video.FileName;

            return null;
        }
        //🧩 Helper 3: Cloud Streaming Proxy
        //This streams from ANY cloud server while protecting the real URL.
        private async Task<IActionResult> StreamFromCloud(string url)
        {
            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
                return NotFound("Cloud server refused the request.");

            var stream = await response.Content.ReadAsStreamAsync();

            return File(stream, "video/mp4", enableRangeProcessing: true);
        }




        //=================================================================================end method stream cloud
        // Remove SafeReadFile - not needed for streaming
        // Keep GetVideoMimeType as-is (it's correct)

        // For uploads, SafeWriteFile can stay but add error logging:
        private bool SafeWriteFile(string path, IFormFile file)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)); // Ensure directory exists

                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    file.CopyTo(stream);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطأ في حفظ الملف: {path}");
                return false;
            }
        }
        //=======================================================================================
        /* //✔ Add a safe file‑write helper (for uploads)
         private bool SafeWriteFile(string path, IFormFile file)
         {
             try
             {
                 using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                 {
                     file.CopyTo(stream);
                 }
                 return true;
             }
             catch
             {
                 return false;
             }
         }*/
    }
}


    



    
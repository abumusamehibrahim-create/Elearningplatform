using ELearningPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class VideoUploadManager
{
    private readonly IWebHostEnvironment _env;

    public VideoUploadManager(IWebHostEnvironment env)
    {
        _env = env;
    }

    // MAIN FUNCTION
    public async Task<string> HandleUpload(
        string uploadType,
        Video video,
        IFormFile videoFile,
        IFormFile worksheetFile)
    {
       
        
        // -------------------------
        // LOCAL → let controller handle everything
        // -------------------------
        if (uploadType == "Local")
        {
            return "Local";
        }

        // -------------------------
        // CLOUD UPLOAD
        // -------------------------
        if (uploadType == "Cloud")
        {
            // Upload video to cloud
            string cloudUrl = await UploadToCloud(videoFile);

            video.Description = cloudUrl;
            video.UseBunny = false;
            video.FileName = null;

            // Upload worksheet to cloud (optional)
            if (worksheetFile != null)
            {
                string worksheetUrl = await UploadWorksheetToCloud(worksheetFile);

                video.WorksheetFiles ??= new List<WorksheetFile>();
                video.WorksheetFiles.Add(new WorksheetFile
                {
                    FileName = worksheetFile.FileName,
                    FilePath = worksheetUrl,
                    FileUrl = worksheetUrl,
                    AllowDownload = true
                });
            }

            return "Cloud";
        }

        // -------------------------
        // BUNNY UPLOAD
        // -------------------------
        if (uploadType == "Bunny")
        {
            // Upload video to Bunny
            var bunny = await UploadToBunny(videoFile);

            video.UseBunny = true;
            video.BunnyVideoId = bunny.VideoId;
            video.BunnyLibraryId = bunny.LibraryId;
            video.BunnyCDNHostname = bunny.Hostname;

            video.FileName = null;
            video.Description = null;

            // Upload worksheet to Bunny (optional)
            if (worksheetFile != null)
            {
                string worksheetUrl = await UploadWorksheetToBunny(worksheetFile);

                video.WorksheetFiles ??= new List<WorksheetFile>();
                video.WorksheetFiles.Add(new WorksheetFile
                {
                    FileName = worksheetFile.FileName,
                    FilePath = worksheetUrl,
                    FileUrl = worksheetUrl,
                    AllowDownload = true
                });
            }

            return "Bunny";
        }

        throw new Exception("Invalid upload type");
    }

    // -------------------------
    // CLOUD VIDEO UPLOAD
    // -------------------------
    private async Task<string> UploadToCloud(IFormFile file)
    {
        // TODO: real cloud upload
        return "https://cloud-server.com/videos/" + file.FileName;
    }

    // -------------------------
    // CLOUD WORKSHEET UPLOAD
    // -------------------------
    private async Task<string> UploadWorksheetToCloud(IFormFile file)
    {
        // TODO: real cloud upload
        return "https://cloud-server.com/worksheets/" + file.FileName;
    }

    // -------------------------
    // BUNNY VIDEO UPLOAD
    // -------------------------
    private async Task<(string VideoId, string LibraryId, string Hostname)> UploadToBunny(IFormFile file)
    {
        // TODO: real Bunny API upload
        return ("12345", "99999", "mycdn.b-cdn.net");
    }

    // -------------------------
    // BUNNY WORKSHEET UPLOAD
    // -------------------------
    private async Task<string> UploadWorksheetToBunny(IFormFile file)
    {
        // TODO: real Bunny API upload
        return "https://mycdn.b-cdn.net/worksheets/" + file.FileName;
    }
   /* [HttpPost]
    public async Task<IActionResult> UploadVideo(int courseId, int? id,
string title, string? description, bool isFree, int orderNumber,
IFormFile? videoFile, IFormFile[] worksheetFiles, bool[] allowDownload, List<WorksheetItem> worksheetItems)
    {
        // Read upload type (Local / Cloud / Bunny)
        string uploadType = Request.Form["uploadType"];

        // ========================= NEW VIDEO =========================
        if (id == null)
        {
            if (videoFile == null)
            {
                ViewBag.Error = "يرجى اختيار ملف فيديو";
                ViewBag.CourseId = courseId;
                return View();
            }

            var allowedExtensions = new[]
            {
            ".mp4", ".webm", ".mkv", ".avi", ".mov",
            ".wmv", ".flv", ".m4v", ".3gp", ".ts"
        };

            var ext = Path.GetExtension(videoFile.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
            {
                ViewBag.Error = "نوع الملف غير مدعوم";
                ViewBag.CourseId = courseId;
                return View();
            }

            // Create video model
            var video = new Video
            {
                CourseId = courseId,
                Title = title,
                Description = description,
                IsFree = isFree,
                OrderNumber = orderNumber
            };

            // Call UploadManager
            var result = await _uploadManager.HandleUpload(uploadType, video, videoFile, null);

            // ========================= LOCAL UPLOAD =========================
            if (result == "Local")
            {
                string fileName = SanitizeFileName(title) + ext;
                var uploadPath = Path.Combine(_env.ContentRootPath, "ProtectedVideos");
                Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    int counter = 1;
                    while (System.IO.File.Exists(filePath))
                    {
                        fileName = SanitizeFileName(title) + $"_{counter}" + ext;
                        filePath = Path.Combine(uploadPath, fileName);
                        counter++;
                    }
                }

                if (!SafeWriteFile(filePath, videoFile))
                {
                    ViewBag.Error = "حدث خطأ أثناء رفع الفيديو";
                    ViewBag.CourseId = courseId;
                    return View();
                }

                video.FileName = fileName;
                video.UseBunny = false;
                video.Description = null;
            }

            // ========================= CLOUD UPLOAD =========================
            if (result == "Cloud")
            {
                // UploadManager already updated:
                // video.Description = cloudUrl;
                // video.FileName = null;
                // video.UseBunny = false;
            }

            // ========================= BUNNY UPLOAD =========================
            if (result == "Bunny")
            {
                // UploadManager already updated:
                // video.UseBunny = true;
                // video.BunnyVideoId = "...";
                // video.BunnyLibraryId = "...";
                // video.BunnyCDNHostname = "...";
            }

            // Save video
            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            // ========================= WORKSHEET FILES =========================
            if (worksheetFiles != null && worksheetFiles.Length > 0)
            {
                if (result == "Local")
                {
                    var allowed = new[]
                    {
                    ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx",
                    ".ppt", ".pptx", ".xls", ".xlsx"
                };

                    var folder = Path.Combine(_env.ContentRootPath, "ProtectedWorksheetFile");
                    Directory.CreateDirectory(folder);

                    int index = 0;
                    foreach (var file in worksheetFiles)
                    {
                        var ext2 = Path.GetExtension(file.FileName).ToLower();
                        if (!allowed.Contains(ext2)) continue;

                        var wsName = Guid.NewGuid().ToString("N") + ext2;
                        var wsPath = Path.Combine(folder, wsName);

                        using (var stream = new FileStream(wsPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        _context.WorksheetFiles.Add(new WorksheetFile
                        {
                            VideoId = video.Id,
                            FileName = wsName,
                            FilePath = "/ProtectedWorksheetFile/" + wsName,
                            AllowDownload = allowDownload != null && allowDownload.Length > index
                                ? allowDownload[index]
                                : false
                        });

                        index++;
                    }
                }
                else if (result == "Cloud")
                {
                    foreach (var file in worksheetFiles)
                    {
                        string url = await _uploadManager.UploadWorksheetToCloud(file);

                        _context.WorksheetFiles.Add(new WorksheetFile
                        {
                            VideoId = video.Id,
                            FileName = file.FileName,
                            FilePath = url,
                            FileUrl = url,
                            AllowDownload = true
                        });
                    }
                }
                else if (result == "Bunny")
                {
                    foreach (var file in worksheetFiles)
                    {
                        string url = await _uploadManager.UploadWorksheetToBunny(file);

                        _context.WorksheetFiles.Add(new WorksheetFile
                        {
                            VideoId = video.Id,
                            FileName = file.FileName,
                            FilePath = url,
                            FileUrl = url,
                            AllowDownload = true
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }

            // ========================= WORKSHEET ITEMS =========================
            if (worksheetItems != null && worksheetItems.Any(x => !string.IsNullOrWhiteSpace(x.Question)))
            {
                foreach (var item in worksheetItems)
                {
                    if (string.IsNullOrWhiteSpace(item.Question))
                        continue;

                    item.VideoId = video.Id;
                    _context.WorksheetItems.Add(item);
                }

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "تم رفع الفيديو بنجاح";
            return RedirectToAction("Videos", new { courseId });
        }

        // ========================= UPDATE VIDEO =========================
        var existingVideo = _context.Videos.FirstOrDefault(v => v.Id == id);
        if (existingVideo == null)
            return NotFound();

        existingVideo.Title = title;
        existingVideo.Description = description;
        existingVideo.IsFree = isFree;
        existingVideo.OrderNumber = orderNumber;

        var updateResult = await _uploadManager.HandleUpload(uploadType, existingVideo, videoFile, null);

        // Delete old local file if switching to Cloud/Bunny
        if (existingVideo.FileName != null && updateResult != "Local")
        {
            var oldPath = Path.Combine(_env.ContentRootPath, "ProtectedVideos", existingVideo.FileName);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        // Local update
        if (updateResult == "Local" && videoFile != null)
        {
            var ext = Path.GetExtension(videoFile.FileName).ToLower();
            string fileName = SanitizeFileName(title) + ext;

            var uploadPath = Path.Combine(_env.ContentRootPath, "ProtectedVideos");
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await videoFile.CopyToAsync(stream);
            }

            existingVideo.FileName = fileName;
            existingVideo.UseBunny = false;
            existingVideo.Description = null;
        }

        _context.Update(existingVideo);
        await _context.SaveChangesAsync();

        TempData["Success"] = "تم تحديث الفيديو بنجاح";
        return RedirectToAction("Videos", new { courseId });
    }
   */

    //if you want to call
    //string result = await _videoUploadManager.HandleUpload(uploadType, video, videoFile, worksheetFile);
}

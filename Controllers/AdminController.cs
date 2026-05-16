using ELearningPlatform.Data;
using ELearningPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ELearningPlatform.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController :BaseController
    {
        // private readonly ApplicationDbContext _context;

        ///public AdminController(ApplicationDbContext context)
        //{
        //    _context = context;
        /// }
        //private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        IFormFile[] worksheetFiles;
        public AdminController(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env):base(db)
        {
           // _context = db;
            _userManager = userManager;
            _env = env;
        }
        // student with payemnt=======================================
        public async Task<IActionResult> Students(string name, string email, string status, string course, string transfer)
        {
            try { 
            var students = _context.Users
                .Include(u => u.Payments)
                .ThenInclude(p => p.Course)
                .AsQueryable();

            // البحث
            if (!string.IsNullOrEmpty(transfer))
            {
                students = students.Where(u =>
                    u.FullName.Contains(transfer) ||
                    u.Email.Contains(transfer));
            }

            // فلترة حسب حالة الدفع
            if (!string.IsNullOrEmpty(status))
            {
                students = students.Where(u =>
                    u.Payments.Any(p => p.Status == status));
            }

            // فلترة حسب الكورس
            if (!string.IsNullOrEmpty(course))
            {
                students = students.Where(u =>
                    u.Payments.Any(p => p.CourseId != null &&
                                        p.Course != null &&
                                        p.Course.Title.Contains(course)));
            }


            var list = await students
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(list);

            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }

        }

        //index========================================
        public async Task<IActionResult> Index()
        {
            try { 
            ViewBag.TotalStudents = await _context.Payments.Select(p => p.UserId).Distinct().CountAsync();
            ViewBag.TotalRevenue = await _context.Payments.Where(p => p.Status == "Completed").SumAsync(p => p.Amount);
            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.TotalVideos = await _context.Videos.CountAsync();
            return View();
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }
        // ===================== APPROVE PAYMENT =====================
        public async Task<IActionResult> ApprovePayment(int id)
        {
            try { 


            var payment = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound();

            payment.Status = "Approved";
                
                if(payment.User!=null)
                { payment.User.IsActive = true;
                    // تفعيل المستخدم
                    payment.User.IsActive = true;

                    // إصلاح أي مشاكل قديمة
                    payment.User.LockoutEnd = null;
                    payment.User.LockoutEnabled = false;
                    payment.User.EmailConfirmed = true;

                    // تحديث SecurityStamp لإعادة تفعيل الحساب
                    payment.User.SecurityStamp = Guid.NewGuid().ToString();

                    // تحديث SessionToken
                    payment.User.SessionToken = Guid.NewGuid().ToString();

                    await _userManager.UpdateAsync(payment.User);






                }

                await _context.SaveChangesAsync();

                // إضافة الطالب إلى الكورس


                // var exists = await _context.UserCourses
                //  .AnyAsync(uc => uc.UserId == payment.UserId && uc.CourseId == payment.CourseId);
                // إذا CourseId = null → لا يوجد كورس لإضافته للطالب

                if (payment.CourseId != null)
            {
                bool alreadyHasCourse = await _context.UserCourses

                    .AnyAsync(uc => uc.UserId == payment.UserId &&
                                    uc.CourseId != null &&
                                    uc.CourseId == payment.CourseId);




            if (!alreadyHasCourse)
                {
                    if (payment.CourseId.HasValue)
                    {
                        _context.UserCourses.Add(new UserCourse
                        {
                            UserId = payment.UserId,
                            CourseId = payment.CourseId.Value
                        });
                    }


                    await _context.SaveChangesAsync();
                } }
            // سجل النشاط
            _context.ActivityLogs.Add(new ActivityLog
            {
                Action = "Approve Payment",
                AdminName = User.Identity.Name,
                Details = $"Approved payment #{payment.Id} for user {payment.User?.Email ?? "Unknown"} (Course: {payment.Course?.Title ?? "No Course"})",
                Date = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["Message"] = "✔ تم قبول الدفع بنجاح.";
            return RedirectToAction("PaymentList");
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }

        // ===================== REJECT PAYMENT =====================
        public async Task<IActionResult> RejectPayment(int id)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(id);

                if (payment == null)
                    return NotFound();

                payment.Status = "Rejected";
                await _context.SaveChangesAsync();
                // سجل النشاط
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Action = "Reject Payment",
                    AdminName = User.Identity.Name,
                    Details = $"Rejected payment #{payment.Id} for user {payment.User?.Email ?? "Unknown"} (Course: {payment.Course?.Title ?? "No Course"})",
                    Date = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                TempData["Message"] = "❌ تم رفض الدفع.";
                return RedirectToAction("PaymentList");

            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }
        // ===== Courses ===================================================================================
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses.Include(c => c.Videos).ToListAsync();
            return View(courses);
        }
        //=================================================================================

        [HttpGet]
        public IActionResult CreateCourse() => View();


        //===========================================================

        [HttpPost]
        public async Task<IActionResult> CreateCourse(Course course, IFormFile? thumbnail)
        {
            try { 
            if (!ModelState.IsValid)
                return View(course);

            if (thumbnail != null)
            {
                // 1) إنشاء المجلد إذا لم يكن موجودًا
                var folderPath = Path.Combine("wwwroot", "thumbnails");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // 2) التحقق من الامتداد
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(thumbnail.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("", "يجب أن تكون الصورة بصيغة JPG أو PNG أو WEBP فقط.");
                    return View(course);
                }

                // 3) التحقق من الحجم (2MB)
                if (thumbnail.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "حجم الصورة يجب ألا يتجاوز 2MB.");
                    return View(course);
                }

                // 4) إنشاء اسم فريد
                var fileName = Guid.NewGuid().ToString() + extension;
                var savePath = Path.Combine(folderPath, fileName);

                // 5) ضغط الصورة وحفظها
                using (var image = SixLabors.ImageSharp.Image.Load(thumbnail.OpenReadStream()))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(800, 0) // عرض 800 بكسل، الارتفاع تلقائي
                    }));

                    await image.SaveAsync(savePath);
                }

                course.ThumbnailUrl = fileName;
            }
            else
            {
                course.ThumbnailUrl = "default-course.jpg";
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إنشاء الكورس بنجاح";
            return RedirectToAction("Courses");

            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }

        }


        // ===== Videos ================================================================================
        public async Task<IActionResult> Videos(int courseId)
        {
            try { 
            var course = await _context.Courses
                .Include(c => c.Videos.OrderBy(v => v.OrderNumber))
                .FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return NotFound();
            ViewBag.Course = course;
            return View(course.Videos.ToList());
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }

        [HttpGet]
        public IActionResult UploadVideo(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        //===========================================DeleteCourses=====================
        [HttpGet]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try { 
            var course = await _context.Courses
                .Include(c => c.Videos)
                    .ThenInclude(v => v.WorksheetFiles)
                .Include(c => c.Videos)
                    .ThenInclude(v => v.WorksheetItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            // 1) حذف ملفات الـ WorksheetFile من السيرفر
            foreach (var video in course.Videos)
            {
                foreach (var file in video.WorksheetFiles)
                {
                    var path = Path.Combine("ProtectedWorksheetFile", file.FileName);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
            }

            // 2) حذف ملفات الفيديو من السيرفر
            foreach (var video in course.Videos)
            {
                var videoPath = Path.Combine("ProtectedVideos", video.FileName);
                if (System.IO.File.Exists(videoPath))
                    System.IO.File.Delete(videoPath);
            }

            // 3) حذف WorksheetItems
            _context.WorksheetItems.RemoveRange(
                course.Videos.SelectMany(v => v.WorksheetItems)
            );

            // 4) حذف WorksheetFiles
            _context.WorksheetFiles.RemoveRange(
                course.Videos.SelectMany(v => v.WorksheetFiles)
            );

            // 5) حذف الفيديوهات
            _context.Videos.RemoveRange(course.Videos);

            // ⚠️ لا نحذف Payments
            // ⚠️ لا نحذف UserCourse

            // 6) حذف الكورس نفسه
            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();

            return RedirectToAction("Courses");
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }
        //===================================================================Editcourses
        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            try { 
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            return View(course);
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditCourse(int id, Course updatedCourse, IFormFile? thumbnail)
        {
            try { 
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            // تحديث البيانات الأساسية
            course.Title = updatedCourse.Title;
            course.Description = updatedCourse.Description;
            course.Price = updatedCourse.Price;
            course.IsActive = updatedCourse.IsActive;

            // تحديث الصورة إذا تم رفع صورة جديدة
            if (thumbnail != null)
            {
                // حذف الصورة القديمة
                if (!string.IsNullOrEmpty(course.ThumbnailUrl))
                {
                    var oldPath = Path.Combine("wwwroot", "thumbnails", course.ThumbnailUrl);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // حفظ الصورة الجديدة
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnail.FileName);
                var newPath = Path.Combine("wwwroot", "thumbnails", fileName);

                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await thumbnail.CopyToAsync(stream);
                }

                course.ThumbnailUrl = fileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Courses");

            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }
        //===========================================================================confirmDelet courses
        [HttpPost]
        public async Task<IActionResult> ConfirmDeleteCourse(int id)
        {
            try { 
            var course = await _context.Courses
                .Include(c => c.Videos)
                    .ThenInclude(v => v.WorksheetFiles)
                .Include(c => c.Videos)
                    .ThenInclude(v => v.WorksheetItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            // حذف الملفات من السيرفر
            foreach (var video in course.Videos)
            {
                foreach (var file in video.WorksheetFiles)
                {
                    var path = Path.Combine("ProtectedWorksheetFile", file.FileName);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                var videoPath = Path.Combine("ProtectedVideos", video.FileName);
                if (System.IO.File.Exists(videoPath))
                    System.IO.File.Delete(videoPath);
            }

            // حذف البيانات من قاعدة البيانات
            _context.WorksheetItems.RemoveRange(course.Videos.SelectMany(v => v.WorksheetItems));
            _context.WorksheetFiles.RemoveRange(course.Videos.SelectMany(v => v.WorksheetFiles));
            _context.Videos.RemoveRange(course.Videos);

            // ⚠️ لا نحذف Payments
            // ⚠️ لا نحذف UserCourse

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف الكورس بنجاح";
            return RedirectToAction("Courses");
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }












        // =====================  UploadVideo =====================

        [HttpPost]
        public async Task<IActionResult> UploadVideo(int courseId, int? id,
    string title, string? description, bool isFree, int orderNumber,
    IFormFile? videoFile, IFormFile[] worksheetFiles, bool[] allowDownload, List<WorksheetItem> worksheetItems)
        {
            try {  // رفع فيديو جديد
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

                // ✅ تغيير اسم الملف - استخدم Title أو معرف فريد
                // الخيار 1: استخدم Title مباشرة (بحذف المسافات)
                string fileName = SanitizeFileName(title) + ext;

                // أو الخيار 2: استخدم معرف فريد (أكثر أماناً)
                // string fileName = Guid.NewGuid().ToString("N") + ext;

                var uploadPath = Path.Combine(_env.ContentRootPath, "ProtectedVideos");
                Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);

                // تأكد من عدم وجود ملف بنفس الاسم
                if (System.IO.File.Exists(filePath))
                {
                    // إذا كان موجود، أضف رقم
                    int counter = 1;
                    while (System.IO.File.Exists(filePath))
                    {
                        fileName = SanitizeFileName(title) + $"_{counter}" + ext;
                        filePath = Path.Combine(uploadPath, fileName);
                        counter++;
                    }
                }

                // رفع آمن
                if (!SafeWriteFile(filePath, videoFile))
                {
                    ViewBag.Error = "حدث خطأ أثناء رفع الفيديو";
                    ViewBag.CourseId = courseId;
                    return View();
                }

                // ✅ حفظ الفيديو بالاسم الجديد
                var video = new Video
                {
                    CourseId = courseId,
                    Title = title,
                    Description = description,
                    IsFree = isFree,
                    OrderNumber = orderNumber,
                    FileName = fileName  // ← الاسم الجديد
                };

                _context.Videos.Add(video);
                await _context.SaveChangesAsync();

                // حفظ أوراق العمل
                if (worksheetFiles != null && worksheetFiles.Length > 0)
                {
                    var allowed = new[]
                    {
                ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx"
            };

                    var folder = Path.Combine(_env.ContentRootPath, "ProtectedWorksheetFile");
                    Directory.CreateDirectory(folder);
                    int index = 0;
                    foreach (var file in worksheetFiles)
                    {
                        if (file == null) continue;
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

                    await _context.SaveChangesAsync();
                }

                // حفظ أسئلة الفيديو
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

            // تعديل فيديو موجود
            var existingVideo = _context.Videos.FirstOrDefault(v => v.Id == id);
            if (existingVideo == null)
                return NotFound();

            existingVideo.Title = title;
            existingVideo.Description = description;
            existingVideo.IsFree = isFree;
            existingVideo.OrderNumber = orderNumber;

            if (videoFile != null)
            {
                var allowedExtensions = new[] { ".mp4", ".webm", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".m4v", ".3gp", ".ts" };
                var ext = Path.GetExtension(videoFile.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                {
                    ViewBag.Error = "نوع الملف غير مدعوم";
                    ViewBag.CourseId = courseId;
                    return View(existingVideo);
                }

                // ✅ حذف الملف القديم
                var oldPath = Path.Combine(_env.ContentRootPath, "ProtectedVideos", existingVideo.FileName);
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }

                // ✅ إنشاء اسم جديد
                string fileName = SanitizeFileName(title) + ext;
                var uploadPath = Path.Combine(_env.ContentRootPath, "ProtectedVideos");
                Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);

                // تأكد من عدم وجود ملف بنفس الاسم
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

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await videoFile.CopyToAsync(stream);
                }

                existingVideo.FileName = fileName;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث الفيديو بنجاح";
            return RedirectToAction("Videos", new { courseId });
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }

        }//==============================================================

        // ✅ دالة لتنظيف اسم الملف (إزالة الأحرف الخاصة)
        private string SanitizeFileName(string fileName)
        {

            if (string.IsNullOrWhiteSpace(fileName))
                return Guid.NewGuid().ToString("N");

            // إزالة الأحرف الخاصة والمسافات
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            string sanitized = new string(fileName
                .Where(c => !invalidChars.Contains(c) && c != ' ')
                .ToArray());

            // إزالة النقاط الإضافية
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"\.+", ".");

            // إذا كان فارغ، استخدم معرف فريد
            if (string.IsNullOrWhiteSpace(sanitized))
                return Guid.NewGuid().ToString("N");

            return sanitized;
        }

       

        //========================================================================end upload

        [HttpPost]
        public async Task<IActionResult> UploadVideox(int courseId, int? id,
 string title, string? description, bool isFree, int orderNumber,
 IFormFile? videoFile, IFormFile[] worksheetFiles, bool[] allowDownload , List<WorksheetItem> worksheetItems)
        {
            try { 
            // رفع فيديو جديد
            if (id == null)
            {
                if (videoFile == null)
                {
                    ViewBag.Error = "يرجى اختيار ملف فيديو";
                    ViewBag.CourseId = courseId;
                    return View();
                }

                //  var allowedExtensions = new[] { ".mp4", ".webm", ".mkv", ".avi" };
                // جميع الامتدادات المدعومة
                var allowedExtensions = new[]
                {
            ".mp4", ".webm", ".mkv", ".avi", ".mov",
            ".wmv", ".flv", ".m4v", ".3gp", ".ts"
        };



                var ext = Path.GetExtension(videoFile.FileName).ToLower();//========================================================

                if (!allowedExtensions.Contains(ext))//=============================
                {
                    ViewBag.Error = "نوع الملف غير مدعوم";
                    ViewBag.CourseId = courseId;
                    return View();
                }//==========================================================================

                var fileName = Guid.NewGuid().ToString("N") + ext;
                var uploadPath = Path.Combine(_env.ContentRootPath, "ProtectedVideos");
                Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);

                // رفع آمن باستخدام SafeWriteFile
                if (!SafeWriteFile(filePath, videoFile))
                {
                    ViewBag.Error = "حدث خطأ أثناء رفع الفيديو. تأكد أن الملف غير تالف.";
                    ViewBag.CourseId = courseId;
                    return View();
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await videoFile.CopyToAsync(stream);
                }
                //========================================================
                var video = new Video
                {
                    CourseId = courseId,
                    Title = title,
                    Description = description,
                    IsFree = isFree,
                    OrderNumber = orderNumber,
                    FileName = fileName
                };
//////////////////////////////////////////////////////////////////////////////////
                _context.Videos.Add(video);
                await _context.SaveChangesAsync();

                // حفظ أوراق العمل
                if (worksheetFiles != null && worksheetFiles.Length > 0)
                {
                    var allowed = new[]
                    {
                ".pdf",".jpg",".jpeg",".png",".doc",".docx",".ppt",".pptx",".xls",".xlsx"
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
                            FilePath = "/ProtectedWorksheetFile/" + wsName ,  // ⭐ مهم جدًا
                            AllowDownload = allowDownload != null && allowDownload.Length > index
                        ? allowDownload[index]
                        : false
                        });
                        index++;
                    }

                    if (worksheetFiles == null || worksheetFiles.Length == 0)
                    {
                        // المستخدم لم يرفع أي ملفات → لا تحفظ شيء
                    }
                    else
                    {
                        await _context.SaveChangesAsync();
                        // هنا نرفع الملفات ونحفظها
                    }
                }
                // حفظ أسئلة الفيديو (WorksheetItem)
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

                TempData["Success"] = "تم رفع الفيديو وأوراق العمل والأسئلة بنجاح";
                return RedirectToAction("Videos", new { courseId });
               
            }

            // تعديل فيديو موجود
            var existingVideo = _context.Videos.FirstOrDefault(v => v.Id == id);
            if (existingVideo == null)
                return NotFound();

            existingVideo.Title = title;
            existingVideo.Description = description;
            existingVideo.IsFree = isFree;
            existingVideo.OrderNumber = orderNumber;

            if (videoFile != null)
            {
                var allowedExtensions = new[] { ".mp4", ".webm", ".mkv", ".avi" };
                var ext = Path.GetExtension(videoFile.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                {
                    ViewBag.Error = "نوع الملف غير مدعوم";
                    ViewBag.CourseId = courseId;
                    return View(existingVideo);
                }

                var fileName = Guid.NewGuid().ToString("N") + ext;
                var uploadPath = Path.Combine(_env.ContentRootPath, "ProtectedVideos");
                Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await videoFile.CopyToAsync(stream);
                }

                existingVideo.FileName = fileName;
            }

            await _context.SaveChangesAsync();

            // حفظ أوراق العمل عند التعديل
            if (worksheetFiles != null && worksheetFiles.Length > 0)
            {
                var allowed = new[]
                {
            ".pdf",".jpg",".jpeg",".png",".doc",".docx",".ppt",".pptx",".xls",".xlsx"
        };

                var folder = Path.Combine(_env.ContentRootPath, "ProtectedWorksheetFile");
                Directory.CreateDirectory(folder);

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
                        VideoId = existingVideo.Id,
                        FileName = wsName,
                        FilePath = "/ProtectedWorksheetFile/" + wsName   // ⭐ مهم جدًا
                    });
                }

                await _context.SaveChangesAsync();
            }




            TempData["Success"] = "تم تحديث الفيديو وأوراق العمل بنجاح";
            return RedirectToAction("Videos", new { courseId });

            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }//=======================================================================================
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
        }

        //✔ Add a safe file‑write helper (for uploads)
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
        }














        //=================================================================updatevideo













        [HttpGet]
        public async Task<IActionResult> UpdateVideo(int id, int courseId)
        {
            try { 
            var video = await _context.Videos
                .Include(v => v.WorksheetFiles)
                .Include(v => v.WorksheetItems)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
                return NotFound();

            ViewBag.CourseId = courseId;

            return View("UploadVideo", video); // نفس صفحة الرفع

            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }


        }

        [HttpPost]
        public async Task<IActionResult> UpdateVideo(
    int id,
    string title,
    string? description,
    bool isFree,
    int orderNumber,
    IFormFile? videoFile,
    IFormFile[] worksheetFiles,
    bool[] allowDownload,
    List<WorksheetItem> worksheetItems)
        {
            try { 
            var video = await _context.Videos
                .Include(v => v.WorksheetFiles)
                .Include(v => v.WorksheetItems)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
                return NotFound();

            // تعديل بيانات الفيديو
            video.Title = title;
            video.Description = description;
            video.IsFree = isFree;
            video.OrderNumber = orderNumber;

            // تعديل ملف الفيديو (اختياري)
            if (videoFile != null)
            {
                var ext = Path.GetExtension(videoFile.FileName).ToLower();
                var allowed = new[] { ".mp4", ".webm", ".mkv", ".avi" };

                if (allowed.Contains(ext))
                {
                    var fileName = Guid.NewGuid().ToString("N") + ext;
                    var path = Path.Combine(_env.ContentRootPath, "ProtectedVideos", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await videoFile.CopyToAsync(stream);
                    }

                    video.FileName = fileName;
                }
            }

            // تعديل أوراق العمل (إضافة جديدة فقط)
            if (worksheetFiles != null && worksheetFiles.Length > 0)
            {
                var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx" };
                var folder = Path.Combine(_env.ContentRootPath, "ProtectedWorksheetFile");
                Directory.CreateDirectory(folder);

                int index = 0;

                foreach (var file in worksheetFiles)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (!allowed.Contains(ext)) continue;

                    var wsName = Guid.NewGuid().ToString("N") + ext;
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

            // تعديل الأسئلة
            var oldItems = _context.WorksheetItems.Where(x => x.VideoId == video.Id);
            _context.WorksheetItems.RemoveRange(oldItems);

            if (worksheetItems != null)
            {
                foreach (var item in worksheetItems)
                {
                    if (string.IsNullOrWhiteSpace(item.Question))
                        continue;

                    item.VideoId = video.Id;
                    _context.WorksheetItems.Add(item);
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث الفيديو بنجاح";
            return RedirectToAction("Videos", new { courseId = video.CourseId });

            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }

        }

        //========================DeleteViedio======================================================================
        [HttpPost]
        public async Task<IActionResult> DeleteVideo(int videoId)
        {
            try { 
            // CHANGE: جلب الفيديو مع الملفات والأسئلة
            var video = await _context.Videos
                .Include(v => v.WorksheetFiles)
                .Include(v => v.WorksheetItems)
                .FirstOrDefaultAsync(v => v.Id == videoId);

            if (video != null)
            {
                // CHANGE: حذف ملف الفيديو من السيرفر
                var filePath = Path.Combine(_env.ContentRootPath, "ProtectedVideos", video.FileName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                // CHANGE: حذف ملفات أوراق العمل من السيرفر
                foreach (var ws in video.WorksheetFiles)
                {
                    var wsPath = Path.Combine(_env.ContentRootPath, "ProtectedWorksheetFile", ws.FileName);
                    if (System.IO.File.Exists(wsPath))
                        System.IO.File.Delete(wsPath);
                }

                // CHANGE: حذف أوراق العمل من قاعدة البيانات
                _context.WorksheetFiles.RemoveRange(video.WorksheetFiles);

                // CHANGE: حذف الأسئلة من قاعدة البيانات
                _context.WorksheetItems.RemoveRange(video.WorksheetItems);

                // حذف الفيديو نفسه
                _context.Videos.Remove(video);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Videos", new { courseId = video?.CourseId });
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }

        //=========================================================================================================
        [HttpPost]
        public IActionResult UpdateVideoOrder([FromBody] List<VideoOrderUpdate> order)
        {
            try { 
            foreach (var item in order)
            {
                var video = _context.Videos.Find(item.Id);
                if (video != null)
                {
                    video.OrderNumber = item.OrderNumber;
                }
            }

            _context.SaveChanges();
            return Ok();
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }

        // watchllllllllllllllllllllllllllllllllllllllllllllllll

        public async Task<IActionResult> Watch(int videoId)
        {
           try{
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




        //delete payment==========================================
        [HttpPost]
        public async Task<IActionResult> DeletePayment(int id)
        {
            try { 
            var payment = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound();

            if (payment.Status == "Approved")
            {
                TempData["Message"] = "❌ لا يمكن حذف دفعة تم قبولها مسبقًا.";
                return RedirectToAction("PaymentList");
            }

            // احذف الدفع
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            // إذا CourseId = null → لا يوجد كورس لإزالته من UserCourses
            if (payment.CourseId != null)
            {
                // تحقق من وجود دفعات أخرى لنفس الكورس
                bool hasOtherPaymentsForSameCourse = await _context.Payments
                    .AnyAsync(p => p.UserId == payment.UserId &&
                                   p.CourseId != null &&
                                   p.CourseId == payment.CourseId);

                // إذا لا توجد دفعات أخرى → احذف UserCourse
                if (!hasOtherPaymentsForSameCourse)
                {
                    var userCourse = await _context.UserCourses
                        .FirstOrDefaultAsync(uc => uc.UserId == payment.UserId &&
                                                   uc.CourseId != null &&
                                                   uc.CourseId == payment.CourseId);

                    if (userCourse != null)
                    {
                        _context.UserCourses.Remove(userCourse);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // سجل النشاط (آمن ضد NULL)
            _context.ActivityLogs.Add(new ActivityLog
            {
                Action = "Delete Payment",
                AdminName = User.Identity.Name,
                Details = $"Deleted payment #{payment.Id} for user {payment.User?.Email ?? "Unknown"} (Course: {payment.Course?.Title ?? "No Course"})",
                Date = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["Message"] = "✔ تم حذف الدفع بنجاح وتم تحديث حالة الكورس للطالب.";

            return RedirectToAction("PaymentList");
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }

        // PaymentList===========================================================
        public async Task<IActionResult> PaymentList()
        {
            try{
                var payments = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }

        //page visit
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> PageVisits(string? user, DateTime? from, DateTime? to)
        {
            try{
                var logs = _context.PageVisitLogs.AsQueryable();

            if (!string.IsNullOrEmpty(user))
                logs = logs.Where(x => x.UserName.Contains(user));

            if (from.HasValue)
                logs = logs.Where(x => x.VisitTime >= from.Value);

            if (to.HasValue)
                logs = logs.Where(x => x.VisitTime <= to.Value);

            logs = logs.OrderByDescending(x => x.VisitTime);

            // 🔥 إرسال القيم للـ View
            ViewBag.UserFilter = user;
            ViewBag.FromFilter = from?.ToString("yyyy-MM-dd");
            ViewBag.ToFilter = to?.ToString("yyyy-MM-dd");

            return View(await logs.ToListAsync());
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }
        // admin chage passowrd
        [HttpGet]
        public IActionResult EditAdmin()
        {
            return View();
        }
        //===============================================================
        [HttpPost]
        public async Task<IActionResult> EditAdmin(string oldPassword, string newPassword)
        {
            try { 
            var admin = await _userManager.GetUserAsync(User);
            if (admin == null)
                return RedirectToAction("Login", "Account");

            var result = await _userManager.ChangePasswordAsync(admin, oldPassword, newPassword);

            if (result.Succeeded)
            {
                admin.PlainPassword = newPassword; // تحديث كلمة المرور الأصلية
              // await _context.SaveChangesAsync();

                TempData["Message"] = "✔ تم تغيير كلمة المرور بنجاح";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "❌ كلمة المرور القديمة غير صحيحة";
            return View();
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }
        //=======================================================================DeleteUser
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try{
                var user = await _context.Users
        .Include(u => u.Payments)
        .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

                // 1) منع حذف الأدمن
                if (await _userManager.IsInRoleAsync(user, "Admin")
                    || await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                {
                TempData["Message"] = "❌ لا يمكن حذف حساب أدمن.";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Students");
            }

            // 2) إذا كان لديه مدفوعات → لا تحذف
            if (user.Payments.Any())
            {
                TempData["Message"] = "⚠ لا يمكن حذف هذا المستخدم لأنه لديه مدفوعات. احذف المدفوعات أولاً.";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Students");
            }
            bool hasCourses = await _context.UserCourses
        .AnyAsync(uc => uc.UserId == id);
            // 3) إذا كان لديه كورسات → لا تحذف
            if (hasCourses)
            {
                TempData["Message"] = "⚠ لا يمكن حذف هذا المستخدم لأنه مسجّل في كورسات. احذف الكورسات أولاً.";
                TempData["MessageType"] = "warning";
                                return RedirectToAction("Students");
            }

            // 4) إذا لم يكن لديه أي علاقات → احذفه بالكامل
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        /*   // 3) إخفاء بيانات المستخدم بدل الحذف
            user.FullName = "Deleted User";
            user.UserName = $"deleted_{Guid.NewGuid().ToString().Substring(0, 8)}";
            user.Email = $"{Guid.NewGuid().ToString().Substring(0, 8)}@deleted.com";
            user.PhoneNumber = null;
            user.PlainPassword = null;
            user.PasswordHash = null;
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.IsPaid = false;

            await _context.SaveChangesAsync(); */
            // 4) سجل النشاط
            _context.ActivityLogs.Add(new ActivityLog
            {
                Action = "Delete User",
                AdminName = User.Identity.Name,
                Details = $"User {id} was anonymized and deactivated.",
                Date = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["Message"] = "✔ تم حذف المستخدم بالكامل لأنه غير مرتبط بأي مدفوعات أو كورسات.";
            TempData["MessageType"] = "success";
            return RedirectToAction("Students");
            }
            catch (Exception e)
            {
                return Content("Error: " + e.Message);
            }
        }

         //===================================================================
       
        //================================================================DeactiveUser=============
        //You do NOT want to delete the user’s courses or payments when deleting the user.
        //You also do NOT want to connect ApplicationUser ↔ UserCourse with cascade delete.
        public async Task<bool> DeactivateUserAsync(string userId)
        {
           
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            // Mark user as inactive
            user.IsActive = false;

            // Mark user as not paid
            user.IsPaid = false;

            // Optional: clear payment date
            user.PaymentDate = null;

            await _context.SaveChangesAsync();
            return true;
           
        }
        //===================================================================================
        //🧩 3. If You Want to Fully Delete the User(But Keep Courses & Payments)
        public async Task<bool> DeleteUserAsync(string userId)
        {

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            // Only delete the user
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
            return true;
        }


    }

}


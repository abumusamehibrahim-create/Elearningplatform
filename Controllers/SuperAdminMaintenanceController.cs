using ELearningPlatform.Data;
using ELearningPlatform.Models;
using ELearningPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELearningPlatform.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FileCleanupService _cleanup;
        public SuperAdminDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager , FileCleanupService cleanup)
        {
            _context = context;
            _userManager = userManager;
            _cleanup = cleanup;
        }
        public IActionResult CleanOrphanFiles()
        {
            _cleanup.CleanupOrphanVideos();
            _cleanup.CleanupOrphanWorksheetFiles();

            TempData["Success"] = "All orphan video and worksheet files have been cleaned successfully.";
            return RedirectToAction("Index");
        }
        // ============================
        // HELPERS
        // ============================

        private async Task<ApplicationUser?> GetSuperAdminAsync()
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == "ABUHMAM84");
        }

        private async Task<bool> CheckSuperAdminPasswordAsync(string password)
        {
            var superAdmin = await GetSuperAdminAsync();
            if (superAdmin == null) return false;

            return await _userManager.CheckPasswordAsync(superAdmin, password);
        }

        private async Task LogActivityAsync(string action, string details)
        {
                           var superAdmin = await GetSuperAdminAsync();
                var name = superAdmin?.UserName ?? "Unknown SuperAdmin";

                var log = new ActivityLog
                {
                    Action = action,
                    AdminName = name,
                    Details = details,
                    Date = DateTime.UtcNow
                };

                _context.Set<ActivityLog>().Add(log);
                await _context.SaveChangesAsync();
            
        }

        private async Task BackupSnapshotAsync(string label)
        {
            var usersCount = await _context.Users.CountAsync();
            var paymentsCount = await _context.Payments.CountAsync();
            var coursesCount = await _context.Courses.CountAsync();
            var videosCount = await _context.Set<Video>().CountAsync();
            var logsCount = await _context.Set<ActivityLog>().CountAsync();

            var details =
                $"Backup before: {label} | Users={usersCount}, Payments={paymentsCount}, Courses={coursesCount}, Videos={videosCount}, Logs={logsCount}";

            await LogActivityAsync("BACKUP_SNAPSHOT", details);
        }

        // ============================
        // DELETE USERS (EXCEPT SUPERADMIN)
        // ============================

        [HttpPost]
        public async Task<IActionResult> DeleteUsers([FromForm] string password)
        {
            try
            {
                if (!await CheckSuperAdminPasswordAsync(password))
                    return Json(new { success = false, message = "❌ Invalid SuperAdmin password. Operation cancelled." });

                await BackupSnapshotAsync("DeleteUsers");

                var superAdmin = await GetSuperAdminAsync();
                if (superAdmin == null)
                    return Json(new { success = false, message = "❌ SuperAdmin not found. Aborting." });

                var users = _context.Users.Where(u => u.Id != superAdmin.Id).ToList();
                var userIds = users.Select(u => u.Id).ToList();

                _context.Payments.RemoveRange(_context.Payments.Where(p => userIds.Contains(p.UserId)));
                _context.Set<PageVisitLog>().RemoveRange(_context.Set<PageVisitLog>().Where(p => userIds.Contains(p.UserId)));
                _context.Set<UserCourse>().RemoveRange(_context.Set<UserCourse>().Where(uc => userIds.Contains(uc.UserId)));

                _context.Users.RemoveRange(users);
                await _context.SaveChangesAsync();

                await LogActivityAsync("DELETE_USERS", "Deleted all users except SuperAdmin.");

                return Json(new { success = true, message = "🧹 All users deleted except SuperAdmin." });
            }
            catch (DbUpdateException dbEx)
            {
                await LogActivityAsync("DB_ERROR", $"Database error: {dbEx.Message}");
                return Json(new { success = false, message = "❌ Database error occurred. Operation cancelled." });
            }
            catch (Exception ex)
            {
                await LogActivityAsync("GENERAL_ERROR", $"Exception: {ex.Message}");
                return Json(new { success = false, message = "❌ Unexpected error occurred. Operation cancelled." });
            }



        }

        // ============================
        // DELETE PAYMENTS
        // ============================

        [HttpPost]
        public async Task<IActionResult> DeletePayments([FromForm] string password)
        {
            try { 
            if (!await CheckSuperAdminPasswordAsync(password))
                return Json(new { success = false, message = "❌ Invalid SuperAdmin password. Operation cancelled." });

            await BackupSnapshotAsync("DeletePayments");

            _context.Payments.RemoveRange(_context.Payments);
            await _context.SaveChangesAsync();

            await LogActivityAsync("DELETE_PAYMENTS", "Deleted all payments.");

            return Json(new { success = true, message = "💳 All payments deleted." });
            }
            catch (DbUpdateException dbEx)
            {
                await LogActivityAsync("DB_ERROR", $"Database error: {dbEx.Message}");
                return Json(new { success = false, message = "❌ Database error occurred. Operation cancelled." });
            }
            catch (Exception ex)
            {
                await LogActivityAsync("GENERAL_ERROR", $"Exception: {ex.Message}");
                return Json(new { success = false, message = "❌ Unexpected error occurred. Operation cancelled." });
            }
        }

        // ============================
        // DELETE COURSES + VIDEOS + WORKSHEETS
        // ============================

        [HttpPost]
        public async Task<IActionResult> DeleteCourses([FromForm] string password)
        {
            try{if (!await CheckSuperAdminPasswordAsync(password))
                return Json(new { success = false, message = "❌ Invalid SuperAdmin password. Operation cancelled." });

            await BackupSnapshotAsync("DeleteCourses");

            _context.Set<WorksheetItem>().RemoveRange(_context.Set<WorksheetItem>());
            _context.Set<WorksheetFile>().RemoveRange(_context.Set<WorksheetFile>());
            _context.Set<Video>().RemoveRange(_context.Set<Video>());
            _context.Set<UserCourse>().RemoveRange(_context.Set<UserCourse>());
            _context.Courses.RemoveRange(_context.Courses);

            await _context.SaveChangesAsync();

            await LogActivityAsync("DELETE_COURSES", "Deleted all courses, videos, worksheets, and user-course relations.");

            return Json(new { success = true, message = "📚 All courses, videos, worksheets deleted." });
            }
            catch (DbUpdateException dbEx)
            {
                await LogActivityAsync("DB_ERROR", $"Database error: {dbEx.Message}");
                return Json(new { success = false, message = "❌ Database error occurred. Operation cancelled." });
            }
            catch (Exception ex)
            {
                await LogActivityAsync("GENERAL_ERROR", $"Exception: {ex.Message}");
                return Json(new { success = false, message = "❌ Unexpected error occurred. Operation cancelled." });
            }
        }

        // ============================
        // DELETE LOGS
        // ============================

        [HttpPost]
        public async Task<IActionResult> DeleteLogs([FromForm] string password)
        {
           try{ if (!await CheckSuperAdminPasswordAsync(password))
                return Json(new { success = false, message = "❌ Invalid SuperAdmin password. Operation cancelled." });

            await BackupSnapshotAsync("DeleteLogs");

            _context.Set<ActivityLog>().RemoveRange(_context.Set<ActivityLog>());
            _context.Set<PageVisitLog>().RemoveRange(_context.Set<PageVisitLog>());
            await _context.SaveChangesAsync();

            await LogActivityAsync("DELETE_LOGS", "Deleted all logs.");

            return Json(new { success = true, message = "📜 All logs deleted." });
            }
            catch (DbUpdateException dbEx)
            {
                await LogActivityAsync("DB_ERROR", $"Database error: {dbEx.Message}");
                return Json(new { success = false, message = "❌ Database error occurred. Operation cancelled." });
            }
            catch (Exception ex)
            {
                await LogActivityAsync("GENERAL_ERROR", $"Exception: {ex.Message}");
                return Json(new { success = false, message = "❌ Unexpected error occurred. Operation cancelled." });
            }
        }

        // ============================
        // DELETE MENU ITEMS
        // ============================
        //why this not require
       /* [HttpPost]
        public async Task<IActionResult> DeleteMenu([FromForm] string password)
        {
            if (!await CheckSuperAdminPasswordAsync(password))
                return Json(new { success = false, message = "❌ Invalid SuperAdmin password. Operation cancelled." });

            await BackupSnapshotAsync("DeleteMenu");

            _context.Set<MenuItem>().RemoveRange(_context.Set<MenuItem>());
            await _context.SaveChangesAsync();

            await LogActivityAsync("DELETE_MENU", "Deleted all menu items.");

            return Json(new { success = true, message = "📂 All menu items deleted." });
        }*/

        // ============================
        // DELETE REVIEWS
        // ============================

        [HttpPost]
        public async Task<IActionResult> DeleteReviews([FromForm] string password)
        {
           try{ if (!await CheckSuperAdminPasswordAsync(password))
                return Json(new { success = false, message = "❌ Invalid SuperAdmin password. Operation cancelled." });

            await BackupSnapshotAsync("DeleteReviews");

            _context.Set<Review>().RemoveRange(_context.Set<Review>());
            await _context.SaveChangesAsync();

            await LogActivityAsync("DELETE_REVIEWS", "Deleted all reviews.");

            return Json(new { success = true, message = "⭐ All reviews deleted." });
            }
            catch (DbUpdateException dbEx)
            {
                await LogActivityAsync("DB_ERROR", $"Database error: {dbEx.Message}");
                return Json(new { success = false, message = "❌ Database error occurred. Operation cancelled." });
            }
            catch (Exception ex)
            {
                await LogActivityAsync("GENERAL_ERROR", $"Exception: {ex.Message}");
                return Json(new { success = false, message = "❌ Unexpected error occurred. Operation cancelled." });
            }
        }

        // ============================
        // DELETE GALLERY
        // ============================

        [HttpPost]
        public async Task<IActionResult> DeleteGallery([FromForm] string password)
        {
            try
            {
                if (!await CheckSuperAdminPasswordAsync(password))
                    return Json(new { success = false, message = "❌ Invalid SuperAdmin password. Operation cancelled." });

                await BackupSnapshotAsync("DeleteGallery");

                _context.Set<GalleryImage>().RemoveRange(_context.Set<GalleryImage>());
                await _context.SaveChangesAsync();

                await LogActivityAsync("DELETE_GALLERY", "Deleted all gallery images.");

                return Json(new { success = true, message = "🖼️ All gallery images deleted." });
            
             }
            catch (DbUpdateException dbEx)
            {
                await LogActivityAsync("DB_ERROR", $"Database error: {dbEx.Message}");
                return Json(new { success = false, message = "❌ Database error occurred. Operation cancelled." });
            }
            catch (Exception ex)
            {
                await LogActivityAsync("GENERAL_ERROR", $"Exception: {ex.Message}");
return Json(new { success = false, message = "❌ Unexpected error occurred. Operation cancelled." });
            }
        }

        // ============================
        // DELETE TEAM MEMBERS
        // ============================

        [HttpPost]
        public async Task<IActionResult> DeleteTeam([FromForm] string password)
        {
            try{if (!await CheckSuperAdminPasswordAsync(password))
                return Json(new { success = false, message = "❌ Invalid SuperAdmin password. Operation cancelled." });

            await BackupSnapshotAsync("DeleteTeam");

            _context.Set<TeamMember>().RemoveRange(_context.Set<TeamMember>());
            await _context.SaveChangesAsync();

            await LogActivityAsync("DELETE_TEAM", "Deleted all team members.");

            return Json(new { success = true, message = "👥 All team members deleted." });
            }
            catch (DbUpdateException dbEx)
            {
                await LogActivityAsync("DB_ERROR", $"Database error: {dbEx.Message}");
                return Json(new { success = false, message = "❌ Database error occurred. Operation cancelled." });
            }
            catch (Exception ex)
            {
                await LogActivityAsync("GENERAL_ERROR", $"Exception: {ex.Message}");
                return Json(new { success = false, message = "❌ Unexpected error occurred. Operation cancelled." });
            }
        }

        // ============================
        // DELETE PAGE CONTENT
        // ============================

        [HttpPost]
        public async Task<IActionResult> DeletePageContent([FromForm] string password)
        {
           try{ if (!await CheckSuperAdminPasswordAsync(password))
                return Json(new { success = false, message = "❌ Invalid SuperAdmin password. Operation cancelled." });

            await BackupSnapshotAsync("DeletePageContent");

            _context.Set<PageContent>().RemoveRange(_context.Set<PageContent>());
            await _context.SaveChangesAsync();

            await LogActivityAsync("DELETE_PAGE_CONTENT", "Deleted all page content.");

            return Json(new { success = true, message = "📄 All page content deleted." });
            }
            catch (DbUpdateException dbEx)
            {
                await LogActivityAsync("DB_ERROR", $"Database error: {dbEx.Message}");
                return Json(new { success = false, message = "❌ Database error occurred. Operation cancelled." });
            }
            catch (Exception ex)
            {
                await LogActivityAsync("GENERAL_ERROR", $"Exception: {ex.Message}");
                return Json(new { success = false, message = "❌ Unexpected error occurred. Operation cancelled." });
            }
        }

        // ============================
        // DELETE FAQ
        // ============================

        [HttpPost]
        public async Task<IActionResult> DeleteFaq([FromForm] string password)
        {
            try
            {
                if (!await CheckSuperAdminPasswordAsync(password))
                    return Json(new { success = false, message = "❌ Invalid SuperAdmin password. Operation cancelled." });

                await BackupSnapshotAsync("DeleteFaq");

                _context.Set<FAQItem>().RemoveRange(_context.Set<FAQItem>());
                await _context.SaveChangesAsync();

                await LogActivityAsync("DELETE_FAQ", "Deleted all FAQ items.");

                return Json(new { success = true, message = "❓ All FAQ items deleted." });
            }
            catch (DbUpdateException dbEx)
            {
                await LogActivityAsync("DB_ERROR", $"Database error: {dbEx.Message}");
                return Json(new { success = false, message = "❌ Database error occurred. Operation cancelled." });
            }
            catch (Exception ex)
            {
                await LogActivityAsync("GENERAL_ERROR", $"Exception: {ex.Message}");
                return Json(new { success = false, message = "❌ Unexpected error occurred. Operation cancelled." });
            }
        }

        // ============================
        // FACTORY RESET
        // ============================

        [HttpPost]
        public async Task<IActionResult> FactoryReset([FromForm] string password)
        {
            try{if (!await CheckSuperAdminPasswordAsync(password))
                return Json(new { success = false, message = "❌ Invalid SuperAdmin password. Factory reset cancelled." });

            await BackupSnapshotAsync("FactoryReset");

            var superAdmin = await GetSuperAdminAsync();
            if (superAdmin == null)
                return Json(new { success = false, message = "❌ SuperAdmin missing. Reset aborted." });

            _context.Set<ActivityLog>().RemoveRange(_context.Set<ActivityLog>());
            _context.Set<PageVisitLog>().RemoveRange(_context.Set<PageVisitLog>());
            _context.Set<FAQItem>().RemoveRange(_context.Set<FAQItem>());
            _context.Set<PageContent>().RemoveRange(_context.Set<PageContent>());
          //  _context.Set<MenuItem>().RemoveRange(_context.Set<MenuItem>());
            _context.Set<Review>().RemoveRange(_context.Set<Review>());
            _context.Set<GalleryImage>().RemoveRange(_context.Set<GalleryImage>());
            _context.Set<TeamMember>().RemoveRange(_context.Set<TeamMember>());
            _context.Set<WorksheetItem>().RemoveRange(_context.Set<WorksheetItem>());
            _context.Set<WorksheetFile>().RemoveRange(_context.Set<WorksheetFile>());
            _context.Set<Video>().RemoveRange(_context.Set<Video>());
            _context.Set<UserCourse>().RemoveRange(_context.Set<UserCourse>());
            _context.Courses.RemoveRange(_context.Courses);
            _context.Payments.RemoveRange(_context.Payments);

            _context.Users.RemoveRange(_context.Users.Where(u => u.Id != superAdmin.Id));

            await _context.SaveChangesAsync();

            await LogActivityAsync("FACTORY_RESET", "Full factory reset executed. All data cleared except SuperAdmin.");

            return Json(new { success = true, message = "🔥 FACTORY RESET COMPLETE. System returned to clean state." });
            }
            catch (DbUpdateException dbEx)
            {
                await LogActivityAsync("DB_ERROR", $"Database error: {dbEx.Message}");
                return Json(new { success = false, message = "❌ Database error occurred. Operation cancelled." });
            }
            catch (Exception ex)
            {
                await LogActivityAsync("GENERAL_ERROR", $"Exception: {ex.Message}");
                return Json(new { success = false, message = "❌ Unexpected error occurred. Operation cancelled." });
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        //=========================================
        [HttpPost]
        public async Task<IActionResult> BackupSnapshot([FromForm] string password)
        {
            if (!await CheckSuperAdminPasswordAsync(password))
                return Json(new { success = false, message = "❌ Invalid SuperAdmin password. Backup cancelled." });

            await BackupSnapshotAsync("Manual Backup Snapshot");

            await LogActivityAsync("BACKUP_SNAPSHOT", "Manual backup snapshot created by SuperAdmin.");

            return Json(new { success = true, message = "📦 Backup Snapshot created successfully." });
        }
      



    }

}

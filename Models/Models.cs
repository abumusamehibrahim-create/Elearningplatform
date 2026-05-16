using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELearningPlatform.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsPaid { get; set; } = false;
        public DateTime? PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public string? PlainPassword { get; set; }
        public string? SessionToken { get; set; }
        public string? LastDeviceId { get; set; }
        public string? LastIP { get; set; }
        public string? LastCountry { get; set; }
        public bool IsActive { get; set; } = true;


    }
    public class UserCourse
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CourseId { get; set; }

        public ApplicationUser User { get; set; }
        public Course Course { get; set; }
    }
    public class Payment
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? CourseId { get; set; }
        public decimal Amount { get; set; }
        public string? StripePaymentId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public ApplicationUser? User { get; set; }
        public Course? Course { get; set; }
    }
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Video> Videos { get; set; } = new List<Video>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public class Video
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsFree { get; set; } = false;
        public int OrderNumber { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Course? Course { get; set; }
        public List<WorksheetFile> WorksheetFiles { get; set; }
        public List<WorksheetItem> WorksheetItems { get; set; }
        // Bunny Stream (??????? - ??????? ??????)
        public bool UseBunny { get; set; } = false;
        public string? BunnyVideoId { get; set; }
        public string? BunnyLibraryId { get; set; }
        public string? BunnyCDNHostname { get; set; }

        [NotMapped]
        public string StreamUrl => $"/Video/Stream?videoId={Id}";
        // ???? ??????? ?????? (???? ?? Bunny)

        /*
         public string StreamUrl =>
           UseBunny && !string.IsNullOrEmpty(BunnyVideoId)
               ? $"https://{BunnyCDNHostname}/{BunnyLibraryId}/{BunnyVideoId}/play.m3u8"
                : $"/uploads/videos/{FileName}";*/

    }

    
   
    public class ActivityLog
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string AdminName { get; set; }
        public string Details { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
    public class PageVisitLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string PageUrl { get; set; }
        public string PageTitle { get; set; }
        public DateTime VisitTime { get; set; } = DateTime.UtcNow;
    }
    public class WorksheetFile
    {
        public int Id { get; set; }
        public int VideoId { get; set; }
        public string FileName { get; set; } // ??? ?????
        public string FilePath { get; set; } // / /ProtectedWorksheetFile/filename.pdf???? ?????
        public Video Video { get; set; }
        public bool AllowDownload { get; set; }
        public string? FileUrl { get; set; }   // ADD THIS
    }
    public class WorksheetItem
    {
        public int Id { get; set; }

        // ????? ?? ???????
        public int VideoId { get; set; }

        // ?? ??????
        public string Question { get; set; }
        public string? Answer { get; set; }  // ← أضف هذاهذا يجعل العمود في قاعدة البيانات NULLABLE.

        // ????? ?????
        public Video Video { get; set; }
    }
    public class WorksheetViewModel
    {
        public int VideoId { get; set; }
        public string Title { get; set; }

        public List<WorksheetFile> WorksheetFiles { get; set; }
        public List<WorksheetItem> WorksheetItems { get; set; }
        //Add-Migration WorksheetViewModel
        //Update-Database

    }
    public class MenuItem
    {
        public int Id { get; set; }

        public string Name { get; set; }          // Example: "About", "Blog"
        public string Url { get; set; }           // Example: "/About"
        public int Order { get; set; }            // Sorting
        public bool IsVisible { get; set; }       // Show/Hide
        public string? Icon { get; set; }
        public int? ParentId { get; set; } // For submenu
        public MenuItem Parent { get; set; }

        public string? Roles { get; set; } // comma-separated roles: "Admin,Teacher"
    }
    public class Review
    {
        public int Id { get; set; }
        public string? StudentName { get; set; }
        public string ?Comment { get; set; }
        public int Rating { get; set; }
        public string? ImageUrl { get; set; }
    }
    public class GalleryImage
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ?ImageUrl { get; set; }
    }
    public class TeamMember
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string ImageUrl { get; set; }
    }
    public class PageContent
    {
        public int Id { get; set; }
        public string? PageName { get; set; }
        public string? Section { get; set; }
        public string? Content { get; set; }
    }
    public class FAQItem
    {
        public int Id { get; set; }

        [Required]
        public string Question { get; set; }   // الطالب يكتبها

        public string? Answer { get; set; }    // الأدمن يكتبها لاحقًا

        public string? StudentName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsVisible { get; set; } = true;
    }

    public class License
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string Domain { get; set; }
        public string LicenseKey { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsActive { get; set; }

        // Device activation system
        public int MaxDevices { get; set; } = 1;
        public int UsedDevices { get; set; } = 0;

        // Relationship with devices
        public List<LicenseDevice> Devices { get; set; }
    }

    public class ClientSetting
    {
        public int Id { get; set; }
        public string? ClientName { get; set; }
        public string? ThemeColor { get; set; }
        public string? LogoPath { get; set; }
        public string? ThemeName { get; set; }

        // public IFormFile LogoFile { get; set; }
    }
    public class LicenseDevice
    {
        public int Id { get; set; }

        // Foreign Key to License
        public int LicenseId { get; set; }
        public License License { get; set; }

        // Unique device identifier
        public string DeviceId { get; set; }

        // Device name (optional)
        public string DeviceName { get; set; }

        // OS info (optional)
        public string OSVersion { get; set; }

        // When activated
        public DateTime ActivatedOn { get; set; } = DateTime.UtcNow;

        // For offline activation token
        public string OfflineToken { get; set; }
    }


}

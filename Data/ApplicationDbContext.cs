using ELearningPlatform.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection.Emit;

namespace ELearningPlatform.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ApplicationDbContext dbBase;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<PageVisitLog> PageVisitLogs { get; set; }
        public DbSet<WorksheetFile> WorksheetFiles { get; set; }
        public DbSet<WorksheetItem> WorksheetItems { get; set; }
       public DbSet<WorksheetItem> WorksheetViewModel { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<GalleryImage> GalleryImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<PageContent> PageContents { get; set; }
        public DbSet<FAQItem> FAQItems { get; set; }
        public DbSet<License> Licenses { get; set; }
        public DbSet<ClientSetting> ClientSettings { get; set; }
        public DbSet<LicenseDevice> LicenseDevices {  get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);

            builder.Entity<PageContent>()
                .ToTable("PageContents"); // ربط صريح
          //  base.OnModelCreating(builder);
           // options.UseSqlServer(connectionString, o => o.CommandTimeout(180));

            builder.Entity<Course>().Property(c => c.Price).HasColumnType("decimal(18,2)");
            builder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(18,2)");
            //=================
            // dont delet payemnt when you delete the courses
            builder.Entity<Payment>()
    .HasOne(p => p.Course)
    .WithMany(c => c.Payments)
    .HasForeignKey(p => p.CourseId)
    .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //========================
            // Seed admin course
            builder.Entity<Course>().HasData(
                new Course
                {
                    Id = 1,
                    Title = "دورة في مادة الرياضيات الشاملة",
                    Description = "تعلم بناء نفسك بشكل احترافية باستخدام الفديوهات = مستقبلك بين يديك",
                    Price = 99.00m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}

using ElearningPlatform.Data;
//using ElearningPlatform.modles;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ElearningPlatform.Data
{
   /* public class ApplicationDbContext : IdentityDbContext<>Application
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<VideoProgress> VideoProgresses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Enrollment: one user per course
            builder.Entity<Enrollment>()
                .HasIndex(e => new { e.UserId, e.CourseId })
                .IsUnique();

            // VideoProgress: one record per user per video
            builder.Entity<VideoProgress>()
                .HasIndex(vp => new { vp.UserId, vp.VideoId })
                .IsUnique();

            // Seed initial admin + sample data
            builder.Entity<Course>().HasData(
                new Course
                {
                    Id = 1,
                    Title = "????? ?? ??????? ?? C#",
                    Description = "???? ??????? ??????? ?? ??? C# ?? ????? ??? ????????",
                    Price = 99.99m,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Course
                {
                    Id = 2,
                    Title = "ASP.NET Core MVC ?????????",
                    Description = "???? ??????? ??? ???????? ???????? ASP.NET Core",
                    Price = 149.99m,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );
        }
    }*/
}
namespace ELearningPlatform.Services
{
    using ELearningPlatform.Data;
    using Microsoft.EntityFrameworkCore;

    public class FileCleanupService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public FileCleanupService(IWebHostEnvironment env, ApplicationDbContext context)
        {
            _env = env;
            _context = context;
        }

        public void CleanupOrphanVideos()
        {
            var folder = Path.Combine(_env.ContentRootPath, "ProtectedVideos");
            var files = Directory.GetFiles(folder);

            var dbFiles = _context.Videos.Select(v => v.FileName).ToList();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);

                if (!dbFiles.Contains(fileName))
                {
                    System.IO.File.Delete(file);
                }
            }
        }

        public void CleanupOrphanWorksheetFiles()
        {
            var folder = Path.Combine(_env.ContentRootPath, "ProtectedWorksheetFile");
            var files = Directory.GetFiles(folder);

            var dbFiles = _context.WorksheetFiles.Select(v => v.FileName).ToList();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);

                if (!dbFiles.Contains(fileName))
                {
                    System.IO.File.Delete(file);
                }
            }
        }
    }

}

using ELearningPlatform.Data;
using ELearningPlatform.Models;

namespace ELearningPlatform.Services
{
    public class VideoAccessService
    {
        private readonly ApplicationDbContext _db;

        public VideoAccessService(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool UserHasAccess(string userId, int courseId)
        {
            return _db.Payments.Any(p =>
                p.UserId == userId &&
                p.CourseId == courseId &&
                p.Status == "Completed");
        }

        public bool UserHasAccessToVideo(string userId, int videoId)
        {
            var video = _db.Videos.Find(videoId);
            if (video == null) return false;
            if (video.IsFree) return true;
            return UserHasAccess(userId, video.CourseId);
        }
    }
}

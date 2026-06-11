namespace ELearningPlatform
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System.Text;

    public class BunnyVideoManager2
    {
        private readonly HttpClient _http;
        private readonly string _libraryId;
        private readonly string _apiKey;
        private readonly IConfiguration _config;
        public BunnyVideoManager2(IConfiguration config)
        {
            _http = new HttpClient();
            _config = config;
            // VALUES FROM YOUR BUNNY ACCOUNT
            _libraryId = config["BUNNY_STREAM_LIBRARY_ID"];
            _apiKey = config["BUNNY_STREAM_API_KEY"];
        }

        // ============================================================
        // ⭐ 1) Create Video (Get Video ID)
        // ============================================================
        public async Task<string> CreateVideoAsync(string title)
        {
            var url = $"https://video.bunnycdn.com/library/{_libraryId}/videos";

            var body = new { title = title };
            var json = JsonConvert.SerializeObject(body);

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("AccessKey", _apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(responseJson);

            return data.guid; // Video ID
        }

        // ============================================================
        // ⭐ 2) Upload Video File
        // ============================================================
        public async Task UploadVideoFileAsync(string videoId, IFormFile file)
        {
            var url = $"https://video.bunnycdn.com/library/{_libraryId}/videos/{videoId}";

            using var stream = file.OpenReadStream();
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Add("AccessKey", _apiKey);
            request.Content = content;

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        // ============================================================
        // ⭐ 3) Get Video Playback URL
        // ============================================================
        public string GetVideoUrl(string videoId)
        {
            return $"https://iframe.mediadelivery.net/embed/{_libraryId}/{videoId}";
        }

        // ============================================================
        // ⭐ 4) Delete Video
        // ============================================================
        public async Task DeleteVideoAsync(string videoId)
        {
            var url = $"https://video.bunnycdn.com/library/{_libraryId}/videos/{videoId}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Add("AccessKey", _apiKey);

            await _http.SendAsync(request);
        }
        //security function for video
        public string GenerateSignedUrl(string videoId)
        {
            string securityKey = _config["BUNNY_STREAM_SECURITY_KEY"];
            string libraryId = _config["BUNNY_STREAM_LIBRARY_ID"];
            string hostname = "vz-8910d323-df2.b-cdn.net"; // CDN الخاص بك

            long expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60;

            string path = $"/{libraryId}/{videoId}/play.m3u8";

            string hashInput = securityKey + path + expires;

            string token = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(hashInput)
                )
            ).ToLower();

            return $"https://{hostname}{path}?token={token}&expires={expires}";
        }

        //===========================✔ Watch Action مع حماية Token=scurity for link copy====


    }
}
/*
 private readonly BunnyVideoManager _bunny;

public AdminController(IConfiguration config)
{
    _bunny = new BunnyVideoManager(config);
}

public async Task<IActionResult> UploadLessonVideo(IFormFile videoFile, string title)
{
    // 1) Create video ID
    string videoId = await _bunny.CreateVideoAsync(title);

    // 2) Upload file
    await _bunny.UploadVideoFileAsync(videoId, videoFile);

    // 3) Get playback URL
    string videoUrl = _bunny.GetVideoUrl(videoId);

    // 4) Save to DB
    lesson.VideoId = videoId;
    lesson.VideoUrl = videoUrl;
    _context.SaveChanges();

    return RedirectToAction("Lessons");
}

 
 
 
 
 
 
 */
namespace ELearningPlatform
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System.Text;
    using System.Security.Cryptography;

    public class BunnyVideoManager2
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly string _libraryId;
        private readonly string _apiKey;

        public BunnyVideoManager2(IConfiguration config)
        {
            _config = config;
            _http = new HttpClient();

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

            return data.guid;
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
        // ⭐ 3) Generate Signed URL (HLS)
        // ============================================================
        /*   public string GenerateSignedUrl(string videoId)
           {
               string securityKey = _config["BUNNY_CDN_TOKEN_KEY"];
               string cdnHostname = _config["BUNNY_CDN_HOSTNAME"];

               long expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600;

               // IMPORTANT: Directory path, not playlist.m3u8
               string path = $"/{videoId}/";

               // HMAC-SHA256
               var keyBytes = Encoding.UTF8.GetBytes(securityKey);
               var messageBytes = Encoding.UTF8.GetBytes($"{path}{expires}");

               using var hmac = new HMACSHA256(keyBytes);
               var hashBytes = hmac.ComputeHash(messageBytes);

               // Base64URL encoding
               string base64Url = Convert.ToBase64String(hashBytes)
                   .Replace("+", "-")
                   .Replace("/", "_")
                   .Replace("=", "");

               string token = $"HS256-{base64Url}";

               return $"https://{cdnHostname}/{videoId}/playlist.m3u8?bcdn_token={token}&expires={expires}";
           }
        */
        public string GenerateSignedUrlBasic(string videoId)
        {
            // هذا هو الـ Token Authentication Key من Bunny
            string securityKey = _config["BUNNY_STREAM_TOKEN_KEY"];

            // هذا هو الـ Pull Zone الصحيح
            string hostname = _config["BUNNY_STREAM_PULLZONE"];
            // مثال: vz-c79f935b-846.b-cdn.net

            long expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600;

            // مسار ملف الـ HLS داخل Bunny Stream
            string path = $"/{videoId}/playlist.m3u8";

            // صيغة التوقيع المطلوبة من Bunny
            string hashableBase = securityKey + path + expires;

            using var md5 = System.Security.Cryptography.MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(hashableBase));

            // Base64 URL Safe
            string token = Convert.ToBase64String(hashBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            return $"https://{hostname}{path}?token={token}&expires={expires}";
        }

        public string GenerateSignedUrl1(string videoId)
{
    // مفتاح التوكن من Bunny (Token Authentication Key)
    string securityKey = _config["BUNNY_STREAM_TOKEN_KEY"];

    // هذا هو الـ Pull Zone الصحيح الخاص بمكتبة الفيديو
    string hostname = _config["BUNNY_STREAM_PULLZONE"]; 
    // مثال: elearningvideos.b-cdn.net

    long expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600;

    // مسار ملف الـ HLS داخل Bunny Stream
    string path = $"/{videoId}/playlist.m3u8";

    // صيغة التوقيع المطلوبة من Bunny
    string hashableBase = securityKey + path + expires;

    using var md5 = System.Security.Cryptography.MD5.Create();
    var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(hashableBase));

    // Base64 URL Safe
    string token = Convert.ToBase64String(hashBytes)
        .Replace("+", "-")
        .Replace("/", "_")
        .Replace("=", "");

    return $"https://{hostname}{path}?token={token}&expires={expires}";
}

        public string GenerateSignedUrl(string videoId)
          {
              string securityKey = _config["BUNNY_STREAM_SECURITY_KEY"];
              //string securityKey = _config["BUNNY_CDN_TOKEN_KEY"];

              string cdn = _config["BUNNY_STREAM_CDN"];

              long expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600;

              string path = $"/{videoId}/playlist.m3u8";


              string hashInput = $"{securityKey}{path}{expires}";

              using var sha = SHA256.Create();
              var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
              string token = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();



              return $"https://{cdn}{path}?token={token}&expires={expires}";
          }

          

        // ============================================================
        // ⭐ 4) Delete Video
        // ============================================================
        public async Task DeleteVideoAsync(string videoId)
        {
            var url = $"https://video.bunnycdn.com/library/{_libraryId}/videos/{videoId}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Add("AccessKey", _apiKey);

            var response = await _http.SendAsync(request);

            string result = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Bunny Delete Response: " + result);

            response.EnsureSuccessStatusCode();
        }

        public string GetVideoUrl(string videoId)
        {
            return GenerateSignedUrl(videoId);
        }
        private async Task<string> Bunny_CreateVideo(string title)
        {
            string libraryId = _config["BUNNY_STREAM_LIBRARY_ID"];
            string apiKey = _config["BUNNY_STREAM_API_KEY"];

            var url = $"https://video.bunnycdn.com/library/{libraryId}/videos";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("AccessKey", apiKey);

            var response = await client.PostAsync(url, null);
            var json = await response.Content.ReadAsStringAsync();

            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            return data.guid;
        }

        private async Task Bunny_UploadVideo(string videoId, IFormFile file)
        {
            string libraryId = _config["BUNNY_STREAM_LIBRARY_ID"];
            string apiKey = _config["BUNNY_STREAM_API_KEY"];

            var url = $"https://video.bunnycdn.com/library/{libraryId}/videos/{videoId}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("AccessKey", apiKey);

            using var stream = file.OpenReadStream();
            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StreamContent(stream)
            };

            request.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }


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
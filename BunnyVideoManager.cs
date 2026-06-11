namespace ELearningPlatform
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Microsoft.AspNetCore.Http;

    public class BunnyVideoManager
    {
        private readonly HttpClient _http;
        private readonly string _storageZone;
        private readonly string _apiKey;
        private readonly string _region;
        private readonly string _cdnBaseUrl;
        private readonly string _storageBaseUrl;

        public BunnyVideoManager(IConfiguration config)
        {
            _http = new HttpClient();

            _storageZone = config["BUNNY_STORAGE_ZONE"];
            _apiKey = config["BUNNY_API_KEY"];
            _region = config["BUNNY_STORAGE_REGION"]; // ny, de, sg...
            _cdnBaseUrl = config["BUNNY_CDN_BASE_URL"];   // https://yourzone.b-cdn.net

            // مثال: https://ny.storage.bunnycdn.com/yourzone
            _storageBaseUrl = $"https://{_region}.storage.bunnycdn.com/{_storageZone}";
        }

        // ============================================================
        // ⭐ Upload Video
        // ============================================================
        public async Task<string> UploadVideoAsync(IFormFile file, string fileName)
        {
            var url = $"{_storageBaseUrl}/videos/{fileName}";

            using var content = new StreamContent(file.OpenReadStream());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Add("AccessKey", _apiKey);
            request.Content = content;

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return $"{_cdnBaseUrl}/videos/{fileName}";
        }

        // ============================================================
        // ⭐ Upload Worksheet
        // ============================================================
        public async Task<string> UploadWorksheetAsync(IFormFile file, string fileName)
        {
            var url = $"{_storageBaseUrl}/worksheets/{fileName}";

            using var content = new StreamContent(file.OpenReadStream());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Add("AccessKey", _apiKey);
            request.Content = content;

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return $"{_cdnBaseUrl}/worksheets/{fileName}";
        }

        // ============================================================
        // ⭐ Delete Video
        // ============================================================
        public async Task DeleteVideoAsync(string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
                return;

            string fileName = ExtractFileName(videoUrl);
            var url = $"{_storageBaseUrl}/videos/{fileName}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Add("AccessKey", _apiKey);

            await _http.SendAsync(request);
        }

        // ============================================================
        // ⭐ Delete Worksheet
        // ============================================================
        public async Task DeleteWorksheetAsync(string worksheetUrl)
        {
            if (string.IsNullOrWhiteSpace(worksheetUrl))
                return;

            string fileName = ExtractFileName(worksheetUrl);
            var url = $"{_storageBaseUrl}/worksheets/{fileName}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Add("AccessKey", _apiKey);

            await _http.SendAsync(request);
        }

        // ============================================================
        // ⭐ Update Video
        // ============================================================
        public async Task<string> UpdateVideoAsync(string oldUrl, IFormFile newFile, string newFileName)
        {
            await DeleteVideoAsync(oldUrl);
            return await UploadVideoAsync(newFile, newFileName);
        }

        // ============================================================
        // ⭐ Stream Video
        // ============================================================
        public async Task<Stream> StreamVideoAsync(string videoUrl)
        {
            string fileName = ExtractFileName(videoUrl);
            var url = $"{_cdnBaseUrl}/videos/{fileName}";

            return await _http.GetStreamAsync(url);
        }

        // ============================================================
        // ⭐ Stream Worksheet
        // ============================================================
        public async Task<Stream> StreamWorksheetAsync(string worksheetUrl)
        {
            string fileName = ExtractFileName(worksheetUrl);
            var url = $"{_cdnBaseUrl}/worksheets/{fileName}";

            return await _http.GetStreamAsync(url);
        }

        // ============================================================
        // ⭐ GenerateWorksheetSasUrl (Bunny version)
        // ============================================================
        public string GenerateWorksheetSasUrl(string fileName, int seconds = 30)
        {
            // Bunny لا يدعم SAS مثل Azure
            // لكن لو عندك Signed Token نضيفه هنا
            return $"{_cdnBaseUrl}/worksheets/{fileName}";
        }

        // ============================================================
        // ⭐ Helper
        // ============================================================
        private string ExtractFileName(string pathOrUrl)
        {
            if (Uri.IsWellFormedUriString(pathOrUrl, UriKind.Absolute))
                return Path.GetFileName(new Uri(pathOrUrl).LocalPath);

            return Path.GetFileName(pathOrUrl);
        }
    }
}
/*
 ـ Controller (نفس أسلوب Azure)
csharp
private readonly BunnyVideoManager _videoManager;

public AdminController(ApplicationDbContext context, IWebHostEnvironment env, IConfiguration config)
{
    _context = context;
    _env = env;
    _videoManager = new BunnyVideoManager(config);
}
رفع فيديو:

csharp
string fileName = SanitizeFileName(title) + Path.GetExtension(videoFile.FileName);
string videoUrl = await _videoManager.UploadVideoAsync(videoFile, fileName);
// خزّن videoUrl في SQL

رفع Worksheet:

csharp
string wsName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
string wsUrl  = await _videoManager.UploadWorksheetAsync(file, wsName);

// خزّن wsUrl في SQL
Stream فيديو:

csharp
var stream = await _videoManager.StreamVideoAsync(video.FileName);
return File(stream, "video/mp4");
Stream Worksheet (مع Watermark مثل ما عملت):

csharp
var stream = await _videoManager.StreamWorksheetAsync(file.FileName);
// ثم نفس منطق MemoryStream + AddWatermark
 
 
 
 
 
 
 */
namespace ELearningPlatform
{
    using Microsoft.AspNetCore.Http;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;

    public class BunnyStorageManager
    {
        private readonly HttpClient _http;
        private readonly string _storageZone;
        private readonly string _apiKey;
        private readonly string _cdnBaseUrl;
        private readonly IConfiguration _config;

        public BunnyStorageManager(IConfiguration config)
        {
            _http = new HttpClient();

            _storageZone = config["BUNNY_STORAGE_ZONE"];
            _apiKey = config["BUNNY_STORAGE_API_KEY"];
            _cdnBaseUrl = config["BUNNY_STORAGE_CDN"];
            _config = config;
        }

        // ============================================================
        // ⭐ Upload Worksheet
        // ============================================================
        public async Task<string> UploadWorksheetAsync(IFormFile file, string fileName)
        {
            var url = $"https://storage.bunnycdn.com/{_storageZone}/worksheets/{fileName}";

            using var stream = file.OpenReadStream();
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Add("AccessKey", _apiKey);
            request.Content = content;

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return $"{_cdnBaseUrl}/worksheets/{fileName}";
        }

        // ============================================================
        // ⭐ Delete Worksheet
        // ============================================================
        public async Task DeleteWorksheetAsync(string fileName)
        {
            var url = $"https://storage.bunnycdn.com/{_storageZone}/worksheets/{fileName}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Add("AccessKey", _apiKey);

            await _http.SendAsync(request);
        }

        // ============================================================
        // ⭐ Generate Signed URL (Token Protected)
        // ============================================================
        public string GenerateWorksheetSignedUrl(string fileName)
        {
            string securityKey = _config["BUNNY_STORAGE_SECURITY_KEY"];
            string cdnHostname = _config["BUNNY_STORAGE_CDN"]; // بدون https

            long expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60;

            string path = $"/{_storageZone}/worksheets/{fileName}";

            string hashInput = securityKey + path + expires;

            string token = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    Encoding.UTF8.GetBytes(hashInput)
                )
            ).ToLower();

            return $"https://{cdnHostname}{path}?token={token}&expires={expires}";
        }

        public async Task<byte[]> DownloadWorksheetAsync(string fileName)
        {
            var url = $"https://storage.bunnycdn.com/{_storageZone}/worksheets/{fileName}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("AccessKey", _apiKey);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

    }
}

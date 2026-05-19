namespace ELearningPlatform
{
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;

    public class AzureVideoManager
    {
        private readonly BlobServiceClient _blobService;
        private readonly BlobContainerClient _videoContainer;
        private readonly BlobContainerClient _worksheetContainer;

        public AzureVideoManager(IConfiguration config)
        {
            // Create Azure Blob Service
            _blobService = new BlobServiceClient(config["AZURE_STORAGE_CONNECTION_STRING"]);

            // Create containers
            _videoContainer = _blobService.GetBlobContainerClient("videos");
            _worksheetContainer = _blobService.GetBlobContainerClient("worksheets");

            // Ensure containers exist
            _videoContainer.CreateIfNotExists(PublicAccessType.Blob);
            _worksheetContainer.CreateIfNotExists(PublicAccessType.Blob);
        }

        // ============================================================
        // ⭐ Upload Video
        // ============================================================
        public async Task<string> UploadVideoAsync(IFormFile file, string fileName)
        {
            var blob = _videoContainer.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream, overwrite: true);
            }

            return blob.Uri.ToString(); // return Azure URL
        }

        // ============================================================
        // ⭐ Upload Worksheet
        // ============================================================
        public async Task<string> UploadWorksheetAsync(IFormFile file, string fileName)
        {
            var blob = _worksheetContainer.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream, overwrite: true);
            }

            return blob.Uri.ToString();
        }

        // ============================================================
        // ⭐ Delete Video
        // ============================================================
        public async Task DeleteVideoAsync(string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
                return;

            string fileName = Path.GetFileName(new Uri(videoUrl).LocalPath);

            var blob = _videoContainer.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync();
        }

        // ============================================================
        // ⭐ Delete Worksheet
        // ============================================================
        public async Task DeleteWorksheetAsync(string worksheetUrl)
        {
            if (string.IsNullOrWhiteSpace(worksheetUrl))
                return;

            string fileName = Path.GetFileName(new Uri(worksheetUrl).LocalPath);

            var blob = _worksheetContainer.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync();
        }

        // ============================================================
        // ⭐ Update Video (delete old + upload new)
        // ============================================================
        public async Task<string> UpdateVideoAsync(string oldUrl, IFormFile newFile, string newFileName)
        {
            await DeleteVideoAsync(oldUrl);
            return await UploadVideoAsync(newFile, newFileName);
        }

        // ============================================================
        // ⭐ Stream Video (returns Stream)
        // ============================================================
        public async Task<Stream> StreamVideoAsync(string videoUrl)
        {
            string fileName = Path.GetFileName(new Uri(videoUrl).LocalPath);

            var blob = _videoContainer.GetBlobClient(fileName);

            var response = await blob.DownloadAsync();
            return response.Value.Content;
        }
    }
    /*
     Controller
Add this field:

csharp
private readonly AzureVideoManager _videoManager;
Inject it in the constructor:

csharp
public AdminController(ApplicationDbContext context, IWebHostEnvironment env, IConfiguration config)
{
    _context = context;
    _env = env;
    _videoManager = new AzureVideoManager(config);
}


    ✔ Upload video
     string fileName = SanitizeFileName(title) + Path.GetExtension(videoFile.FileName);
string videoUrl = await _videoManager.UploadVideoAsync(videoFile, fileName);

    ✔ Upload worksheet
     
     string wsName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
string wsUrl = await _videoManager.UploadWorksheetAsync(file, wsName);

⭐ How to use it in UpdateVideo
    string newUrl = await _videoManager.UpdateVideoAsync(existingVideo.FileName, videoFile, newFileName);
existingVideo.FileName = newUrl;

How to use it in Stream
csharp
var stream = await _videoManager.StreamVideoAsync(video.FileName);
return File(stream, "video/mp4");

     
     
     
     */




}

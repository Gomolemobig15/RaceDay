
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private const string ContainerName = "raceday-images";

        public BlobStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["BlobStorage:ConnectionString"]
                ?? throw new InvalidOperationException("Blob Storage connection string is missing.");

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Please select an image.");

            string[] allowedTypes =
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/webp"
            };

            if (!allowedTypes.Contains(file.ContentType))
                throw new ArgumentException("Only JPEG, PNG, GIF and WEBP images are allowed.");

            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("Image size cannot exceed 5 MB.");

            BlobContainerClient container =
                _blobServiceClient.GetBlobContainerClient(ContainerName);

            await container.CreateIfNotExistsAsync();

            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid()}{extension}";

            BlobClient blob = container.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    }
                });
            }

            return blob.Uri.ToString();
        }
    }
}

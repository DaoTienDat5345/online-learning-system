using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Quiz_Web.Models.Settings;
using Quiz_Web.Services.IServices;

namespace Quiz_Web.Services
{
    public class CloudflareR2StorageService : IStorageService
    {
        private readonly CloudflareR2Settings _r2Settings;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CloudflareR2StorageService> _logger;

        public CloudflareR2StorageService(
            IOptions<CloudflareR2Settings> r2Options,
            IWebHostEnvironment env,
            ILogger<CloudflareR2StorageService> logger)
        {
            _r2Settings = r2Options.Value;
            _env = env;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder = "videos")
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
            var subPath = $"{folder}/{DateTime.UtcNow:yyyy/MM}/{uniqueFileName}";

            // If Cloudflare R2 credentials are not set, fall back to local disk storage
            if (!_r2Settings.IsConfigured)
            {
                _logger.LogWarning("Cloudflare R2 is not fully configured. Storing file locally in wwwroot/uploads.");
                return await UploadToLocalStorageAsync(fileStream, folder, uniqueFileName);
            }

            try
            {
                _logger.LogInformation("Uploading file {FileName} ({ContentType}) to Cloudflare R2 bucket: {Bucket}", fileName, contentType, _r2Settings.BucketName);

                var s3Config = new AmazonS3Config
                {
                    ServiceURL = $"https://{_r2Settings.AccountId}.r2.cloudflarestorage.com",
                    AuthenticationRegion = "auto"
                };

                var credentials = new BasicAWSCredentials(_r2Settings.AccessKeyId, _r2Settings.SecretAccessKey);
                using var client = new AmazonS3Client(credentials, s3Config);

                var putRequest = new PutObjectRequest
                {
                    BucketName = _r2Settings.BucketName,
                    Key = subPath,
                    InputStream = fileStream,
                    ContentType = contentType,
                    DisablePayloadSigning = true
                };

                var response = await client.PutObjectAsync(putRequest);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    string publicUrl;
                    if (!string.IsNullOrWhiteSpace(_r2Settings.PublicUrl))
                    {
                        publicUrl = $"{_r2Settings.PublicUrl.TrimEnd('/')}/{subPath}";
                    }
                    else
                    {
                        publicUrl = $"https://{_r2Settings.BucketName}.{_r2Settings.AccountId}.r2.cloudflarestorage.com/{subPath}";
                    }

                    _logger.LogInformation("File uploaded to Cloudflare R2 successfully: {Url}", publicUrl);
                    return publicUrl;
                }

                _logger.LogWarning("Cloudflare R2 returned status {Status}. Falling back to local storage.", response.HttpStatusCode);
                return await UploadToLocalStorageAsync(fileStream, folder, uniqueFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file to Cloudflare R2. Falling back to local storage.");
                return await UploadToLocalStorageAsync(fileStream, folder, uniqueFileName);
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrlOrKey)
        {
            if (string.IsNullOrWhiteSpace(fileUrlOrKey)) return false;

            try
            {
                // Check if it's a local file
                if (fileUrlOrKey.StartsWith("/uploads/") || fileUrlOrKey.StartsWith("uploads/"))
                {
                    var relativePath = fileUrlOrKey.TrimStart('/');
                    var fullPath = Path.Combine(_env.WebRootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        return true;
                    }
                    return false;
                }

                // If R2 is configured, delete from R2
                if (_r2Settings.IsConfigured)
                {
                    // Extract object key from URL
                    var uri = new Uri(fileUrlOrKey);
                    var objectKey = uri.AbsolutePath.TrimStart('/');

                    var s3Config = new AmazonS3Config
                    {
                        ServiceURL = $"https://{_r2Settings.AccountId}.r2.cloudflarestorage.com",
                        AuthenticationRegion = "auto"
                    };

                    var credentials = new BasicAWSCredentials(_r2Settings.AccessKeyId, _r2Settings.SecretAccessKey);
                    using var client = new AmazonS3Client(credentials, s3Config);

                    var deleteRequest = new DeleteObjectRequest
                    {
                        BucketName = _r2Settings.BucketName,
                        Key = objectKey
                    };

                    var response = await client.DeleteObjectAsync(deleteRequest);
                    return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent || response.HttpStatusCode == System.Net.HttpStatusCode.OK;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file from storage: {File}", fileUrlOrKey);
                return false;
            }
        }

        private async Task<string> UploadToLocalStorageAsync(Stream fileStream, string folder, string uniqueFileName)
        {
            var relativeFolder = Path.Combine("uploads", folder, DateTime.UtcNow.ToString("yyyy/MM"));
            var physicalFolder = Path.Combine(_env.WebRootPath, relativeFolder);
            Directory.CreateDirectory(physicalFolder);

            var physicalFilePath = Path.Combine(physicalFolder, uniqueFileName);

            // Reset stream position if possible
            if (fileStream.CanSeek && fileStream.Position > 0)
            {
                fileStream.Position = 0;
            }

            await using (var localFile = File.Create(physicalFilePath))
            {
                await fileStream.CopyToAsync(localFile);
            }

            var localUrl = "/" + Path.Combine(relativeFolder, uniqueFileName).Replace("\\", "/");
            _logger.LogInformation("File saved to local storage: {Url}", localUrl);
            return localUrl;
        }
    }
}

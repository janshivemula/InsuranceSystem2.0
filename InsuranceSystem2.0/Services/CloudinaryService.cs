using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using InsuranceSystem2._0.Services.Interfaces;
using InsuranceSystem2._0.Exceptions;
using System.IO;
using System.Text.RegularExpressions;


namespace InsuranceSystem2._0.Services.Implementations
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;


        public CloudinaryService(IConfiguration configuration)
        {
            var account = new Account(
                configuration["CloudinarySettings:CloudName"],
                configuration["CloudinarySettings:ApiKey"],
                configuration["CloudinarySettings:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }


        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("Please select a profile image.");

            // Maximum size = 5 MB
            const long maxFileSize = 5 * 1024 * 1024;

            if (file.Length > maxFileSize)
                throw new BadRequestException("Profile image size cannot exceed 5 MB.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new BadRequestException(
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");

            var allowedContentTypes = new[]
               {
                 "image/jpeg",
                 "image/png",
                "image/webp"
               };

            if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
            {
                throw new BadRequestException(
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");
            }

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(
                    file.FileName,
                    file.OpenReadStream())
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception(result.Error.Message);

            return result.SecureUrl.ToString();
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            try
            {
                var uri = new Uri(imageUrl);

                var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                // Find upload folder after "upload"
                int uploadIndex = Array.IndexOf(segments, "upload");

                if (uploadIndex == -1 || uploadIndex + 1 >= segments.Length)
                    return;

                // Skip version if present (v123456...)
                int startIndex = uploadIndex + 1;

                if (segments[startIndex].StartsWith("v"))
                    startIndex++;

                var publicId = string.Join("/", segments[startIndex..]);

                // Remove extension
                publicId = Regex.Replace(publicId, @"\.[^.]+$", "");

                var deleteParams = new DeletionParams(publicId);

                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Result != "ok" && result.Result != "not found")
                {
                    throw new Exception(result.Error?.Message ?? "Unable to delete image.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cloudinary delete failed: {ex.Message}");
            }
        }
    }
}
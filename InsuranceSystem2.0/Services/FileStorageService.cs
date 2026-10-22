using InsuranceSystem2._0.Exceptions;
using Microsoft.AspNetCore.Http;

namespace InsuranceSystem2._0.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        // Allowed file types
        private readonly string[] _allowedExtensions =
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png",
            ".doc",
            ".docx"
        };

        // Maximum file size (10 MB)
        private const long MaxFileSize = 10 * 1024 * 1024;

        public FileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveClaimDocumentAsync(IFormFile file)
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("Please select a document.");
            }

            if (file.Length > MaxFileSize)
            {
                throw new BadRequestException("File size cannot exceed 10 MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions.Contains(extension))
            {
                throw new BadRequestException(
                    "Only PDF, JPG, JPEG, PNG, DOC and DOCX files are allowed.");
            }

            // Create upload folder if it doesn't exist
            var uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "claims");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique file name
            var fileName = $"{Guid.NewGuid()}{extension}";

            var fullPath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path to store in database
            return Path.Combine("uploads", "claims", fileName)
                .Replace("\\", "/");
        }
    }
}
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
            // 1. Validate file
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException(
                    "Please select a document.");
            }

            if (file.Length > MaxFileSize)
            {
                throw new BadRequestException(
                    "File size cannot exceed 10 MB.");
            }

            // 2. Validate extension
            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!_allowedExtensions.Contains(extension))
            {
                throw new BadRequestException(
                    "Only PDF, JPG, JPEG, PNG, DOC and DOCX files are allowed.");
            }

            // 3. Get the web root directory safely
            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot");
            }

            // 4. Create the uploads/claims folder
            var uploadsFolder = Path.Combine(
                webRootPath,
                "uploads",
                "claims");

            Directory.CreateDirectory(uploadsFolder);

            // 5. Generate a unique file name
            var fileName = $"{Guid.NewGuid()}{extension}";

            var fullPath = Path.Combine(
                uploadsFolder,
                fileName);

            // 6. Save the uploaded file
            await using (var stream = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write))
            {
                await file.CopyToAsync(stream);
            }

            // 7. Return the relative path for the database
            return Path.Combine(
                "uploads",
                "claims",
                fileName).Replace("\\", "/");
        }

    }
}
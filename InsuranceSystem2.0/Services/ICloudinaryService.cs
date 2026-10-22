using Microsoft.AspNetCore.Http;

namespace InsuranceSystem2._0.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file);

        Task DeleteImageAsync(string imageUrl);
    }
}
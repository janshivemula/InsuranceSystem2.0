using Microsoft.AspNetCore.Http;

namespace InsuranceSystem2._0.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveClaimDocumentAsync(IFormFile file);
    }
}

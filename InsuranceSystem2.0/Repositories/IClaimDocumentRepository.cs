using InsuranceSystem2._0.DTOs.Claim;
using InsuranceSystem2._0.Models;

namespace InsuranceSystem2._0.Repositories.Interfaces
{
    public interface IClaimDocumentRepository
    {
        Task<IEnumerable<ClaimDocument>> GetAllAsync();

        Task<IEnumerable<ClaimDocument>> GetByClaimIdAsync(int claimId);

        Task<ClaimDocument?> GetByIdAsync(int id);

        Task AddAsync(ClaimDocument claimDocument);

        Task SaveChangesAsync();
    }
}
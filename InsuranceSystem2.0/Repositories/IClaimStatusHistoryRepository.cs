
using InsuranceSystem2._0.DTOs.Claim;
using InsuranceSystem2._0.Models;

namespace InsuranceSystem2._0.Repositories.Interfaces
{
    public interface IClaimStatusHistoryRepository
    {
        Task<IEnumerable<ClaimStatusHistory>> GetAllAsync();

        Task<IEnumerable<ClaimStatusHistory>> GetByClaimIdAsync(int claimId);

        Task<ClaimStatusHistory?> GetByIdAsync(int historyId);

        Task AddAsync(ClaimStatusHistory claimStatusHistory);

        Task SaveChangesAsync();
    }
}
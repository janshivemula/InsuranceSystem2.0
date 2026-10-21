using InsuranceSystem2._0.DTOs.Policy;
using InsuranceSystem2._0.Models;
using InsuranceSystem2._0.Repositories;

namespace InsuranceSystem2._0.Repositories.Interfaces
{
    public interface IPolicyRepository
    {
        Task<PagedResult<Policy>> GetAllAsync(PolicyQueryDto query);

        Task<IEnumerable<Policy>> GetPoliciesByCustomerIdAsync(int customerId);

        Task<IEnumerable<Policy>> GetPoliciesByUserIdAsync(int userId);

        Task<IEnumerable<Policy>> GetActivePoliciesAsync();

        Task<Policy?> GetByIdAsync(int id);

        Task<Policy?> GetByPolicyNumberAsync(string policyNumber);

        Task AddAsync(Policy policy);

        Task UpdateAsync(Policy policy);

        Task SaveChangesAsync();
    }
}
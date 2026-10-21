using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.Models;

namespace InsuranceSystem2._0.Repositories.Interfaces
{
    public interface IPremiumPaymentRepository
    {
        Task<PagedResponse<PremiumPayment>> GetAllAsync(PaginationRequestDto paginationDto);
        Task<PagedResponse<PremiumPayment>> GetPaymentsByPolicyIdAsync(int policyId, PaginationRequestDto paginationDto);
        Task<PagedResponse<PremiumPayment>> GetPaymentsByCustomerIdAsync(int customerId, PaginationRequestDto paginationDto);
        Task<PremiumPayment?> GetByIdAsync(int id);
        Task<PremiumPayment?> GetByTransactionReferenceAsync(string transactionReference);
        Task AddAsync(PremiumPayment premiumPayment);
        Task UpdateAsync(PremiumPayment premiumPayment);
        Task SaveChangesAsync();
    }
}
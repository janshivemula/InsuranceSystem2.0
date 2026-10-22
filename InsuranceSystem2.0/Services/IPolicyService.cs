using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.Policy;

namespace InsuranceSystem2._0.Services.Interfaces
{
    public interface IPolicyService
    {
        Task<PagedResponse<PolicyResponseDto>> GetAllPoliciesAsync(PolicyQueryDto query);

        Task<IEnumerable<PolicyResponseDto>> GetPoliciesByCustomerIdAsync(int customerId);

        Task<IEnumerable<PolicyResponseDto>> GetActivePoliciesByCustomerIdAsync(int customerId);
        Task<IEnumerable<PolicyResponseDto>> GetMyPoliciesAsync(int userId);

        Task<IEnumerable<PolicyResponseDto>> GetActivePoliciesAsync();

        Task<PolicyResponseDto> GetPolicyByIdAsync(int policyId, int loggedInUserId, string role);

        Task<PolicyResponseDto> GetPolicyByPolicyNumberAsync(string policyNumber, int loggedInUserId, string role);

        Task<PolicyResponseDto> PurchasePolicyAsync(int customerUserId, PurchasePolicyRequestDto requestDto);

        Task<PolicyResponseDto> IssuePolicyByInternalStaffAsync(IssuePolicyRequestDto requestDto);
        Task<PolicyResponseDto> CancelPolicyAsync(int policyId);
    }
}
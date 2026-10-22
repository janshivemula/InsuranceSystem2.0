using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.Plan;

namespace InsuranceSystem2._0.Services.Interfaces
{
    public interface IPolicyPlanService
    {
        Task<PagedResponse<PlanResponseDto>> GetAllPlansAsync(PolicyPlanQueryDto query);

        Task<IEnumerable<PlanResponseDto>> GetActivePlansAsync();

        Task<IEnumerable<PlanResponseDto>> GetPlansByProductIdAsync(int productId);

        Task<IEnumerable<PlanResponseDto>> GetActivePlansByProductIdAsync(int productId);

        Task<PlanResponseDto?> GetPlanByIdAsync(int id);

        Task<PlanResponseDto> CreatePlanAsync(PlanRequestDto requestDto);

        Task UpdatePlanAsync(int id, PlanRequestDto requestDto);

        Task SoftDeletePlanAsync(int id);
    }
}
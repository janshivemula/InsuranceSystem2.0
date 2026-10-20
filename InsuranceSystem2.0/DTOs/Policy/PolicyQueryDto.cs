using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.DTOs.Policy
{
    public class PolicyQueryDto : PaginationRequestDto
    {
        public PolicyStatus? Status { get; set; }

        public int? CustomerId { get; set; }

        public int? PlanId { get; set; }
    }
}

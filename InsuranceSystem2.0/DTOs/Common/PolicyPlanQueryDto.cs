using InsuranceSystem2._0.Enums;
using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Common
{
    public class PolicyPlanQueryDto
    {
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }

        public int? ProductId { get; set; }

        public PremiumType? PremiumType { get; set; }

        public bool? IsActive { get; set; }

        public string SortBy { get; set; } = "PlanName";

        public bool Descending { get; set; } = false;
    }
}

using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.DTOs.Plan
{
    public class PlanResponseDto
    {
        public int PlanId { get; set; }

        public int InsuranceProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;

        public string PlanName { get; set; } = string.Empty;
        public decimal CoverageAmount { get; set; }
        public decimal PremiumAmount { get; set; }
        public PremiumType PremiumType { get; set; }
        public int DurationInYears { get; set; }
        public string TermsAndConditions { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}

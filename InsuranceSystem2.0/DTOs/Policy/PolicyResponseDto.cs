using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.DTOs.Policy
{
    public class PolicyResponseDto
    {
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public ProductType ProductType { get; set; }

        public decimal CoverageAmount { get; set; }
        public decimal PremiumAmount { get; set; }
        public PremiumType PremiumType { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public PolicyStatus PolicyStatus { get; set; }

        public decimal TotalPremiumPaid { get; set; }

        public decimal TotalPremiumAmount { get; set; }

        public decimal RemainingPremiumAmount { get; set; }

        public DateTime? LastPaymentDate { get; set; }

        public DateTime? NextDueDate { get; set; }

        public bool IsPaymentDue { get; set; }

        public int InstallmentsPaid { get; set; }

        public int TotalInstallments { get; set; }
    }
}

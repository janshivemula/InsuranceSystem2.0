using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.DTOs.Claim
{
    public class ClaimResponseDto
    {
        public int ClaimId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;

        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string PlanName { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public decimal ClaimAmount { get; set; }
        public string ClaimReason { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }

        public ClaimStatus ClaimStatus { get; set; }

        public string? StaffRemarks { get; set; }
        public string? AdminRemarks { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public List<ClaimDocumentResponseDto> Documents { get; set; } = new();
        public List<ClaimStatusHistoryResponseDto> StatusHistory { get; set; } = new();
    }
}

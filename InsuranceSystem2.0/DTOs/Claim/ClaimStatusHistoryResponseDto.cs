using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.DTOs.Claim
{
    public class ClaimStatusHistoryResponseDto
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }

        public ClaimStatus PreviousStatus { get; set; }
        public ClaimStatus NewStatus { get; set; }

        public string? Remarks { get; set; }

        public int UpdatedByUserId { get; set; }
        public string UpdatedByUserName { get; set; } = string.Empty;

        public DateTime UpdatedDate { get; set; }
    }
}

using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.Helpers
{
    public class ClaimStatusValidator
    {
        public static bool IsValidTransition(ClaimStatus currentStatus, ClaimStatus newStatus)
        {
            return currentStatus switch
            {
                ClaimStatus.Submitted => newStatus == ClaimStatus.UnderReview,

                ClaimStatus.UnderReview =>
                    newStatus == ClaimStatus.RecommendedForApproval ||
                    newStatus == ClaimStatus.RecommendedForRejection,

                ClaimStatus.RecommendedForApproval => newStatus == ClaimStatus.Approved,

                ClaimStatus.RecommendedForRejection => newStatus == ClaimStatus.Rejected,

                ClaimStatus.Approved => false,

                ClaimStatus.Rejected => false,

                _ => false
            };
        }
    }
}

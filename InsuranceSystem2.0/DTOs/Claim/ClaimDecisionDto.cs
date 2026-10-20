using InsuranceSystem2._0.Enums;
using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Claim
{
    public class ClaimDecisionDto
    {
        [Required]
        public ClaimStatus FinalDecisionStatus { get; set; }

        [Required]
        [StringLength(500)]
        public string Remarks { get; set; }
    }
}

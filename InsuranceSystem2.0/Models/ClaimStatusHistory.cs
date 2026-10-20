using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.Models
{
    public class ClaimStatusHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required(ErrorMessage = "Claim is required.")]
        public int ClaimId { get; set; }

        [ForeignKey(nameof(ClaimId))]
        public Claim Claim { get; set; }

        [Required(ErrorMessage = "Previous status is required.")]
        public ClaimStatus PreviousStatus { get; set; }

        [Required(ErrorMessage = "New status is required.")]
        public ClaimStatus NewStatus { get; set; }

        [Required(ErrorMessage = "Remarks are required.")]
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string Remarks { get; set; } = string.Empty;

        [Required(ErrorMessage = "Updated By is required.")]
        public int UpdatedBy { get; set; }

        [ForeignKey(nameof(UpdatedBy))]
        public User User { get; set; }

        [Required(ErrorMessage = "Updated Date is required.")]
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}

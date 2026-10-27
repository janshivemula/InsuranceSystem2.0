using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Claim number is required.")]
        [StringLength(20, ErrorMessage = "Claim number cannot exceed 20 characters.")]
        public string ClaimNumber { get; set; } = string.Empty;

        // Foreign Key to Customer
        [Required(ErrorMessage = "Customer is required.")]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = null!;

        // Foreign Key to Policy
        [Required(ErrorMessage = "Policy is required.")]
        public int PolicyId { get; set; }

        [ForeignKey(nameof(PolicyId))]
        public Policy Policy { get; set; } = null!;

        [Required(ErrorMessage = "Claim amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Claim amount must be greater than 0.")]
        public decimal ClaimAmount { get; set; }

        [Required(ErrorMessage = "Claim reason is required.")]
        [StringLength(500, ErrorMessage = "Claim reason cannot exceed 500 characters.")]
        public string ClaimReason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Incident date is required.")]
        public DateOnly IncidentDate { get; set; }

        [Required(ErrorMessage = "Claim status is required.")]
        public ClaimStatus ClaimStatus { get; set; }

        [StringLength(500)]
        public string? StaffRemarks { get; set; }

        [StringLength(500)]
        public string? AdminRemarks { get; set; }

        // Staff member who most recently reviewed the claim
        public int? ReviewedByUserId { get; set; }

        [ForeignKey(nameof(ReviewedByUserId))]
        public User? ReviewedByUser { get; set; }

        public DateTime? ReviewedAt { get; set; }

        // Admin who made the final claim decision
        public int? DecidedByUserId { get; set; }

        [ForeignKey(nameof(DecidedByUserId))]
        public User? DecidedByUser { get; set; }

        public DateTime? DecidedAt { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        // One Claim can have many supporting documents
        public ICollection<ClaimDocument> ClaimDocuments { get; set; } = new List<ClaimDocument>();

        // One Claim can have many status history records
        public ICollection<ClaimStatusHistory> ClaimStatusHistories { get; set; } = new List<ClaimStatusHistory>();
    }
}
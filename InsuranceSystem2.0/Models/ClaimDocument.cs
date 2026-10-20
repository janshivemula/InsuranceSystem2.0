using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace InsuranceSystem2._0.Models
{
    public class ClaimDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [Required(ErrorMessage = "Claim ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid claim.")]
        public int ClaimId { get; set; }//ForiegnKey 

        public Claim? Claim { get; set; }

        [Required(ErrorMessage = "Document name is required.")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Document name must be between 3 and 100 characters.")]
        public string DocumentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document type is required.")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Document type must be between 3 and 50 characters.")]
        public string DocumentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document reference is required.")]
        [StringLength(255,
            ErrorMessage = "Document reference cannot exceed 255 characters.")]

        public string DocumentReference { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; } = DateTime.Now;
    }
}

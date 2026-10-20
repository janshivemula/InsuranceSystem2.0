using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Claim
{
    public class ClaimDocumentRequestDto
    {
        [Required]
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Document name is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Document name must be between 2 and 100 characters.")]
        public string DocumentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document type is required.")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Document type must be between 2 and 50 characters.")]
        public string DocumentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document file is required.")]
        public IFormFile Document { get; set; } = null!;
    }
}

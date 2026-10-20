using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Claim
{
    public class ClaimRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Policy id must be greater than 0.")]
        public int PolicyId { get; set; }

        [Range(typeof(decimal), "1", "999999999999",
            ErrorMessage = "Claim amount must be greater than 0.")]
        public decimal ClaimAmount { get; set; }

        [Required(ErrorMessage = "Claim reason is required.")]
        [StringLength(500, MinimumLength = 5,
            ErrorMessage = "Claim reason must be between 5 and 500 characters.")]
        public string ClaimReason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Incident date is required.")]
        public DateTime IncidentDate { get; set; }

        [Required(ErrorMessage = "Document name is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Document name must be between 2 and 100 characters.")]
        public string DocumentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document type is required.")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Document type must be between 2 and 50 characters.")]
        public string DocumentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Supporting document is required.")]
        public IFormFile Document { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Claim
{
    public class SupportingDocumentDto
    {
        [Required]
        [StringLength(100)]
        public string DocumentName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        public IFormFile Document { get; set; } = null!;
    }
}

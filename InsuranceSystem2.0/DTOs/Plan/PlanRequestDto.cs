using InsuranceSystem2._0.Enums;
using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Plan
{
    public class PlanRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Insurance product id must be greater than 0.")]
        public int InsuranceProductId { get; set; }

        [Required(ErrorMessage = "Plan name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Plan name must be between 3 and 100 characters.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Plan name can contain only alphabets and spaces.")]
        public string PlanName { get; set; } = string.Empty;

        [Range(typeof(decimal), "1", "999999999999", ErrorMessage = "Coverage amount must be greater than 0.")]
        public decimal CoverageAmount { get; set; }

        [Range(typeof(decimal), "1", "999999999999", ErrorMessage = "Premium amount must be greater than 0.")]
        public decimal PremiumAmount { get; set; }

        [EnumDataType(typeof(PremiumType), ErrorMessage = "Invalid premium type.")]
        public PremiumType PremiumType { get; set; }

        [Range(1, 100, ErrorMessage = "Duration in years must be between 1 and 100.")]
        public int DurationInYears { get; set; }

        [Required(ErrorMessage = "Terms and conditions are required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Terms and conditions must be between 10 and 1000 characters.")]
        public string TermsAndConditions { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}

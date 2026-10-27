using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.Models
{
    public class PolicyPlan
    {
        [Key]
        public int PlanId { get; set; }

        // Foreign Key to Insurance Product
        [Required(ErrorMessage = "Product is required.")]
        public int InsuranceProductId { get; set; }

        //navi
        [ForeignKey(nameof(InsuranceProductId))]
        public InsuranceProduct InsuranceProduct { get; set; } = null!;

        [Required(ErrorMessage = "Plan Name is required.")]
        [StringLength(100, ErrorMessage = "Plan Name cannot exceed 100 characters.")]
        public string PlanName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Coverage Amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Coverage Amount must be greater than zero.")]
        public decimal CoverageAmount { get; set; }

        // here premium amount means per installement not the whole premium 
        [Required(ErrorMessage = "Premium Amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Premium Amount must be greater than zero.")]
        public decimal PremiumAmount { get; set; }

        [Required(ErrorMessage = "Premium Type is required.")]
        public PremiumType PremiumType { get; set; }

        [Required(ErrorMessage = "Duration is required.")]
        [Range(1, 100, ErrorMessage = "Duration must be between 1 and 100 years.")]
        public int DurationInYears { get; set; }

        [Required(ErrorMessage = "Terms and Conditions are required.")]
        [StringLength(1000, ErrorMessage = "Terms and Conditions cannot exceed 1000 characters.")]
        public string TermsAndConditions { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        // One Policy Plan can have many Policies
        public ICollection<Policy> Policies { get; set; } = new List<Policy>();
    }
}

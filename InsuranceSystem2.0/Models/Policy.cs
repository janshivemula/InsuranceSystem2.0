using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.Models
{
    // ** actual policy purchased by cust
    public class Policy
    {
        [Key]
        public int PolicyId { get; set; }

        [Required(ErrorMessage = "Policy Number is required.")]
        [StringLength(20, ErrorMessage = "Policy Number cannot exceed 20 characters.")]
        public string PolicyNumber { get; set; } = string.Empty;

        // Foreign Key one cust - many policy
        [Required(ErrorMessage = "Customer is required.")]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = null!;

        // Foreign Key to Insurance Product
        [Required(ErrorMessage = "Insurance Product is required.")]
        public int InsuranceProductId { get; set; }

        [ForeignKey(nameof(InsuranceProductId))]
        public InsuranceProduct InsuranceProduct { get; set; } = null!;

        // Foreign Key to Policy Plan
        [Required(ErrorMessage = "Policy Plan is required.")]
        public int PlanId { get; set; }

        [ForeignKey(nameof(PlanId))]
        public PolicyPlan Plan { get; set; } = null!;

        [Required(ErrorMessage = "Start Date is required.")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required.")]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Policy Status is required.")]
        public PolicyStatus PolicyStatus { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total Premium Paid cannot be negative.")]
        public decimal TotalPremiumPaid { get; set; } = 0;

        public DateTime? LastPaymentDate { get; set; }

        public DateTime? NextDueDate { get; set; }
        public bool IsPaymentDue { get; set; } = false;

        public int InstallmentsPaid { get; set; } = 0;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        // One Policy - many Premium Payments
        public ICollection<PremiumPayment> Payments { get; set; } = new List<PremiumPayment>();

        // One Policy - many Claims
        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
  

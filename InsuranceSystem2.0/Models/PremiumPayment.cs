using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.Models
{
    public class PremiumPayment
    {
        [Key]
        public int PaymentId { get; set; }

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

        [Required(ErrorMessage = "Payment amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment date is required.")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Payment mode is required.")]
        public PaymentMode PaymentMode { get; set; }

        [Required(ErrorMessage = "Transaction reference is required.")]
        [StringLength(100, ErrorMessage = "Transaction reference cannot exceed 100 characters.")]
        public string TransactionReference { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment status is required.")]
        public PaymentStatus PaymentStatus { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

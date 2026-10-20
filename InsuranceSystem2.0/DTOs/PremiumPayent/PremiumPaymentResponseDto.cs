using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.DTOs.PremiumPayent
{
    public class PremiumPaymentResponseDto
    {
        public int PaymentId { get; set; }

        public int PolicyId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string PolicyNumber { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public PaymentMode PaymentMode { get; set; }

        public string TransactionReference { get; set; } = string.Empty;

        public PaymentStatus PaymentStatus { get; set; }

        public DateTime CreatedDate { get; set; }

        public decimal TotalPremiumPaid { get; set; }

        public DateTime? NextDueDate { get; set; }

        public bool IsPaymentDue { get; set; }
    }
}

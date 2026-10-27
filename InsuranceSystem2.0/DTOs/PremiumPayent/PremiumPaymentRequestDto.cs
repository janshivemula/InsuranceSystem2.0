using InsuranceSystem2._0.Enums;
using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.PremiumPayent
{
    public class PremiumPaymentRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Policy id must be greater than 0.")]
        public int PolicyId { get; set; }


        [EnumDataType(typeof(PaymentMode), ErrorMessage = "Invalid payment mode.")]
        public PaymentMode PaymentMode { get; set; }

        
    }
}
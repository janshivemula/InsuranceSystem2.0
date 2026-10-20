using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Policy
{
    public class PurchasePolicyRequestDto
    {
        [Required]
        public int PlanId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }
    }
}

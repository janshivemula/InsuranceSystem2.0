using System.ComponentModel.DataAnnotations;
using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.Models
{
    public class InsuranceProduct
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(100, ErrorMessage = "Product Name cannot exceed 100 characters.")]
        [MinLength(3, ErrorMessage = "Product Name must contain at least 3 characters.")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product Type is required.")]
        public ProductType ProductType { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [MinLength(10, ErrorMessage = "Description must contain at least 10 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        // Navigation Property
        //One product - many plans
        public ICollection<PolicyPlan>? PolicyPlans { get; set; } = new List<PolicyPlan>();

        // One Insurance Product - many Policies
        public ICollection<Policy> Policies { get; set; } = new List<Policy>();
    }
}

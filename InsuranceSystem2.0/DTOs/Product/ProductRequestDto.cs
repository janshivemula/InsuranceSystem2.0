using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Product
{
    public class ProductRequestDto
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 100 characters.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Product name can contain only alphabets and spaces.")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product type is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Product type must be between 3 and 50 characters.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Product type can contain only alphabets and spaces.")]
        public string ProductType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters.")]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}

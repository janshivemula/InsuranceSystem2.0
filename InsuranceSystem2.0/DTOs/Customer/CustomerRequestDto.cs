using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Customer
{
    public class CustomerRequestDto
    {
        [Required(ErrorMessage = "Date of birth is required.")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 200 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "City must be between 2 and 50 characters.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "City can contain only alphabets and spaces.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "State must be between 2 and 50 characters.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "State can contain only alphabets and spaces.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pin code is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pin code must be a valid 6-digit number.")]
        public string PinCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nominee name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nominee name must be between 3 and 100 characters.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Nominee name can contain only alphabets and spaces.")]
        public string NomineeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nominee relation is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Nominee relation must be between 2 and 50 characters.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Nominee relation can contain only alphabets and spaces.")]
        public string NomineeRelation { get; set; } = string.Empty;

        //  profile picture
        public IFormFile? ProfileImage { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}

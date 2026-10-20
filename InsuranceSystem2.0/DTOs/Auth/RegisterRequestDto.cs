using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Full name can contain only alphabets and spaces.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Mobile number must be a valid 10-digit Indian mobile number.")]
        public string MobileNumber { get; set; } = string.Empty;
    }
}

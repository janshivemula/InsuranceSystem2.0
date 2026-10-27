using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Auth
{
    public class VerifyOtpRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be a 6-digit number.")]
        public string Otp { get; set; } = string.Empty;
    }
}

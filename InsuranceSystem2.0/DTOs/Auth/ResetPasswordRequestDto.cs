using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.Auth
{
    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Reset token is required.")]
        public string Token { get; set; } = string.Empty;


        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "Password must contain minimum 8 characters.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}

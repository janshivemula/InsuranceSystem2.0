using System.ComponentModel.DataAnnotations;
using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is Required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%?&])[A-Za-z\d@$!%?&]{8,}$",
        ErrorMessage = "Password must contain at least 8 characters, one uppercase letter, one lowercase letter, one number and one special character.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Mobile Number must be exactly 10 digits")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpiry { get; set; }


        //navi one-one
        public Customer? Customer { get; set; }

        //One user Can have many claim histories
        public ICollection<ClaimStatusHistory>? ClaimHistories { get; set; } = new List<ClaimStatusHistory>();
    }
}


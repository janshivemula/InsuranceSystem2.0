using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.Models
{
    // Holds a registration until the customer verifies the OTP sent to their email.
    // The real User row is created only after successful verification.
    public class PendingRegistration
    {
        [Key]
        public int PendingRegistrationId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // BCrypt hash, the plain password is never stored
        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; } = string.Empty;

        // BCrypt hash of the 6-digit OTP
        [Required]
        [StringLength(100)]
        public string OtpHash { get; set; } = string.Empty;

        public DateTime OtpExpiry { get; set; }

        public int FailedAttempts { get; set; } = 0;

        public DateTime LastOtpSentAt { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
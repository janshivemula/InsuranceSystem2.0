using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceSystem2._0.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        // fk 
        [Required(ErrorMessage = "User is required.")]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        public DateOnly DateOfBirth { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(50, ErrorMessage = "State cannot exceed 50 characters.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "PIN Code is required.")]
        [RegularExpression(@"^[1-9][0-9]{5}$",
            ErrorMessage = "PIN Code must be a valid 6-digit Indian PIN Code.")]
        public string PinCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nominee Name is required.")]
        [StringLength(100, ErrorMessage = "Nominee Name cannot exceed 100 characters.")]
        public string NomineeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nominee Relation is required.")]
        [StringLength(50, ErrorMessage = "Nominee Relation cannot exceed 50 characters.")]
        public string NomineeRelation { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public string? ProfileImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Property

        // One Customer - Many Policies
        public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();

        // One Customer - Many Premium Payments
        public virtual ICollection<PremiumPayment> PremiumPayments { get; set; } = new List<PremiumPayment>();
    }
}


using System.ComponentModel.DataAnnotations;

namespace InsuranceSystem2._0.DTOs.User
{
    public class UserStatusUpdateDto
    {
        [Required(ErrorMessage = "User active status is required.")]
        public bool IsActive { get; set; }

        [StringLength(250, ErrorMessage = "Reason cannot exceed 250 characters.")]
        public string? Remarks { get; set; }
    }
}

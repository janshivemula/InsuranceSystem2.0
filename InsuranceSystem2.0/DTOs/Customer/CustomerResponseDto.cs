namespace InsuranceSystem2._0.DTOs.Customer
{
    public class CustomerResponseDto
    {
        public int CustomerId { get; set; }
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;

        public string NomineeName { get; set; } = string.Empty;
        public string NomineeRelation { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}

using InsuranceSystem2._0.DTOs.Common;

namespace InsuranceSystem2._0.DTOs.Customer
{
    public class CustomerQueryDto : PaginationRequestDto
    {
        public bool? IsActive { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }
    }
}

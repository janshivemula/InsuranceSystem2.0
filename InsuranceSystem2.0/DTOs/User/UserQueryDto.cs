using InsuranceSystem2._0.Enums;

namespace InsuranceSystem2._0.DTOs.User
{
    public class UserQueryDto : PaginationRequestDto
    {
        public UserRole? Role { get; set; }

        public bool? IsActive { get; set; }
    }
}

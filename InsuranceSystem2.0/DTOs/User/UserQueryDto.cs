using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.DTOs.Common;

namespace InsuranceSystem2._0.DTOs.User
{
    public class UserQueryDto : PaginationRequestDto
    {
        public UserRole? Role { get; set; }

        public bool? IsActive { get; set; }
    }
}

using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.User;
using InsuranceSystem2._0.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceSystem2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/User
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryDto query)
        {
            var users = await _userService.GetAllUsersAsync(query);

            return Ok(new ApiResponse<PagedResponse<UserResponseDto>>
            {
                Success = true,
                Message = "Users retrieved successfully.",
                Data = users,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/User/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveUsers()
        {
            var users = await _userService.GetActiveUsersAsync();

            return Ok(new ApiResponse<IEnumerable<UserResponseDto>>
            {
                Success = true,
                Message = "Active users retrieved successfully.",
                Data = users,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/User/internal-staff
        [HttpGet("internal-staff")]
        public async Task<IActionResult> GetActiveInternalStaff()
        {
            var users = await _userService.GetActiveInternalStaffAsync();

            return Ok(new ApiResponse<IEnumerable<UserResponseDto>>
            {
                Success = true,
                Message = "Active internal staff retrieved successfully.",
                Data = users,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/User/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = null,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<UserResponseDto>
            {
                Success = true,
                Message = "User retrieved successfully.",
                Data = user,
                Timestamp = DateTime.UtcNow
            });
        }

        // POST: api/User/admin
        [HttpPost("admin")]
        public async Task<IActionResult> CreateAdmin(CreateAdminRequestDto dto)
        {
            var admin = await _userService.CreateAdminAsync(dto);

            return CreatedAtAction(nameof(GetUserById),
                new { id = admin.UserId },
                new ApiResponse<UserResponseDto>
                {
                    Success = true,
                    Message = "Admin created successfully.",
                    Data = admin,
                    Timestamp = DateTime.UtcNow
                });
        }

        // POST: api/User/internal-staff
        [HttpPost("internal-staff")]
        public async Task<IActionResult> CreateInternalStaff(CreateInternalStaffRequestDto dto)
        {
            var staff = await _userService.CreateInternalStaffAsync(dto);

            return CreatedAtAction(nameof(GetUserById),
                new { id = staff.UserId },
                new ApiResponse<UserResponseDto>
                {
                    Success = true,
                    Message = "Internal staff created successfully.",
                    Data = staff,
                    Timestamp = DateTime.UtcNow
                });
        }

        // PUT: api/User/5/status
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, UserStatusUpdateDto dto)
        {
            var user = await _userService.UpdateUserStatusAsync(id, dto);

            return Ok(new ApiResponse<UserResponseDto>
            {
                Success = true,
                Message = dto.IsActive
                    ? "User activated successfully."
                    : "User deactivated successfully.",
                Data = user,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

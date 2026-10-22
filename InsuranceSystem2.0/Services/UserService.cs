using AutoMapper;
using InsuranceSystem2._0.Services.Interfaces;
using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.User;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Exceptions;
using InsuranceSystem2._0.Models;
using InsuranceSystem2._0.Repositories.Interfaces;

namespace InsuranceSystem2._0.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IMapper mapper, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // Get all users
        public async Task<PagedResponse<UserResponseDto>> GetAllUsersAsync(UserQueryDto query)
        {
            var pagedUsers = await _userRepository.GetAllAsync(query);

            var userDtos = _mapper.Map<IEnumerable<UserResponseDto>>(pagedUsers.Items);

            return new PagedResponse<UserResponseDto>
            {
                Records = userDtos,
                CurrentPage = query.PageNumber,
                PageSize = query.PageSize,
                TotalRecords = pagedUsers.TotalRecords,
                TotalPages = (int)Math.Ceiling((double)pagedUsers.TotalRecords / query.PageSize),
                IsLastPage = query.PageNumber >= (int)Math.Ceiling((double)pagedUsers.TotalRecords / query.PageSize),
                SortField = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        // Get all active users
        public async Task<IEnumerable<UserResponseDto>> GetActiveUsersAsync()
        {
            var users = await _userRepository.GetActiveUsersAsync();

            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
        }

        // Get all active internal staff
        public async Task<IEnumerable<UserResponseDto>> GetActiveInternalStaffAsync()
        {
            var staffUsers = await _userRepository.GetActiveInternalStaffAsync();

            return _mapper.Map<IEnumerable<UserResponseDto>>(staffUsers);
        }

        // Get user by Id
        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                return null;

            return _mapper.Map<UserResponseDto>(user);
        }

        // Create Admin
        public async Task<UserResponseDto> CreateAdminAsync(CreateAdminRequestDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            if (await _userRepository.EmailExistsAsync(email))
                throw new ConflictException("Email already exists.");

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                MobileNumber = dto.MobileNumber.Trim(),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("Admin account created successfully. Email: {Email}", user.Email);

            return _mapper.Map<UserResponseDto>(user);
        }

        // Create Internal Staff
        public async Task<UserResponseDto> CreateInternalStaffAsync(CreateInternalStaffRequestDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            if (await _userRepository.EmailExistsAsync(email))
                throw new ConflictException("Email already exists.");

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                MobileNumber = dto.MobileNumber.Trim(),
                Role = UserRole.InternalStaff,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("Internal Staff account created successfully. Email: {Email}", user.Email);

            return _mapper.Map<UserResponseDto>(user);
        }

        // Activate / Deactivate User
        public async Task<UserResponseDto> UpdateUserStatusAsync(int id, UserStatusUpdateDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                throw new NotFoundException("User not found.");

            if (!dto.IsActive)
            {
                await _userRepository.SoftDeleteAsync(user);
            }
            else
            {
                user.IsActive = true;
                user.UpdatedDate = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }

            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("User {UserId} status changed to {Status}", user.UserId, user.IsActive ? "Active" : "Inactive");

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
        {
            return await _userRepository.UpdateUserStatusAsync(userId, isActive);
        }
    }
}
using InsuranceSystem2._0.DTOs.Auth;
using InsuranceSystem2._0.DTOs.User;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Exceptions;
using InsuranceSystem2._0.Models;
using InsuranceSystem2._0.Repositories.Interfaces;
using InsuranceSystem2._0.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InsuranceSystem2._0.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<UserResponseDto> RegisterCustomerAsync(RegisterRequestDto requestDto)
        {
            var email = requestDto.Email.Trim().ToLower();

            if (await _userRepository.EmailExistsAsync(email))
                throw new ConflictException("User with this email already exists.");

            var user = new User
            {
                FullName = requestDto.FullName.Trim(),
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(requestDto.Password),
                MobileNumber = requestDto.MobileNumber.Trim(),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new UserResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate

            };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto requestDto)
        {
            var email = requestDto.Email.Trim().ToLower();

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
                throw new NotFoundException("Invalid email or password.");

            if (!user.IsActive)
                throw new BadRequestException("This user account is inactive.");

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(requestDto.Password, user.Password);

            if (!isPasswordValid)
                throw new NotFoundException("Invalid email or password.");

            return GenerateToken(user);
        }

        private LoginResponseDto GenerateToken(User user)
        {
            var jwtSection = _configuration.GetSection("Jwt");

            var secretKey = jwtSection["Key"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var expiryMinutes = jwtSection["DurationInMinutes"];

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new BadRequestException("JWT SecretKey is missing in configuration.");

            if (string.IsNullOrWhiteSpace(issuer))
                throw new BadRequestException("JWT Issuer is missing in configuration.");

            if (string.IsNullOrWhiteSpace(audience))
                throw new BadRequestException("JWT Audience is missing in configuration.");

            if (!int.TryParse(expiryMinutes, out int tokenExpiryMinutes))
                tokenExpiryMinutes = 60;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(tokenExpiryMinutes);

            var claims = new List<System.Security.Claims.Claim>
    {
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.FullName),
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString())
    };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponseDto
            {
                Token = jwtToken,
                TokenType = "Bearer",
                Email = user.Email,
                Role = user.Role.ToString(),
                ExpiresAt = expiresAt
            };
        }

        public async Task<string> ForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
                throw new NotFoundException("User with this email does not exist.");


            user.PasswordResetToken = Guid.NewGuid().ToString();

            user.PasswordResetTokenExpiry =
                DateTime.UtcNow.AddMinutes(15);


            user.UpdatedDate = DateTime.UtcNow;


            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();


            return user.PasswordResetToken;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            var user = await _userRepository.GetByEmailAsync(email);


            if (user == null)
                throw new NotFoundException("User not found.");


            if (user.PasswordResetToken != dto.Token)
                throw new BadRequestException("Invalid reset token.");


            if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
                throw new BadRequestException("Reset token expired.");


            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);


            user.PasswordResetToken = null;

            user.PasswordResetTokenExpiry = null;

            user.UpdatedDate = DateTime.UtcNow;


            await _userRepository.UpdateAsync(user);

            await _userRepository.SaveChangesAsync();


            return true;
        }
    }
}
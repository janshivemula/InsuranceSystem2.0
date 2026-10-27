using InsuranceSystem2._0.Data;
using InsuranceSystem2._0.DTOs.Auth;
using InsuranceSystem2._0.DTOs.User;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Exceptions;
using InsuranceSystem2._0.Models;
using InsuranceSystem2._0.Repositories.Interfaces;
using InsuranceSystem2._0.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InsuranceSystem2._0.Services.Implementations
{
    public class AuthService : IAuthService
    {
        // NEW: OTP rules
        private const int OtpValidityMinutes = 5;
        private const int MaxOtpAttempts = 5;
        private const int ResendCooldownSeconds = 60;

        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;               // NEW
        private readonly IEmailSender _emailSender;           // NEW
        private readonly ILogger<AuthService> _logger;        // NEW

        public AuthService(
            IUserRepository userRepository,
            IConfiguration configuration,
            AppDbContext context,
            IEmailSender emailSender,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        // =====================================================
        // STEP 1: Customer submits the register form -> OTP is emailed.
        // No user is created yet.
        // =====================================================
        public async Task<OtpSentResponseDto> RegisterCustomerAsync(RegisterRequestDto requestDto)
        {
            var email = requestDto.Email.Trim().ToLower();

            if (await _userRepository.EmailExistsAsync(email))
                throw new ConflictException("User with this email already exists.");

            var now = DateTime.UtcNow;

            var pending = await _context.PendingRegistrations
                .FirstOrDefaultAsync(p => p.Email == email);

            if (pending != null)
                EnsureResendCooldownPassed(pending, now);

            var otp = GenerateOtp();

            if (pending == null)
            {
                pending = new PendingRegistration { Email = email, CreatedDate = now };
                await _context.PendingRegistrations.AddAsync(pending);
            }

            // A new register attempt overwrites the older pending details
            pending.FullName = requestDto.FullName.Trim();
            pending.PasswordHash = BCrypt.Net.BCrypt.HashPassword(requestDto.Password);
            pending.MobileNumber = requestDto.MobileNumber.Trim();
            pending.OtpHash = BCrypt.Net.BCrypt.HashPassword(otp);
            pending.OtpExpiry = now.AddMinutes(OtpValidityMinutes);
            pending.FailedAttempts = 0;
            pending.LastOtpSentAt = now;

            await _context.SaveChangesAsync();

            try
            {
                await SendOtpEmailAsync(email, pending.FullName, otp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send registration OTP to {Email}.", email);

                _context.PendingRegistrations.Remove(pending);
                await _context.SaveChangesAsync();

                throw new BadRequestException("Unable to send the verification email. Please try again.");
            }

            _logger.LogInformation("Registration OTP sent to {Email}.", email);

            return BuildOtpSentResponse(pending, "A verification OTP has been sent to your email.");
        }

        // =====================================================
        // STEP 2: Customer enters the OTP -> user is created.
        // =====================================================
        public async Task<UserResponseDto> VerifyRegistrationOtpAsync(VerifyOtpRequestDto requestDto)
        {
            var email = requestDto.Email.Trim().ToLower();

            var pending = await _context.PendingRegistrations
                .FirstOrDefaultAsync(p => p.Email == email);

            if (pending == null)
                throw new NotFoundException("No pending registration found for this email. Please register again.");

            if (pending.OtpExpiry < DateTime.UtcNow)
                throw new BadRequestException("OTP has expired. Please request a new OTP.");

            if (pending.FailedAttempts >= MaxOtpAttempts)
                throw new BadRequestException("Too many incorrect attempts. Please request a new OTP.");

            if (!BCrypt.Net.BCrypt.Verify(requestDto.Otp, pending.OtpHash))
            {
                pending.FailedAttempts++;
                await _context.SaveChangesAsync();

                var remaining = MaxOtpAttempts - pending.FailedAttempts;

                throw new BadRequestException(
                    remaining > 0
                        ? $"Invalid OTP. {remaining} attempt(s) remaining."
                        : "Too many incorrect attempts. Please request a new OTP.");
            }

            // Safety check: the email may have been registered in the meantime
            if (await _userRepository.EmailExistsAsync(email))
                throw new ConflictException("User with this email already exists.");

            var user = new User
            {
                FullName = pending.FullName,
                Email = pending.Email,
                Password = pending.PasswordHash,   // already hashed
                MobileNumber = pending.MobileNumber,
                Role = UserRole.Customer,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            _context.PendingRegistrations.Remove(pending);

            // Same scoped DbContext: the user is created and the pending row removed together
            await _context.SaveChangesAsync();

            _logger.LogInformation("Email {Email} verified. User {UserId} registered.", email, user.UserId);

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

        // =====================================================
        // RESEND OTP
        // =====================================================
        public async Task<OtpSentResponseDto> ResendRegistrationOtpAsync(ResendOtpRequestDto requestDto)
        {
            var email = requestDto.Email.Trim().ToLower();

            var pending = await _context.PendingRegistrations
                .FirstOrDefaultAsync(p => p.Email == email);

            if (pending == null)
                throw new NotFoundException("No pending registration found for this email. Please register again.");

            var now = DateTime.UtcNow;

            EnsureResendCooldownPassed(pending, now);

            var otp = GenerateOtp();

            pending.OtpHash = BCrypt.Net.BCrypt.HashPassword(otp);
            pending.OtpExpiry = now.AddMinutes(OtpValidityMinutes);
            pending.FailedAttempts = 0;
            pending.LastOtpSentAt = now;

            await _context.SaveChangesAsync();

            try
            {
                await SendOtpEmailAsync(email, pending.FullName, otp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend registration OTP to {Email}.", email);
                throw new BadRequestException("Unable to send the verification email. Please try again.");
            }

            _logger.LogInformation("Registration OTP resent to {Email}.", email);

            return BuildOtpSentResponse(pending, "A new OTP has been sent to your email.");
        }

        // ---------- OTP helpers (NEW) ----------

        private static string GenerateOtp()
        {
            // Cryptographically secure 6-digit code
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        private static void EnsureResendCooldownPassed(PendingRegistration pending, DateTime now)
        {
            var secondsSinceLast = (now - pending.LastOtpSentAt).TotalSeconds;

            if (secondsSinceLast < ResendCooldownSeconds)
            {
                var wait = (int)Math.Ceiling(ResendCooldownSeconds - secondsSinceLast);
                throw new BadRequestException($"Please wait {wait} second(s) before requesting another OTP.");
            }
        }

        private static OtpSentResponseDto BuildOtpSentResponse(PendingRegistration pending, string message)
        {
            return new OtpSentResponseDto
            {
                Message = message,
                Email = pending.Email,
                OtpExpiresAt = pending.OtpExpiry,
                ResendAvailableAt = pending.LastOtpSentAt.AddSeconds(ResendCooldownSeconds)
            };
        }

        private async Task SendOtpEmailAsync(string email, string fullName, string otp)
        {
            var body = $@"
<div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:24px;border:1px solid #e5e5e5;border-radius:8px'>
  <h2 style='color:#1a56db;margin-top:0'>Verify your email</h2>
  <p>Hello {fullName},</p>
  <p>Use the one-time password below to complete your registration:</p>
  <p style='font-size:32px;letter-spacing:8px;font-weight:bold;text-align:center;margin:24px 0'>{otp}</p>
  <p>This OTP is valid for <b>{OtpValidityMinutes} minutes</b>. Do not share it with anyone.</p>
  <p style='color:#888;font-size:12px'>If you did not request this, you can ignore this email.</p>
</div>";

            await _emailSender.SendAsync(email, "Your Insurance System verification code", body);
        }

        // =====================================================
        // EXISTING CODE (unchanged)
        // =====================================================

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
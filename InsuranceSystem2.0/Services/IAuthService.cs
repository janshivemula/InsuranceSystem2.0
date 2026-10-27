using InsuranceSystem2._0.DTOs.Auth;
using InsuranceSystem2._0.DTOs.User;

namespace InsuranceSystem2._0.Services.Interfaces
{
    public interface IAuthService
    {
        // CHANGED: registration is now a 2-step flow (OTP verification)
        Task<OtpSentResponseDto> RegisterCustomerAsync(RegisterRequestDto dto);   // step 1: send OTP
        Task<UserResponseDto> VerifyRegistrationOtpAsync(VerifyOtpRequestDto dto); // step 2: create user
        Task<OtpSentResponseDto> ResendRegistrationOtpAsync(ResendOtpRequestDto dto); // NEW

        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);

        Task<string> ForgotPasswordAsync(ForgotPasswordRequestDto dto);

        Task<bool> ResetPasswordAsync(ResetPasswordRequestDto dto);
    }
}
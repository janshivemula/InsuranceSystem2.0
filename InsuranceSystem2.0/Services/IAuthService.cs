using InsuranceSystem2._0.DTOs.Auth;
using InsuranceSystem2._0.DTOs.User;

namespace InsuranceSystem2._0.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterCustomerAsync(RegisterRequestDto dto);

        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);

        Task<string> ForgotPasswordAsync(ForgotPasswordRequestDto dto);

        Task<bool> ResetPasswordAsync(ResetPasswordRequestDto dto);
    }
}
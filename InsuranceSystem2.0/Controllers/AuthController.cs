using InsuranceSystem2._0.DTOs.Auth;
using InsuranceSystem2._0.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceSystem2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Constructor
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // CHANGED: Step 1 of registration. Validates the form and emails an OTP.
        // The user is NOT created here anymore.
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterCustomerAsync(requestDto);

            return Ok(result);
        }

        // NEW: Step 2 of registration. Verifies the OTP and creates the user.
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyRegistrationOtpAsync(requestDto);

            return Created("", result);
        }

        // NEW: Sends a fresh OTP (60 second cooldown).
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp(ResendOtpRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResendRegistrationOtpAsync(requestDto);

            return Ok(result);
        }

        // Login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(requestDto);

            return Ok(result);
        }

        // Forgot Password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var token = await _authService.ForgotPasswordAsync(requestDto);


            return Ok(new
            {
                message = "Password reset token generated successfully.",
                email = requestDto.Email,
                token = token
            });
        }

        // Reset Password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var result = await _authService.ResetPasswordAsync(requestDto);


            if (!result)
                return BadRequest("Password reset failed.");


            return Ok(new
            {
                message = "Password changed successfully."
            });
        }
    }
}

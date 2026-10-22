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

        // Customer Registration
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterCustomerAsync(requestDto);

            return Created("", result);
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

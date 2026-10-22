using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.Customer;
using InsuranceSystem2._0.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuranceSystem2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // Get All Customers (Admin / Internal Staff)
        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers([FromQuery] CustomerQueryDto query)
        {
            var customers = await _customerService.GetAllCustomersAsync(query);

            return Ok(new ApiResponse<PagedResponse<CustomerResponseDto>>
            {
                Success = true,
                Message = "Customers retrieved successfully.",
                Data = customers,
                Timestamp = DateTime.UtcNow
            });
        }

        // Get Active Customers
        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCustomers()
        {
            var customers = await _customerService.GetActiveCustomersAsync();

            return Ok(new ApiResponse<IEnumerable<CustomerResponseDto>>
            {
                Success = true,
                Message = "Active customers retrieved successfully.",
                Data = customers,
                Timestamp = DateTime.UtcNow
            });
        }

        // Get Customer By Id
        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Customer not found.",
                    Data = null,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<CustomerResponseDto>
            {
                Success = true,
                Message = "Customer retrieved successfully.",
                Data = customer,
                Timestamp = DateTime.UtcNow
            });
        }

        // Get Customer By User Id
        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetCustomerByUserId(int userId)
        {
            var customer = await _customerService.GetCustomerByUserIdAsync(userId);

            if (customer == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Customer not found.",
                    Data = null,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<CustomerResponseDto>
            {
                Success = true,
                Message = "Customer retrieved successfully.",
                Data = customer,
                Timestamp = DateTime.UtcNow
            });
        }

        // Get Logged-in Customer Profile
        [Authorize(Roles = "Customer")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var customer = await _customerService.GetMyProfileAsync(userId);

            if (customer == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Customer profile not found.",
                    Data = null,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<CustomerResponseDto>
            {
                Success = true,
                Message = "Customer profile retrieved successfully.",
                Data = customer,
                Timestamp = DateTime.UtcNow
            });
        }

        // Create Customer Profile
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromForm] CustomerRequestDto requestDto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var customer = await _customerService.CreateCustomerAsync(userId, requestDto, requestDto.ProfileImage);

            return CreatedAtAction(nameof(GetCustomerById),
                new { id = customer.CustomerId },
                new ApiResponse<CustomerResponseDto>
                {
                    Success = true,
                    Message = "Customer profile created successfully.",
                    Data = customer,
                    Timestamp = DateTime.UtcNow
                });
        }

        // Update Customer Profile
        [Authorize(Roles = "Customer")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromForm] CustomerRequestDto requestDto)
        {
            int loggedInUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var customer = await _customerService.UpdateCustomerAsync(id, loggedInUserId, requestDto, requestDto.ProfileImage);

            return Ok(new ApiResponse<CustomerResponseDto>
            {
                Success = true,
                Message = "Customer profile updated successfully.",
                Data = customer,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

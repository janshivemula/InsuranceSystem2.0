using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.PremiumPayent;
using InsuranceSystem2._0.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuranceSystem2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PremiumPaymentController : ControllerBase
    {
        private readonly IPremiumPaymentService _paymentService;

        public PremiumPaymentController(IPremiumPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // GET: api/PremiumPayment
        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpGet]
        public async Task<IActionResult> GetAllPayments([FromQuery] PaginationRequestDto paginationDto)
        {
            var payments = await _paymentService.GetAllPaymentsAsync(paginationDto);

            return Ok(new ApiResponse<PagedResponse<PremiumPaymentResponseDto>>
            {
                Success = true,
                Message = "Payments retrieved successfully.",
                Data = payments,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/PremiumPayment/5
        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);

            if (payment == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Payment not found.",
                    Data = null,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<PremiumPaymentResponseDto>
            {
                Success = true,
                Message = "Payment retrieved successfully.",
                Data = payment,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/PremiumPayment/policy/5
        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpGet("policy/{policyId:int}")]
        public async Task<IActionResult> GetPaymentsByPolicy(int policyId, [FromQuery] PaginationRequestDto paginationDto)
        {
            var payments = await _paymentService.GetPaymentsByPolicyIdAsync(policyId, paginationDto);

            return Ok(new ApiResponse<PagedResponse<PremiumPaymentResponseDto>>
            {
                Success = true,
                Message = "Policy payments retrieved successfully.",
                Data = payments,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/PremiumPayment/customer/5
        [Authorize(Roles = "Admin,InternalStaff,Customer")]
        [HttpGet("customer/{customerId:int}")]
        public async Task<IActionResult> GetPaymentsByCustomer(int customerId, [FromQuery] PaginationRequestDto paginationDto)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string role = User.FindFirst(ClaimTypes.Role)!.Value;

            var payments = await _paymentService.GetPaymentsByCustomerIdAsync(customerId, userId, role, paginationDto);

            return Ok(new ApiResponse<PagedResponse<PremiumPaymentResponseDto>>
            {
                Success = true,
                Message = "Customer payments retrieved successfully.",
                Data = payments,
                Timestamp = DateTime.UtcNow
            });
        }

        // POST: api/PremiumPayment
        [Authorize(Roles = "Customer,Admin,InternalStaff")]
        [HttpPost]
        public async Task<IActionResult> MakePayment([FromBody] PremiumPaymentRequestDto requestDto)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string role = User.FindFirst(ClaimTypes.Role)!.Value;

            var payment = await _paymentService.MakePaymentAsync(requestDto, userId, role);

            return CreatedAtAction(nameof(GetPaymentById),
                new { id = payment.PaymentId },
                new ApiResponse<PremiumPaymentResponseDto>
                {
                    Success = true,
                    Message = "Payment recorded successfully.",
                    Data = payment,
                    Timestamp = DateTime.UtcNow
                });
        }

    }
}

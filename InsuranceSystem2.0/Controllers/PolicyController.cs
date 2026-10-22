using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.Policy;
using InsuranceSystem2._0.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuranceSystem2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyService _policyService;

        public PolicyController(IPolicyService policyService)
        {
            _policyService = policyService;
        }

        // GET: api/Policy
        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpGet]
        public async Task<IActionResult> GetAllPolicies([FromQuery] PolicyQueryDto query)
        {
            var policies = await _policyService.GetAllPoliciesAsync(query);

            return Ok(new ApiResponse<PagedResponse<PolicyResponseDto>>
            {
                Success = true,
                Message = "Policies retrieved successfully.",
                Data = policies,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/Policy/active
        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpGet("active")]
        public async Task<IActionResult> GetActivePolicies()
        {
            var policies = await _policyService.GetActivePoliciesAsync();

            return Ok(new ApiResponse<IEnumerable<PolicyResponseDto>>
            {
                Success = true,
                Message = "Active policies retrieved successfully.",
                Data = policies,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/Policy/{id}
        [Authorize(Roles = "Admin,InternalStaff,Customer")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPolicyById(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string role = User.FindFirstValue(ClaimTypes.Role)!;

            var policy = await _policyService.GetPolicyByIdAsync(id, userId, role);

            return Ok(new ApiResponse<PolicyResponseDto>
            {
                Success = true,
                Message = "Policy retrieved successfully.",
                Data = policy,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/Policy/number/{policyNumber}
        [Authorize(Roles = "Admin,InternalStaff,Customer")]
        [HttpGet("number/{policyNumber}")]
        public async Task<IActionResult> GetPolicyByPolicyNumber(string policyNumber)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string role = User.FindFirstValue(ClaimTypes.Role)!;

            var policy = await _policyService.GetPolicyByPolicyNumberAsync(policyNumber, userId, role);

            return Ok(new ApiResponse<PolicyResponseDto>
            {
                Success = true,
                Message = "Policy retrieved successfully.",
                Data = policy,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/Policy/customer/{customerId}
        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpGet("customer/{customerId:int}")]
        public async Task<IActionResult> GetPoliciesByCustomer(int customerId)
        {
            var policies = await _policyService.GetPoliciesByCustomerIdAsync(customerId);

            return Ok(new ApiResponse<IEnumerable<PolicyResponseDto>>
            {
                Success = true,
                Message = "Customer policies retrieved successfully.",
                Data = policies,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/Policy/customer/{customerId}/active
        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpGet("customer/{customerId:int}/active")]
        public async Task<IActionResult> GetActivePoliciesByCustomer(int customerId)
        {
            var policies = await _policyService.GetActivePoliciesByCustomerIdAsync(customerId);

            return Ok(new ApiResponse<IEnumerable<PolicyResponseDto>>
            {
                Success = true,
                Message = "Active customer policies retrieved successfully.",
                Data = policies,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/Policy/my
        [Authorize(Roles = "Customer")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyPolicies()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var policies = await _policyService.GetMyPoliciesAsync(userId);

            return Ok(new ApiResponse<IEnumerable<PolicyResponseDto>>
            {
                Success = true,
                Message = "Your policies retrieved successfully.",
                Data = policies,
                Timestamp = DateTime.UtcNow
            });
        }

        // POST: api/Policy/purchase
        [Authorize(Roles = "Customer")]
        [HttpPost("purchase")]
        public async Task<IActionResult> PurchasePolicy([FromBody] PurchasePolicyRequestDto requestDto)
        {
            int customerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var policy = await _policyService.PurchasePolicyAsync(customerUserId, requestDto);

            return CreatedAtAction(nameof(GetPolicyById),
                new { id = policy.PolicyId },
                new ApiResponse<PolicyResponseDto>
                {
                    Success = true,
                    Message = "Policy purchased successfully. Waiting for premium payment.",
                    Data = policy,
                    Timestamp = DateTime.UtcNow
                });
        }

        // POST: api/Policy/issue
        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpPost("issue")]
        public async Task<IActionResult> IssuePolicy([FromBody] IssuePolicyRequestDto requestDto)
        {
            var policy = await _policyService.IssuePolicyByInternalStaffAsync(requestDto);

            return CreatedAtAction(nameof(GetPolicyById),
                new { id = policy.PolicyId },
                new ApiResponse<PolicyResponseDto>
                {
                    Success = true,
                    Message = "Policy issued successfully.",
                    Data = policy,
                    Timestamp = DateTime.UtcNow
                });
        }

        // PUT: api/Policy/cancel/{id}
        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> CancelPolicy(int id)
        {
            var policy = await _policyService.CancelPolicyAsync(id);

            return Ok(new ApiResponse<PolicyResponseDto>
            {
                Success = true,
                Message = "Policy cancelled successfully.",
                Data = policy,
                Timestamp = DateTime.UtcNow
            });
        }

    }
}

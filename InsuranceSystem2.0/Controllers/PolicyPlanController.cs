using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.Plan;
using InsuranceSystem2._0.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceSystem2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolicyPlanController : ControllerBase
    {
        private readonly IPolicyPlanService _planService;

        public PolicyPlanController(IPolicyPlanService planService)
        {
            _planService = planService;
        }

        // GET: api/PolicyPlan
        [Authorize(Roles = "Admin,InternalStaff,Customer")]
        [HttpGet]
        public async Task<IActionResult> GetAllPlans([FromQuery] PolicyPlanQueryDto query)
        {
            var plans = await _planService.GetAllPlansAsync(query);

            return Ok(new ApiResponse<PagedResponse<PlanResponseDto>>
            {
                Success = true,
                Message = "Policy plans retrieved successfully.",
                Data = plans,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/PolicyPlan/active
        [Authorize(Roles = "Admin,InternalStaff,Customer")]
        [HttpGet("active")]
        public async Task<IActionResult> GetActivePlans()
        {
            var plans = await _planService.GetActivePlansAsync();

            return Ok(new ApiResponse<IEnumerable<PlanResponseDto>>
            {
                Success = true,
                Message = "Active policy plans retrieved successfully.",
                Data = plans,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/PolicyPlan/{id}
        [Authorize(Roles = "Admin,InternalStaff,Customer")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPlanById(int id)
        {
            var plan = await _planService.GetPlanByIdAsync(id);

            if (plan == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Policy plan not found.",
                    Data = null,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<PlanResponseDto>
            {
                Success = true,
                Message = "Policy plan retrieved successfully.",
                Data = plan,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/PolicyPlan/product/{productId}
        [Authorize(Roles = "Admin,InternalStaff,Customer")]
        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetPlansByProductId(int productId)
        {
            var plans = await _planService.GetPlansByProductIdAsync(productId);

            return Ok(new ApiResponse<IEnumerable<PlanResponseDto>>
            {
                Success = true,
                Message = "Policy plans retrieved successfully.",
                Data = plans,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/PolicyPlan/product/{productId}/active
        [Authorize(Roles = "Admin,InternalStaff,Customer")]
        [HttpGet("product/{productId:int}/active")]
        public async Task<IActionResult> GetActivePlansByProductId(int productId)
        {
            var plans = await _planService.GetActivePlansByProductIdAsync(productId);

            return Ok(new ApiResponse<IEnumerable<PlanResponseDto>>
            {
                Success = true,
                Message = "Active policy plans retrieved successfully.",
                Data = plans,
                Timestamp = DateTime.UtcNow
            });
        }

        // POST: api/PolicyPlan
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreatePlan([FromBody] PlanRequestDto requestDto)
        {
            var plan = await _planService.CreatePlanAsync(requestDto);

            return CreatedAtAction(nameof(GetPlanById),
                new { id = plan.PlanId },
                new ApiResponse<PlanResponseDto>
                {
                    Success = true,
                    Message = "Policy plan created successfully.",
                    Data = plan,
                    Timestamp = DateTime.UtcNow
                });
        }

        // PUT: api/PolicyPlan/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePlan(int id, [FromBody] PlanRequestDto requestDto)
        {
            await _planService.UpdatePlanAsync(id, requestDto);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Policy plan updated successfully.",
                Data = null,
                Timestamp = DateTime.UtcNow
            });
        }

        // DELETE: api/PolicyPlan/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDeletePlan(int id)
        {
            await _planService.SoftDeletePlanAsync(id);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Policy plan deactivated successfully.",
                Data = null,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

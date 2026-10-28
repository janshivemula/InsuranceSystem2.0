using InsuranceSystem2._0.DTOs.Claim;
using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuranceSystem2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public ClaimController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        // Admin

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllClaims()
        {
            var claims = await _claimService.GetAllClaimsAsync();

            return Ok(new ApiResponse<IEnumerable<ClaimResponseDto>>
            {
                Success = true,
                Message = "Claims retrieved successfully.",
                Data = claims,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin,InternalStaff")]
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedClaims([FromQuery] ClaimPaginationRequestDto request)
        {
            var result = await _claimService.GetPagedClaimsAsync(request);

            return Ok(new ApiResponse<PagedResponse<ClaimResponseDto>>
            {
                Success = true,
                Message = "Claims retrieved successfully.",
                Data = result,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetClaimsByStatus(ClaimStatus status)
        {
            var claims = await _claimService.GetClaimsByStatusAsync(status);

            return Ok(new ApiResponse<IEnumerable<ClaimResponseDto>>
            {
                Success = true,
                Message = "Claims retrieved successfully.",
                Data = claims,
                Timestamp = DateTime.UtcNow
            });
        }

        // Customer

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreateClaim([FromForm] ClaimRequestDto requestDto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var claim = await _claimService.CreateClaimAsync(userId, requestDto);

            return Ok(new ApiResponse<ClaimResponseDto>
            {
                Success = true,
                Message = "Claim submitted successfully.",
                Data = claim,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyClaims()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var claims = await _claimService.GetMyClaimsAsync(userId);

            return Ok(new ApiResponse<IEnumerable<ClaimResponseDto>>
            {
                Success = true,
                Message = "Customer claims retrieved successfully.",
                Data = claims,
                Timestamp = DateTime.UtcNow
            });
        }

        // Common

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetClaimById(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

            var claim = await _claimService.GetClaimByIdAsync(id, userId, role);

            if (claim == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Claim not found.",
                    Data = null,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<ClaimResponseDto>
            {
                Success = true,
                Message = "Claim retrieved successfully.",
                Data = claim,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("number/{claimNumber}")]
        public async Task<IActionResult> GetClaimByNumber(string claimNumber)
        {
            var claim = await _claimService.GetClaimByClaimNumberAsync(claimNumber);

            if (claim == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Claim not found.",
                    Data = null,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<ClaimResponseDto>
            {
                Success = true,
                Message = "Claim retrieved successfully.",
                Data = claim,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("policy/{policyId:int}")]
        public async Task<IActionResult> GetClaimsByPolicy(int policyId)
        {
            var claims = await _claimService.GetClaimsByPolicyIdAsync(policyId);

            return Ok(new ApiResponse<IEnumerable<ClaimResponseDto>>
            {
                Success = true,
                Message = "Policy claims retrieved successfully.",
                Data = claims,
                Timestamp = DateTime.UtcNow
            });
        }

        // Claim Documents

        [HttpPost("document")]
        public async Task<IActionResult> AddDocument([FromForm] ClaimDocumentRequestDto requestDto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

            var document = await _claimService.AddClaimDocumentAsync(userId, role, requestDto);

            return Ok(new ApiResponse<ClaimDocumentResponseDto>
            {
                Success = true,
                Message = "Document uploaded successfully.",
                Data = document,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("{claimId:int}/documents")]
        public async Task<IActionResult> GetDocuments(int claimId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

            var documents = await _claimService.GetClaimDocumentsAsync(claimId, userId, role);

            return Ok(new ApiResponse<IEnumerable<ClaimDocumentResponseDto>>
            {
                Success = true,
                Message = "Documents retrieved successfully.",
                Data = documents,
                Timestamp = DateTime.UtcNow
            });
        }

        // Claim History

        [HttpGet("{claimId:int}/history")]
        public async Task<IActionResult> GetHistory(int claimId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

            var history = await _claimService.GetClaimStatusHistoryAsync(claimId, userId, role);

            return Ok(new ApiResponse<IEnumerable<ClaimStatusHistoryResponseDto>>
            {
                Success = true,
                Message = "Claim history retrieved successfully.",
                Data = history,
                Timestamp = DateTime.UtcNow
            });
        }

        // Internal Staff

        [Authorize(Roles = "InternalStaff")]
        [HttpPut("{claimId:int}/review")]
        public async Task<IActionResult> ReviewClaim(int claimId,[FromBody] ClaimReviewDto requestDto)
        {
            int staffUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var claim = await _claimService.ReviewClaimByStaffAsync(claimId, staffUserId, requestDto);

            return Ok(new ApiResponse<ClaimResponseDto>
            {
                Success = true,
                Message = "Claim reviewed successfully.",
                Data = claim,
                Timestamp = DateTime.UtcNow
            });
        }


        // Admin Decision

        [Authorize(Roles = "Admin")]
        [HttpPut("{claimId:int}/decision")]
        public async Task<IActionResult> DecideClaim(int claimId,[FromBody] ClaimDecisionDto requestDto)
        {
            int adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var claim = await _claimService.DecideClaimByAdminAsync(claimId, adminUserId, requestDto);

            return Ok(new ApiResponse<ClaimResponseDto>
            {
                Success = true,
                Message = "Claim decision recorded successfully.",
                Data = claim,
                Timestamp = DateTime.UtcNow
            });
        }

    }
}

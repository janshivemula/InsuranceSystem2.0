using AutoMapper;
using InsuranceSystem2._0.Data;
using InsuranceSystem2._0.DTOs.Claim;
using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Exceptions;
using InsuranceSystem2._0.Helpers;
using InsuranceSystem2._0.Models;
using InsuranceSystem2._0.Repositories.Interfaces;
using InsuranceSystem2._0.Services;
using InsuranceSystem2._0.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InsuranceManagementSystem.Services.Implementations
{
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IClaimDocumentRepository _claimDocumentRepository;
        private readonly IClaimStatusHistoryRepository _claimStatusHistoryRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly ILogger<ClaimService> _logger;
        private readonly IFileStorageService _fileStorageService;

        public ClaimService(
            IClaimRepository claimRepository,
            IClaimDocumentRepository claimDocumentRepository,
            IClaimStatusHistoryRepository claimStatusHistoryRepository,
            IPolicyRepository policyRepository,
            IUserRepository userRepository,
            ICustomerRepository customerRepository,
            IMapper mapper,
            AppDbContext context,
            ILogger<ClaimService> logger,
            IFileStorageService fileStorageService)
        {
            _claimRepository = claimRepository;
            _claimDocumentRepository = claimDocumentRepository;
            _claimStatusHistoryRepository = claimStatusHistoryRepository;
            _policyRepository = policyRepository;
            _userRepository = userRepository;
            _customerRepository = customerRepository;
            _mapper = mapper;
            _context = context;
            _logger = logger;
            _fileStorageService = fileStorageService;
        }

        public async Task<IEnumerable<ClaimResponseDto>> GetAllClaimsAsync()
        {
            var claims = await _claimRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ClaimResponseDto>>(claims);
        }

        public async Task<IEnumerable<ClaimResponseDto>> GetClaimsByCustomerIdAsync(int customerId)
        {
            var claims = await _claimRepository.GetClaimsByCustomerIdAsync(customerId);
            return _mapper.Map<IEnumerable<ClaimResponseDto>>(claims);
        }

        public async Task<IEnumerable<ClaimResponseDto>> GetMyClaimsAsync(int userId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);

            if (customer == null)
                throw new NotFoundException("Customer not found.");

            var claims = await _claimRepository.GetClaimsByCustomerIdAsync(customer.CustomerId);
            return _mapper.Map<IEnumerable<ClaimResponseDto>>(claims);
        }

        public async Task<IEnumerable<ClaimResponseDto>> GetClaimsByPolicyIdAsync(int policyId)
        {
            var policy = await _policyRepository.GetByIdAsync(policyId);

            if (policy == null)
                throw new NotFoundException("Policy not found.");

            var claims = await _claimRepository.GetClaimsByPolicyIdAsync(policyId);
            return _mapper.Map<IEnumerable<ClaimResponseDto>>(claims);
        }

        public async Task<IEnumerable<ClaimResponseDto>> GetClaimsByStatusAsync(ClaimStatus status)
        {
            var claims = await _claimRepository.GetClaimsByStatusAsync(status);
            return _mapper.Map<IEnumerable<ClaimResponseDto>>(claims);
        }

        public async Task<PagedResponse<ClaimResponseDto>> GetPagedClaimsAsync(ClaimPaginationRequestDto request)
        {
            var result = await _claimRepository.GetPagedClaimsAsync(request);

            var claimDtos = _mapper.Map<IEnumerable<ClaimResponseDto>>(result.Claims);

            return new PagedResponse<ClaimResponseDto>
            {
                Records = claimDtos,
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = (int)Math.Ceiling(result.TotalRecords / (double)request.PageSize),
                IsLastPage = request.PageNumber >= (int)Math.Ceiling(result.TotalRecords / (double)request.PageSize),
                SortField = request.SortBy,
                SortDirection = request.SortDirection
            };
        }


        public async Task<ClaimResponseDto?> GetClaimByIdAsync(int claimId, int userId, UserRole role)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);

            if (claim == null)
                return null;

            if (role == UserRole.Customer)
            {
                var customer = await _customerRepository.GetByUserIdAsync(userId);

                if (customer == null)
                    throw new NotFoundException("Customer not found for the logged-in user.");

                if (claim.CustomerId != customer.CustomerId)
                    throw new UnauthorizedAccessException("You are not authorized to access this claim.");
            }

            return _mapper.Map<ClaimResponseDto>(claim);
        }

        public async Task<ClaimResponseDto?> GetClaimByClaimNumberAsync(string claimNumber)
        {
            if (string.IsNullOrWhiteSpace(claimNumber))
                throw new BadRequestException("Claim number is required.");

            var claim = await _claimRepository.GetByClaimNumberAsync(claimNumber.Trim());

            if (claim == null)
                return null;

            return _mapper.Map<ClaimResponseDto>(claim);
        }

        public async Task<ClaimResponseDto> CreateClaimAsync(int userId, ClaimRequestDto requestDto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var customer = await _customerRepository.GetByUserIdAsync(userId);

                if (customer == null)
                    throw new NotFoundException("Customer not found for the logged-in user.");

                var policy = await _policyRepository.GetByIdAsync(requestDto.PolicyId);

                if (policy == null)
                    throw new NotFoundException("Policy not found.");

                if (policy.CustomerId != customer.CustomerId)
                    throw new BadRequestException("This policy does not belong to the customer.");

                if (!customer.IsActive)
                    throw new BadRequestException("Inactive customer cannot submit a claim.");

                if (!policy.Plan.IsActive)
                    throw new BadRequestException("Cannot submit a claim for an inactive policy plan.");

                if (!policy.Plan.InsuranceProduct.IsActive)
                    throw new BadRequestException("Cannot submit a claim for an inactive insurance product.");

                if (policy.PolicyStatus != PolicyStatus.Active)
                    throw new BadRequestException("Claims can only be submitted for active policies.");

                var incidentDate = DateOnly.FromDateTime(requestDto.IncidentDate);

                if (await _claimRepository.HasOpenClaimAsync(requestDto.PolicyId, incidentDate))
                    throw new ConflictException("An open claim already exists for this policy and incident date.");

                if (requestDto.ClaimAmount <= 0)
                    throw new BadRequestException("Claim amount must be greater than zero.");

                if (requestDto.ClaimAmount > policy.Plan.CoverageAmount)
                    throw new BadRequestException($"Claim amount cannot exceed policy coverage amount of {policy.Plan.CoverageAmount}.");

                if (incidentDate > DateOnly.FromDateTime(DateTime.UtcNow))
                    throw new BadRequestException("Incident date cannot be in the future.");

                if (incidentDate < policy.StartDate)
                    throw new BadRequestException("Incident date cannot be before the policy start date.");

                if (incidentDate > policy.EndDate)
                    throw new BadRequestException("Incident date cannot be after the policy expiry date.");

                if (string.IsNullOrWhiteSpace(requestDto.ClaimReason))
                    throw new BadRequestException("Claim reason is required.");

                if (requestDto.Document == null || requestDto.Document.Length == 0)
                    throw new BadRequestException("A supporting document is required.");

                var claim = new Claim
                {
                    ClaimNumber = await GenerateClaimNumberAsync(),
                    CustomerId = customer.CustomerId,
                    PolicyId = requestDto.PolicyId,
                    ClaimAmount = requestDto.ClaimAmount,
                    ClaimReason = requestDto.ClaimReason.Trim(),
                    IncidentDate = incidentDate,
                    ClaimStatus = ClaimStatus.Submitted,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                await _claimRepository.AddAsync(claim);
                await _claimRepository.SaveChangesAsync();

                var filePath = await _fileStorageService.SaveClaimDocumentAsync(requestDto.Document);

                var claimDocument = new ClaimDocument
                {
                    ClaimId = claim.ClaimId,
                    DocumentName = requestDto.DocumentName.Trim(),
                    DocumentType = requestDto.DocumentType.Trim(),
                    DocumentReference = filePath,
                    UploadedDate = DateTime.UtcNow
                };

                await _claimDocumentRepository.AddAsync(claimDocument);

                var history = new ClaimStatusHistory
                {
                    ClaimId = claim.ClaimId,
                    PreviousStatus = ClaimStatus.Submitted,
                    NewStatus = ClaimStatus.Submitted,
                    Remarks = "Claim Submitted",
                    UpdatedBy = userId,
                    UpdatedDate = DateTime.UtcNow
                };

                await _claimStatusHistoryRepository.AddAsync(history);

                await _claimDocumentRepository.SaveChangesAsync();
                await _claimStatusHistoryRepository.SaveChangesAsync();

                var createdClaim = await _claimRepository.GetByIdAsync(claim.ClaimId);

                if (createdClaim == null)
                    throw new BadRequestException("Claim could not be created.");

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Claim {ClaimNumber} submitted successfully by Customer {CustomerId}.",
                    claim.ClaimNumber,
                    customer.CustomerId);

                return _mapper.Map<ClaimResponseDto>(createdClaim);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<string> GenerateClaimNumberAsync()
        {
            string claimNumber;

            do
            {
                claimNumber = $"CLM{DateTime.UtcNow:yyMMddHHmmss}";
            }
            while (await _claimRepository.ClaimNumberExistsAsync(claimNumber));

            return claimNumber;
        }

        public async Task<ClaimDocumentResponseDto> AddClaimDocumentAsync(int userId, UserRole role, ClaimDocumentRequestDto requestDto)
        {
            if (role == UserRole.Customer)
            {
                await ValidateClaimOwnershipAsync(requestDto.ClaimId, userId);
            }

            var claim = await _claimRepository.GetByIdAsync(requestDto.ClaimId);

            if (claim == null)
                throw new NotFoundException("Claim not found.");

            if (claim.ClaimStatus == ClaimStatus.Approved || claim.ClaimStatus == ClaimStatus.Rejected)
                throw new BadRequestException("Cannot add documents after the claim has been closed.");

            var filePath = await _fileStorageService.SaveClaimDocumentAsync(requestDto.Document);

            var document = new ClaimDocument
            {
                ClaimId = requestDto.ClaimId,
                DocumentName = requestDto.DocumentName.Trim(),
                DocumentType = requestDto.DocumentType.Trim(),
                DocumentReference = filePath,
                UploadedDate = DateTime.UtcNow
            };

            await _claimDocumentRepository.AddAsync(document);
            await _claimDocumentRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Document '{DocumentName}' uploaded for Claim {ClaimId} by User {UserId}.",
                document.DocumentName,
                requestDto.ClaimId,
                userId);

            return _mapper.Map<ClaimDocumentResponseDto>(document);
        }


        public async Task<IEnumerable<ClaimDocumentResponseDto>> GetClaimDocumentsAsync(int claimId, int userId, UserRole role)
        {
            if (role == UserRole.Customer)
            {
                await ValidateClaimOwnershipAsync(claimId, userId);
            }

            var claim = await _claimRepository.GetByIdAsync(claimId);

            if (claim == null)
                throw new NotFoundException("Claim not found.");

            var documents = await _claimDocumentRepository.GetByClaimIdAsync(claimId);

            return _mapper.Map<IEnumerable<ClaimDocumentResponseDto>>(documents);
        }


        public async Task<IEnumerable<ClaimStatusHistoryResponseDto>> GetClaimStatusHistoryAsync(int claimId, int userId, UserRole role)
        {
            if (role == UserRole.Customer)
            {
                await ValidateClaimOwnershipAsync(claimId, userId);
            }

            var claim = await _claimRepository.GetByIdAsync(claimId);

            if (claim == null)
                throw new NotFoundException("Claim not found.");

            var histories = await _claimStatusHistoryRepository.GetByClaimIdAsync(claimId);

            return _mapper.Map<IEnumerable<ClaimStatusHistoryResponseDto>>(histories);
        }

        public async Task ReviewClaimByStaffAsync(int claimId, int staffUserId, ClaimReviewDto requestDto)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);

            if (claim == null)
                throw new NotFoundException("Claim not found.");

            var staff = await _userRepository.GetByIdAsync(staffUserId);

            if (staff == null)
                throw new NotFoundException("Internal staff not found.");

            if (staff.Role != UserRole.InternalStaff)
                throw new BadRequestException("Only Internal Staff can review claims.");

            if (!staff.IsActive)
                throw new BadRequestException("Inactive Internal Staff cannot review claims.");

            if (claim.ClaimStatus == ClaimStatus.Approved || claim.ClaimStatus == ClaimStatus.Rejected)
                throw new BadRequestException("Closed claims cannot be reviewed.");

            if (!ClaimStatusValidator.IsValidTransition(claim.ClaimStatus, requestDto.RecommendedStatus))
                throw new BadRequestException($"Invalid status transition from {claim.ClaimStatus} to {requestDto.RecommendedStatus}.");

            var previousStatus = claim.ClaimStatus;

            claim.ClaimStatus = requestDto.RecommendedStatus;
            claim.StaffRemarks = requestDto.Remarks.Trim();
            claim.UpdatedDate = DateTime.UtcNow;

            await _claimRepository.UpdateAsync(claim);

            var history = new ClaimStatusHistory
            {
                ClaimId = claim.ClaimId,
                PreviousStatus = previousStatus,
                NewStatus = requestDto.RecommendedStatus,
                Remarks = requestDto.Remarks.Trim(),
                UpdatedBy = staffUserId,
                UpdatedDate = DateTime.UtcNow
            };

            await _claimStatusHistoryRepository.AddAsync(history);

            await _claimRepository.SaveChangesAsync();
            await _claimStatusHistoryRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Internal Staff {StaffUserId} reviewed Claim {ClaimId}. Status changed from {OldStatus} to {NewStatus}.",
                staffUserId,
                claimId,
                previousStatus,
                requestDto.RecommendedStatus);
        }

        public async Task DecideClaimByAdminAsync(int claimId, int adminUserId, ClaimDecisionDto requestDto)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);

            if (claim == null)
                throw new NotFoundException("Claim not found.");

            var admin = await _userRepository.GetByIdAsync(adminUserId);

            if (admin == null)
                throw new NotFoundException("Admin not found.");

            if (admin.Role != UserRole.Admin)
                throw new BadRequestException("Only Admin can approve or reject claims.");

            if (!admin.IsActive)
                throw new BadRequestException("Inactive Admin cannot approve or reject claims.");

            if (claim.ClaimStatus == ClaimStatus.Approved || claim.ClaimStatus == ClaimStatus.Rejected)
                throw new BadRequestException("Claim has already been closed.");

            if (!ClaimStatusValidator.IsValidTransition(claim.ClaimStatus, requestDto.FinalDecisionStatus))
                throw new BadRequestException($"Invalid status transition from {claim.ClaimStatus} to {requestDto.FinalDecisionStatus}.");

            var previousStatus = claim.ClaimStatus;

            claim.ClaimStatus = requestDto.FinalDecisionStatus;
            claim.AdminRemarks = requestDto.Remarks.Trim();
            claim.UpdatedDate = DateTime.UtcNow;

            await _claimRepository.UpdateAsync(claim);

            var history = new ClaimStatusHistory
            {
                ClaimId = claim.ClaimId,
                PreviousStatus = previousStatus,
                NewStatus = requestDto.FinalDecisionStatus,
                Remarks = requestDto.Remarks.Trim(),
                UpdatedBy = adminUserId,
                UpdatedDate = DateTime.UtcNow
            };

            await _claimStatusHistoryRepository.AddAsync(history);

            await _claimRepository.SaveChangesAsync();
            await _claimStatusHistoryRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Admin {AdminUserId} made final decision on Claim {ClaimId}. Status changed from {OldStatus} to {NewStatus}.",
                adminUserId,
                claimId,
                previousStatus,
                requestDto.FinalDecisionStatus);
        }


        private async Task ValidateClaimOwnershipAsync(int claimId, int userId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);

            if (customer == null)
                throw new NotFoundException("Customer not found for the logged-in user.");

            bool ownsClaim = await _claimRepository.IsClaimOwnedByCustomerAsync(claimId, customer.CustomerId);

            if (!ownsClaim)
                throw new UnauthorizedAccessException("You are not authorized to access this claim.");
        }
    }
}
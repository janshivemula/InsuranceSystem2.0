using InsuranceSystem2._0.Repositories.Interfaces;
using InsuranceSystem2._0.Data;
using InsuranceSystem2._0.DTOs.Claim;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem2._0.Repositories.Implementations
{
    public class ClaimRepository : IClaimRepository
    {
        private readonly AppDbContext _context;

        public ClaimRepository(AppDbContext context)
        {
            _context = context;
        }

        // Select claim details and related navigation properties.
        private IQueryable<Claim> GetClaimQuery()
        {
            return _context.Claims.Select(c => new Claim
            {
                ClaimId = c.ClaimId,
                ClaimNumber = c.ClaimNumber,
                CustomerId = c.CustomerId,
                PolicyId = c.PolicyId,
                ClaimAmount = c.ClaimAmount,
                IncidentDate = c.IncidentDate,
                ClaimStatus = c.ClaimStatus,
                CreatedDate = c.CreatedDate,

                Customer = c.Customer == null ? null : new Customer
                {
                    CustomerId = c.Customer.CustomerId,
                    UserId = c.Customer.UserId,
                    User = c.Customer.User
                },

                Policy = c.Policy == null ? null : new Policy
                {
                    PolicyId = c.Policy.PolicyId,
                    Plan = c.Policy.Plan
                },

                ClaimDocuments = c.ClaimDocuments
                    .Select(d => d)
                    .ToList(),

                ClaimStatusHistories = c.ClaimStatusHistories
                    .Select(h => h)
                    .ToList()
            });
        }

        // Retrieves all claims.
        public async Task<IEnumerable<Claim>> GetAllAsync()
        {
            return await GetClaimQuery()
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        // Retrieves a claim by its unique identifier.
        public async Task<Claim?> GetByIdAsync(int id)
        {
            return await GetClaimQuery()
                .FirstOrDefaultAsync(c => c.ClaimId == id);
        }

        // Retrieves a claim by its business identifier.
        public async Task<Claim?> GetByClaimNumberAsync(
            string claimNumber)
        {
            return await GetClaimQuery()
                .FirstOrDefaultAsync(c => c.ClaimNumber == claimNumber);
        }

        // Retrieves claims submitted by a specific customer.
        public async Task<IEnumerable<Claim>> GetClaimsByCustomerIdAsync(
            int customerId)
        {
            return await GetClaimQuery()
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        // Retrieves claims for a specific policy.
        public async Task<IEnumerable<Claim>> GetClaimsByPolicyIdAsync(
            int policyId)
        {
            return await GetClaimQuery()
                .Where(c => c.PolicyId == policyId)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        // Retrieves claims with a specific status.
        public async Task<IEnumerable<Claim>> GetClaimsByStatusAsync(
            ClaimStatus status)
        {
            return await GetClaimQuery()
                .Where(c => c.ClaimStatus == status)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        // Adds a new claim.
        public async Task AddAsync(Claim claim)
        {
            await _context.Claims.AddAsync(claim);
        }

        // Updates an existing claim.
        public Task UpdateAsync(Claim claim)
        {
            _context.Claims.Update(claim);
            return Task.CompletedTask;
        }

        // Checks whether a claim number already exists.
        public async Task<bool> ClaimNumberExistsAsync(
            string claimNumber)
        {
            return await _context.Claims
                .AnyAsync(c => c.ClaimNumber == claimNumber);
        }

        // Retrieves paginated claims with filtering and sorting.
        public async Task<(IEnumerable<Claim> Claims, int TotalRecords)>
            GetPagedClaimsAsync(ClaimPaginationRequestDto request)
        {
            var query = GetClaimQuery();

            // Filtering
            if (request.ClaimStatus.HasValue)
            {
                query = query.Where(
                    c => c.ClaimStatus == request.ClaimStatus.Value);
            }

            if (request.CustomerId.HasValue)
            {
                query = query.Where(
                    c => c.CustomerId == request.CustomerId.Value);
            }

            if (request.PolicyId.HasValue)
            {
                query = query.Where(
                    c => c.PolicyId == request.PolicyId.Value);
            }

            // Sorting
            switch (request.SortBy.ToLower())
            {
                case "claimamount":
                    query = request.SortDirection.ToLower() == "asc"
                        ? query.OrderBy(c => c.ClaimAmount)
                        : query.OrderByDescending(c => c.ClaimAmount);
                    break;

                case "claimnumber":
                    query = request.SortDirection.ToLower() == "asc"
                        ? query.OrderBy(c => c.ClaimNumber)
                        : query.OrderByDescending(c => c.ClaimNumber);
                    break;

                default:
                    query = request.SortDirection.ToLower() == "asc"
                        ? query.OrderBy(c => c.CreatedDate)
                        : query.OrderByDescending(c => c.CreatedDate);
                    break;
            }

            int totalRecords = await query.CountAsync();

            var claims = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return (claims, totalRecords);
        }

        // Checks whether a claim belongs to a customer.
        public async Task<bool> IsClaimOwnedByCustomerAsync(
            int claimId, int customerId)
        {
            return await _context.Claims
                .AnyAsync(c =>
                    c.ClaimId == claimId &&
                    c.CustomerId == customerId);
        }

        // Checks whether an open claim exists for the policy
        // and incident date.
        public async Task<bool> HasOpenClaimAsync(
            int policyId, DateOnly incidentDate)
        {
            return await _context.Claims.AnyAsync(c =>
                c.PolicyId == policyId &&
                c.IncidentDate == incidentDate &&
                c.ClaimStatus != ClaimStatus.Rejected &&
                c.ClaimStatus != ClaimStatus.Approved);
        }

        // Saves all pending changes.
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

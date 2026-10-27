using InsuranceSystem2._0.Repositories.Interfaces;
using InsuranceSystem2._0.Data;
using InsuranceSystem2._0.DTOs.Policy;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Models;
using InsuranceSystem2._0.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem2._0.Repositories.Implementations
{
    public class PolicyRepository : IPolicyRepository
    {
        private readonly AppDbContext _context;

        // Constructor for Dependency Injection
        public PolicyRepository(AppDbContext context)
        {
            _context = context;
        }

        // Select policy details and related data.
        private IQueryable<Policy> GetPolicyQuery()
        {
            return _context.Policies.Select(p => new Policy
            {
                PolicyId = p.PolicyId,
                PolicyNumber = p.PolicyNumber,
                CustomerId = p.CustomerId,
                PlanId = p.PlanId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                PolicyStatus = p.PolicyStatus,
                CreatedDate = p.CreatedDate,

                // Include payment tracking properties
                InsuranceProductId = p.InsuranceProductId,   // NEW
                TotalPremiumPaid = p.TotalPremiumPaid,
                LastPaymentDate = p.LastPaymentDate,
                NextDueDate = p.NextDueDate,
                IsPaymentDue = p.IsPaymentDue,               // NEW
                InstallmentsPaid = p.InstallmentsPaid,       // NEW
                UpdatedDate = p.UpdatedDate,

                Customer = p.Customer == null ? null : new Customer
                {
                    CustomerId = p.Customer.CustomerId,
                    UserId = p.Customer.UserId,
                    IsActive = p.Customer.IsActive,
                    User = p.Customer.User
                },

                InsuranceProduct = p.InsuranceProduct == null
                    ? null
                    : p.InsuranceProduct,

                Plan = p.Plan == null ? null : new PolicyPlan
                {
                    PlanId = p.Plan.PlanId,
                    InsuranceProductId = p.Plan.InsuranceProductId,
                    PlanName = p.Plan.PlanName,
                    CoverageAmount = p.Plan.CoverageAmount,
                    PremiumAmount = p.Plan.PremiumAmount,
                    PremiumType = p.Plan.PremiumType,
                    DurationInYears = p.Plan.DurationInYears,
                    TermsAndConditions = p.Plan.TermsAndConditions,
                    IsActive = p.Plan.IsActive,
                    InsuranceProduct = p.Plan.InsuranceProduct
                },

                Payments = p.Payments.ToList()
            });
        }

        // Retrieves all policies with filtering, sorting and pagination.
        public async Task<PagedResult<Policy>> GetAllAsync(
            PolicyQueryDto query)
        {
            var policies = GetPolicyQuery();

            // Filtering
            if (query.Status.HasValue)
            {
                policies = policies.Where(
                    p => p.PolicyStatus == query.Status.Value);
            }

            if (query.CustomerId.HasValue)
            {
                policies = policies.Where(
                    p => p.CustomerId == query.CustomerId.Value);
            }

            if (query.PlanId.HasValue)
            {
                policies = policies.Where(
                    p => p.PlanId == query.PlanId.Value);
            }

            // Sorting
            policies = query.SortBy.ToLower() switch
            {
                "policynumber" =>
                    query.SortDirection.ToLower() == "asc"
                        ? policies.OrderBy(p => p.PolicyNumber)
                        : policies.OrderByDescending(p => p.PolicyNumber),

                "startdate" =>
                    query.SortDirection.ToLower() == "asc"
                        ? policies.OrderBy(p => p.StartDate)
                        : policies.OrderByDescending(p => p.StartDate),

                "enddate" =>
                    query.SortDirection.ToLower() == "asc"
                        ? policies.OrderBy(p => p.EndDate)
                        : policies.OrderByDescending(p => p.EndDate),

                "status" =>
                    query.SortDirection.ToLower() == "asc"
                        ? policies.OrderBy(p => p.PolicyStatus)
                        : policies.OrderByDescending(p => p.PolicyStatus),

                _ =>
                    query.SortDirection.ToLower() == "asc"
                        ? policies.OrderBy(p => p.CreatedDate)
                        : policies.OrderByDescending(p => p.CreatedDate)
            };

            int totalRecords = await policies.CountAsync();

            var records = await policies
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Policy>
            {
                Items = records,
                TotalRecords = totalRecords
            };
        }

        // Retrieves all policies for a specific customer.
        public async Task<IEnumerable<Policy>> GetPoliciesByCustomerIdAsync(
            int customerId)
        {
            return await GetPolicyQuery()
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        // Retrieves all policies for a specific user.
        public async Task<IEnumerable<Policy>> GetPoliciesByUserIdAsync(
            int userId)
        {
            return await GetPolicyQuery()
                .Where(p => p.Customer.UserId == userId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        // Retrieves all active policies.
        public async Task<IEnumerable<Policy>> GetActivePoliciesAsync()
        {
            return await GetPolicyQuery()
                .Where(p => p.PolicyStatus == PolicyStatus.Active)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        // Retrieves a policy by its unique identifier.
        public async Task<Policy?> GetByIdAsync(int id)
        {
            return await GetPolicyQuery()
                .FirstOrDefaultAsync(p => p.PolicyId == id);
        }

        // Retrieves a policy by its policy number.
        public async Task<Policy?> GetByPolicyNumberAsync(
            string policyNumber)
        {
            return await GetPolicyQuery()
                .FirstOrDefaultAsync(
                    p => p.PolicyNumber == policyNumber);
        }

        // Adds a new policy.
        public async Task AddAsync(Policy policy)
        {
            await _context.Policies.AddAsync(policy);
        }

        // Updates an existing policy.
        // CHANGED: copies only the fields the services modify, so a partial
        // projection can never overwrite InsuranceProductId, PlanId or CustomerId.
        public async Task UpdateAsync(Policy policy)
        {
            var tracked = await _context.Policies
                .FirstAsync(p => p.PolicyId == policy.PolicyId);

            tracked.PolicyStatus = policy.PolicyStatus;
            tracked.TotalPremiumPaid = policy.TotalPremiumPaid;
            tracked.LastPaymentDate = policy.LastPaymentDate;
            tracked.NextDueDate = policy.NextDueDate;
            tracked.IsPaymentDue = policy.IsPaymentDue;
            tracked.InstallmentsPaid = policy.InstallmentsPaid;
            tracked.UpdatedDate = policy.UpdatedDate;
        }

        // Saves all pending changes to the database.
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
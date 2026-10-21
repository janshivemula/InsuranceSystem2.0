using InsuranceSystem2._0.Repositories.Interfaces;
using InsuranceSystem2._0.Data;
using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem2._0.Repositories.Implementations
{
    public class PolicyPlanRepository : IPolicyPlanRepository
    {
        private readonly AppDbContext _context;

        // Constructor for Dependency Injection
        public PolicyPlanRepository(AppDbContext context)
        {
            _context = context;
        }

        // Select policy plan details and related data.
        private IQueryable<PolicyPlan> GetPolicyPlanQuery()
        {
            return _context.PolicyPlans.Select(p => new PolicyPlan
            {
                PlanId = p.PlanId,
                PlanName = p.PlanName,
                InsuranceProductId = p.InsuranceProductId,
                CoverageAmount = p.CoverageAmount,
                PremiumAmount = p.PremiumAmount,
                PremiumType = p.PremiumType,
                DurationInYears = p.DurationInYears,
                TermsAndConditions = p.TermsAndConditions,
                IsActive = p.IsActive,
                CreatedDate = p.CreatedDate,

                InsuranceProduct = p.InsuranceProduct,

                Policies = p.Policies.Select(policy => new Policy
                {
                    PolicyId = policy.PolicyId,
                    PolicyNumber = policy.PolicyNumber
                }).ToList()
            });
        }

        // Retrieves all policy plans with search, filtering,
        // sorting and pagination.
        public async Task<PagedResponse<PolicyPlan>> GetAllAsync(
            PolicyPlanQueryDto query)
        {
            var plans = GetPolicyPlanQuery();

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                plans = plans.Where(
                    p => p.PlanName.Contains(search));
            }

            // Filter by Product
            if (query.ProductId.HasValue)
            {
                plans = plans.Where(
                    p => p.InsuranceProductId == query.ProductId.Value);
            }

            // Filter by Premium Type
            if (query.PremiumType.HasValue)
            {
                plans = plans.Where(
                    p => p.PremiumType == query.PremiumType.Value);
            }

            // Filter by Active Status
            if (query.IsActive.HasValue)
            {
                plans = plans.Where(
                    p => p.IsActive == query.IsActive.Value);
            }

            // Sorting
            plans = query.SortBy.ToLower() switch
            {
                "coverageamount" => query.Descending
                    ? plans.OrderByDescending(p => p.CoverageAmount)
                    : plans.OrderBy(p => p.CoverageAmount),

                "premiumamount" => query.Descending
                    ? plans.OrderByDescending(p => p.PremiumAmount)
                    : plans.OrderBy(p => p.PremiumAmount),

                "durationyears" => query.Descending
                    ? plans.OrderByDescending(p => p.DurationInYears)
                    : plans.OrderBy(p => p.DurationInYears),

                "createddate" => query.Descending
                    ? plans.OrderByDescending(p => p.CreatedDate)
                    : plans.OrderBy(p => p.CreatedDate),

                _ => query.Descending
                    ? plans.OrderByDescending(p => p.PlanName)
                    : plans.OrderBy(p => p.PlanName)
            };

            var totalRecords = await plans.CountAsync();

            var records = await plans
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(
                (double)totalRecords / query.PageSize);

            return new PagedResponse<PolicyPlan>
            {
                Records = records,
                CurrentPage = query.PageNumber,
                PageSize = query.PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = query.PageNumber >= totalPages,
                SortField = query.SortBy,
                SortDirection = query.Descending ? "DESC" : "ASC"
            };
        }

        // Retrieves only active plans.
        public async Task<IEnumerable<PolicyPlan>> GetActivePlansAsync()
        {
            return await GetPolicyPlanQuery()
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        // Retrieves plans by product ID.
        public async Task<IEnumerable<PolicyPlan>> GetByProductIdAsync(
            int productId)
        {
            return await GetPolicyPlanQuery()
                .Where(p => p.InsuranceProductId == productId)
                .ToListAsync();
        }

        // Retrieves active plans by product ID.
        public async Task<IEnumerable<PolicyPlan>>
            GetActivePlansByProductIdAsync(int productId)
        {
            return await GetPolicyPlanQuery()
                .Where(p =>
                    p.InsuranceProductId == productId &&
                    p.IsActive)
                .ToListAsync();
        }

        // Retrieves a plan by ID.
        public async Task<PolicyPlan?> GetByIdAsync(int id)
        {
            return await GetPolicyPlanQuery()
                .FirstOrDefaultAsync(p => p.PlanId == id);
        }

        // Retrieves a plan by name.
        public async Task<PolicyPlan?> GetByNameAsync(string planName)
        {
            return await GetPolicyPlanQuery()
                .Where(p => p.PlanName == planName)
                .FirstOrDefaultAsync();
        }

        // Checks whether a plan name already exists.
        public async Task<bool> PlanNameExistsAsync(string planName)
        {
            return await _context.PolicyPlans
                .AnyAsync(p => p.PlanName == planName);
        }

        // Adds a new policy plan.
        public async Task AddAsync(PolicyPlan plan)
        {
            await _context.PolicyPlans.AddAsync(plan);
        }

        // Updates an existing policy plan.
        public Task UpdateAsync(PolicyPlan plan)
        {
            _context.PolicyPlans.Update(plan);
            return Task.CompletedTask;
        }

        // Soft delete: marks the plan inactive.
        public Task SoftDeleteAsync(PolicyPlan plan)
        {
            plan.IsActive = false;
            _context.PolicyPlans.Update(plan);
            return Task.CompletedTask;
        }

        // Saves changes.
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

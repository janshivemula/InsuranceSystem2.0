using InsuranceSystem2._0.Repositories.Interfaces;
using InsuranceSystem2._0.Data;
using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem2._0.Repositories.Implementations
{
    public class InsuranceProductRepository : IInsuranceProductRepository
    {
        private readonly AppDbContext _context;

        // Constructor for Dependency Injection
        public InsuranceProductRepository(AppDbContext context)
        {
            _context = context;
        }

        // Select product details and related policy plans and policies.
        private IQueryable<InsuranceProduct> GetProductQuery()
        {
            return _context.InsuranceProducts.Select(p => new InsuranceProduct
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductType = p.ProductType,
                Description = p.Description,
                IsActive = p.IsActive,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,

                PolicyPlans = p.PolicyPlans.Select(plan => new PolicyPlan
                {
                    PlanId = plan.PlanId,
                    PlanName = plan.PlanName,
                    InsuranceProductId = plan.InsuranceProductId,
                    CoverageAmount = plan.CoverageAmount,
                    PremiumAmount = plan.PremiumAmount,
                    PremiumType = plan.PremiumType,
                    DurationInYears = plan.DurationInYears,
                    IsActive = plan.IsActive,
                    CreatedDate = plan.CreatedDate
                }).ToList(),

                Policies = p.Policies.Select(policy => new Policy
                {
                    PolicyId = policy.PolicyId,
                    PolicyNumber = policy.PolicyNumber,
                    CustomerId = policy.CustomerId,
                    PlanId = policy.PlanId,
                    StartDate = policy.StartDate,
                    EndDate = policy.EndDate,
                    PolicyStatus = policy.PolicyStatus,
                    CreatedDate = policy.CreatedDate
                }).ToList()
            });
        }

        // Retrieves all insurance products with search,
        // filtering, sorting and pagination.
        public async Task<PagedResponse<InsuranceProduct>> GetAllAsync(
            ProductQueryDto query)
        {
            var products = GetProductQuery();

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                products = products.Where(
                    p => p.ProductName.Contains(search));
            }

            // Filter by Product Type
            if (query.ProductType.HasValue)
            {
                products = products.Where(
                    p => p.ProductType == query.ProductType.Value);
            }

            // Filter by Active Status
            if (query.IsActive.HasValue)
            {
                products = products.Where(
                    p => p.IsActive == query.IsActive.Value);
            }

            // Sorting
            products = query.SortBy.ToLower() switch
            {
                "createddate" => query.Descending
                    ? products.OrderByDescending(p => p.CreatedDate)
                    : products.OrderBy(p => p.CreatedDate),

                "producttype" => query.Descending
                    ? products.OrderByDescending(p => p.ProductType)
                    : products.OrderBy(p => p.ProductType),

                _ => query.Descending
                    ? products.OrderByDescending(p => p.ProductName)
                    : products.OrderBy(p => p.ProductName)
            };

            var totalRecords = await products.CountAsync();

            var records = await products
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(
                (double)totalRecords / query.PageSize);

            return new PagedResponse<InsuranceProduct>
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

        // Retrieves only active products.
        public async Task<IEnumerable<InsuranceProduct>>
            GetActiveProductsAsync()
        {
            return await GetProductQuery()
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        // Retrieves a product by ID.
        public async Task<InsuranceProduct?> GetByIdAsync(int id)
        {
            return await GetProductQuery()
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        // Retrieves a product by name.
        public async Task<InsuranceProduct?> GetByNameAsync(
            string productName)
        {
            return await GetProductQuery()
                .FirstOrDefaultAsync(p => p.ProductName == productName);
        }

        // Checks whether a product name already exists.
        public async Task<bool> ProductNameExistsAsync(string productName)
        {
            return await _context.InsuranceProducts
                .AnyAsync(p => p.ProductName == productName);
        }

        // Adds a new product.
        public async Task AddAsync(InsuranceProduct product)
        {
            await _context.InsuranceProducts.AddAsync(product);
        }

        // Updates a product.
        public Task UpdateAsync(InsuranceProduct product)
        {
            _context.InsuranceProducts.Update(product);
            return Task.CompletedTask;
        }

        // Soft delete: marks the product inactive.
        public Task SoftDeleteAsync(InsuranceProduct product)
        {
            product.IsActive = false;
            _context.InsuranceProducts.Update(product);
            return Task.CompletedTask;
        }

        // Saves changes.
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

using InsuranceSystem2._0.Repositories.Interfaces;
using InsuranceSystem2._0.Data;
using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem2._0.Repositories.Implementations
{
    public class PremiumPaymentRepository : IPremiumPaymentRepository
    {
        private readonly AppDbContext _context;

        // Constructor for Dependency Injection
        public PremiumPaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        // Select payment details and related policy/customer data.
        private IQueryable<PremiumPayment> GetPaymentQuery()
        {
            return _context.PremiumPayments.Select(p => new PremiumPayment
            {
                PaymentId = p.PaymentId,
                PolicyId = p.PolicyId,
                CustomerId = p.CustomerId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentStatus = p.PaymentStatus,
                PaymentMode = p.PaymentMode,
                TransactionReference = p.TransactionReference,

                Policy = p.Policy,

                Customer = p.Customer == null ? null : new Customer
                {
                    CustomerId = p.Customer.CustomerId,
                    UserId = p.Customer.UserId,
                    User = p.Customer.User
                }
            });
        }

        // Applies payment filters and sorting.
        private IQueryable<PremiumPayment> ApplyFiltersAndSorting(
            IQueryable<PremiumPayment> query,
            PaginationRequestDto paginationDto)
        {
            // Filter by Payment Status
            if (paginationDto.PaymentStatus.HasValue)
            {
                query = query.Where(
                    p => p.PaymentStatus ==
                         paginationDto.PaymentStatus.Value);
            }

            // Filter by Payment Mode
            if (paginationDto.PaymentMode.HasValue)
            {
                query = query.Where(
                    p => p.PaymentMode ==
                         paginationDto.PaymentMode.Value);
            }

            // Sorting
            switch (paginationDto.SortBy.ToLower())
            {
                case "amount":
                    query = paginationDto.SortDirection.ToLower() == "asc"
                        ? query.OrderBy(p => p.Amount)
                        : query.OrderByDescending(p => p.Amount);
                    break;

                case "paymentstatus":
                    query = paginationDto.SortDirection.ToLower() == "asc"
                        ? query.OrderBy(p => p.PaymentStatus)
                        : query.OrderByDescending(p => p.PaymentStatus);
                    break;

                case "paymentmode":
                    query = paginationDto.SortDirection.ToLower() == "asc"
                        ? query.OrderBy(p => p.PaymentMode)
                        : query.OrderByDescending(p => p.PaymentMode);
                    break;

                default:
                    query = paginationDto.SortDirection.ToLower() == "asc"
                        ? query.OrderBy(p => p.PaymentDate)
                        : query.OrderByDescending(p => p.PaymentDate);
                    break;
            }

            return query;
        }

        // Builds the paginated response.
        private async Task<PagedResponse<PremiumPayment>> GetPagedResponseAsync(
            IQueryable<PremiumPayment> query,
            PaginationRequestDto paginationDto)
        {
            int totalRecords = await query.CountAsync();

            var records = await query
                .Skip((paginationDto.PageNumber - 1) * paginationDto.PageSize)
                .Take(paginationDto.PageSize)
                .ToListAsync();

            return new PagedResponse<PremiumPayment>
            {
                Records = records,
                CurrentPage = paginationDto.PageNumber,
                PageSize = paginationDto.PageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(
                    totalRecords / (double)paginationDto.PageSize),
                IsLastPage =
                    paginationDto.PageNumber * paginationDto.PageSize
                    >= totalRecords,
                SortField = paginationDto.SortBy,
                SortDirection = paginationDto.SortDirection
            };
        }

        // Retrieves all premium payments.
        public async Task<PagedResponse<PremiumPayment>> GetAllAsync(
            PaginationRequestDto paginationDto)
        {
            var query = ApplyFiltersAndSorting(
                GetPaymentQuery(), paginationDto);

            return await GetPagedResponseAsync(query, paginationDto);
        }

        // Retrieves payments for a specific policy.
        public async Task<PagedResponse<PremiumPayment>>
            GetPaymentsByPolicyIdAsync(
                int policyId,
                PaginationRequestDto paginationDto)
        {
            var query = GetPaymentQuery()
                .Where(p => p.PolicyId == policyId);

            query = ApplyFiltersAndSorting(query, paginationDto);

            return await GetPagedResponseAsync(query, paginationDto);
        }

        // Retrieves payments made by a specific customer.
        public async Task<PagedResponse<PremiumPayment>>
            GetPaymentsByCustomerIdAsync(
                int customerId,
                PaginationRequestDto paginationDto)
        {
            var query = GetPaymentQuery()
                .Where(p => p.CustomerId == customerId);

            query = ApplyFiltersAndSorting(query, paginationDto);

            return await GetPagedResponseAsync(query, paginationDto);
        }

        // Retrieves a payment by its unique identifier.
        public async Task<PremiumPayment?> GetByIdAsync(int id)
        {
            return await GetPaymentQuery()
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        // Retrieves a payment by its transaction reference.
        public async Task<PremiumPayment?>
            GetByTransactionReferenceAsync(string transactionReference)
        {
            return await GetPaymentQuery()
                .FirstOrDefaultAsync(
                    p => p.TransactionReference == transactionReference);
        }

        // Adds a new premium payment.
        public async Task AddAsync(PremiumPayment premiumPayment)
        {
            await _context.PremiumPayments.AddAsync(premiumPayment);
        }

        // Updates an existing premium payment.
        public Task UpdateAsync(PremiumPayment premiumPayment)
        {
            _context.PremiumPayments.Update(premiumPayment);
            return Task.CompletedTask;
        }

        // Saves all pending changes to the database.
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

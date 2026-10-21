using InsuranceSystem2._0.Repositories.Interfaces;
using InsuranceSystem2._0.Data;
using InsuranceSystem2._0.DTOs.Customer;
using InsuranceSystem2._0.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem2._0.Repositories.Implementations
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        // Constructor for Dependency Injection
        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        // Retrieves all customers with filtering, sorting and pagination.
        public async Task<PagedResult<Customer>> GetAllAsync(CustomerQueryDto query)
        {
            var customers = _context.Customers
                .Select(c => new Customer
                {
                    CustomerId = c.CustomerId,
                    UserId = c.UserId,
                    DateOfBirth = c.DateOfBirth,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    PinCode = c.PinCode,
                    NomineeName = c.NomineeName,
                    NomineeRelation = c.NomineeRelation,
                    ProfileImageUrl = c.ProfileImageUrl,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,

                    User = c.User,

                    Policies = c.Policies,
                    PremiumPayments = c.PremiumPayments
                });

            // Filtering
            if (query.IsActive.HasValue)
            {
                customers = customers.Where(
                    c => c.IsActive == query.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.City))
            {
                var city = query.City.Trim();
                customers = customers.Where(
                    c => c.City.ToLower() == city.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(query.State))
            {
                var state = query.State.Trim();
                customers = customers.Where(
                    c => c.State.ToLower() == state.ToLower());
            }

            // Sorting
            customers = (query.SortBy.ToLower(),
                         query.SortDirection.ToLower()) switch
            {
                ("city", "asc") =>
                    customers.OrderBy(c => c.City),

                ("city", "desc") =>
                    customers.OrderByDescending(c => c.City),

                ("state", "asc") =>
                    customers.OrderBy(c => c.State),

                ("state", "desc") =>
                    customers.OrderByDescending(c => c.State),

                ("createddate", "asc") =>
                    customers.OrderBy(c => c.CreatedDate),

                ("createddate", "desc") =>
                    customers.OrderByDescending(c => c.CreatedDate),

                _ => throw new InvalidOperationException(
                    "Invalid sorting options.")
            };

            var totalRecords = await customers.CountAsync();

            var records = await customers
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Customer>
            {
                Items = records,
                TotalRecords = totalRecords
            };
        }

        // Retrieves all active customers.
        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _context.Customers
                .Where(c => c.IsActive)
                .Select(c => new Customer
                {
                    CustomerId = c.CustomerId,
                    UserId = c.UserId,
                    DateOfBirth = c.DateOfBirth,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    PinCode = c.PinCode,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    User = c.User
                })
                .OrderBy(c => c.CustomerId)
                .ToListAsync();
        }

        // Retrieves a customer by its unique identifier.
        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers
                .Where(c => c.CustomerId == id)
                .Select(c => new Customer
                {
                    CustomerId = c.CustomerId,
                    UserId = c.UserId,
                    DateOfBirth = c.DateOfBirth,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    PinCode = c.PinCode,
                    NomineeName = c.NomineeName,
                    NomineeRelation = c.NomineeRelation,
                    ProfileImageUrl = c.ProfileImageUrl,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    User = c.User,
                    Policies = c.Policies,
                    PremiumPayments = c.PremiumPayments
                })
                .FirstOrDefaultAsync();
        }

        // Retrieves a customer by the associated user ID.
        public async Task<Customer?> GetByUserIdAsync(int userId)
        {
            return await _context.Customers
                .Where(c => c.UserId == userId)
                .Select(c => new Customer
                {
                    CustomerId = c.CustomerId,
                    UserId = c.UserId,
                    DateOfBirth = c.DateOfBirth,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    PinCode = c.PinCode,
                    NomineeName = c.NomineeName,
                    NomineeRelation = c.NomineeRelation,
                    ProfileImageUrl = c.ProfileImageUrl,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    User = c.User
                })
                .FirstOrDefaultAsync();
        }

        // Adds a new customer.
        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
        }

        // Updates an existing customer.
        public Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            return Task.CompletedTask;
        }

        // Saves all pending changes to the database.
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

using InsuranceSystem2._0.Data;
using InsuranceSystem2._0.DTOs.User;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Models;
using InsuranceSystem2._0.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem2._0.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        // Get all users with Pagination, Sorting and Filtering
        public async Task<PagedResult<User>> GetAllAsync(UserQueryDto query)
        {
            IQueryable<User> users = _context.Users.AsNoTracking();

            // Filtering
            if (query.Role.HasValue)
            {
                users = users.Where(u => u.Role == query.Role.Value);
            }

            if (query.IsActive.HasValue)
            {
                users = users.Where(u => u.IsActive == query.IsActive.Value);
            }

            // Validate Sort Direction
            if (!query.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase) &&
                !query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("SortDirection must be either 'asc' or 'desc'.");
            }

            // Sorting
            switch (query.SortBy.ToLower())
            {
                case "fullname":
                    users = query.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                        ? users.OrderBy(u => u.FullName)
                        : users.OrderByDescending(u => u.FullName);
                    break;

                case "email":
                    users = query.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                        ? users.OrderBy(u => u.Email)
                        : users.OrderByDescending(u => u.Email);
                    break;

                case "role":
                    users = query.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                        ? users.OrderBy(u => u.Role)
                        : users.OrderByDescending(u => u.Role);
                    break;

                case "createddate":
                    users = query.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                        ? users.OrderBy(u => u.CreatedDate)
                        : users.OrderByDescending(u => u.CreatedDate);
                    break;

                default:
                    throw new ArgumentException("Invalid sort field.");
            }

            // Total Records
            int totalRecords = await users.CountAsync();

            // Pagination
            var records = await users
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<User>
            {
                Items = records,
                TotalRecords = totalRecords
            };
        }

        // Get Active Users
        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        // Get Active Internal Staff
        public async Task<IEnumerable<User>> GetActiveInternalStaffAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == UserRole.InternalStaff && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        // Get User By Id
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        // Get User By Email
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdForUpdateAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == id);
        }

        // Check Email Exists
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        // Add User
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        // Update User
        public Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return false;

            user.IsActive = isActive;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // Soft Delete (Deactivate User)
        public Task SoftDeleteAsync(User user)
        {
            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;

            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        // Save Changes
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
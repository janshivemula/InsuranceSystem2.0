using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem2._0.Data
{
    public class DbSeeder
    {
        public static async Task SeedAdminAsync(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            bool adminExists = await context.Users.AnyAsync(u => u.Role == UserRole.Admin);

            if (adminExists)
                return;

            var adminUser = new User
            {
                FullName = "System Admin",
                Email = "admin@insurance.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                //Password = "Admin@1234" : no safety 
                MobileNumber = "9999999999",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }
    }
}

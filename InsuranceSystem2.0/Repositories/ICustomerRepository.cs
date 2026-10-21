using InsuranceSystem2._0.DTOs.Customer;
using InsuranceSystem2._0.Models;

namespace InsuranceSystem2._0.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<PagedResult<Customer>> GetAllAsync(CustomerQueryDto query);
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();

        Task<Customer?> GetByIdAsync(int id);

        Task<Customer?> GetByUserIdAsync(int userId);

        Task AddAsync(Customer customer);

        Task UpdateAsync(Customer customer);

        Task SaveChangesAsync();
    }
}
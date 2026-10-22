using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.Customer;
using Microsoft.AspNetCore.Http;

namespace InsuranceSystem2._0.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<PagedResponse<CustomerResponseDto>> GetAllCustomersAsync(CustomerQueryDto query);

        Task<IEnumerable<CustomerResponseDto>> GetActiveCustomersAsync();

        Task<CustomerResponseDto?> GetCustomerByIdAsync(int id);

        Task<CustomerResponseDto?> GetCustomerByUserIdAsync(int userId);
        Task<CustomerResponseDto?> GetMyProfileAsync(int loggedInUserId);

        Task<CustomerResponseDto> CreateCustomerAsync(int userId, CustomerRequestDto requestDto, IFormFile? profileImage);

        Task<CustomerResponseDto> UpdateCustomerAsync(int id, int loggedInUserId, CustomerRequestDto requestDto, IFormFile? profileImage);
    }
}
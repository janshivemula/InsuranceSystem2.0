using AutoMapper;
using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.Customer;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Exceptions;
using InsuranceSystem2._0.Models;
using InsuranceSystem2._0.Repositories.Interfaces;
using InsuranceSystem2._0.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InsureFlowAPI.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;
        private readonly ICloudinaryService _cloudinaryService;

        public CustomerService(
            ICustomerRepository customerRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<CustomerService> logger,
            ICloudinaryService cloudinaryService)
        {
            _customerRepository = customerRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }

        // Get all customers
        public async Task<PagedResponse<CustomerResponseDto>> GetAllCustomersAsync(CustomerQueryDto query)
        {
            if (query.PageNumber < 1)
                throw new BadRequestException("Page number must be greater than zero.");

            if (query.PageSize < 1)
                throw new BadRequestException("Page size must be greater than zero.");

            if (query.PageSize > 100)
                throw new BadRequestException("Maximum page size is 100.");

            var allowedSortFields = new[]
                {
                    "city",
                    "state",
                    "createddate"
                };

            if (string.IsNullOrWhiteSpace(query.SortBy))
            {
                query.SortBy = "CreatedDate";
            }

            query.SortBy = query.SortBy.Trim();

            if (!allowedSortFields.Contains(query.SortBy,StringComparer.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Invalid sort field. Allowed fields are: city, state, createddate");
            }

            if (!allowedSortFields.Contains(query.SortBy.ToLower()))
                throw new BadRequestException("Invalid sort field. Allowed fields are: city, state, createddate");

            var allowedDirections = new[] { "asc", "desc" };
            if (string.IsNullOrWhiteSpace(query.SortDirection))
                query.SortDirection = "desc";

            if (!allowedDirections.Contains(query.SortDirection.ToLower()))
                throw new BadRequestException("Sort direction must be either 'asc' or 'desc'.");

            var result = await _customerRepository.GetAllAsync(query);

            var customerDtos = _mapper.Map<IEnumerable<CustomerResponseDto>>(result.Items);

            var totalPages = (int)Math.Ceiling((double)result.TotalRecords / query.PageSize);

            return new PagedResponse<CustomerResponseDto>
            {
                Records = customerDtos,
                CurrentPage = query.PageNumber,
                PageSize = query.PageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = totalPages,
                IsLastPage = query.PageNumber >= totalPages,
                SortField = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        // Get active customers
        public async Task<IEnumerable<CustomerResponseDto>> GetActiveCustomersAsync()
        {
            var customers = await _customerRepository.GetActiveCustomersAsync();
            return _mapper.Map<IEnumerable<CustomerResponseDto>>(customers);
        }

        // Get customer by customer id
        public async Task<CustomerResponseDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);

            if (customer == null)
            {
                _logger.LogWarning("Customer not found. CustomerId: {CustomerId}", id);
                return null;
            }

            return _mapper.Map<CustomerResponseDto>(customer);
        }

        // Get customer by user id
        public async Task<CustomerResponseDto?> GetCustomerByUserIdAsync(int userId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);

            if (customer == null)
            {
                _logger.LogWarning("Customer not found for UserId: {UserId}", userId);
                return null;
            }

            return _mapper.Map<CustomerResponseDto>(customer);
        }

        // Logged-in customer gets own profile
        public async Task<CustomerResponseDto?> GetMyProfileAsync(int loggedInUserId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(loggedInUserId);

            if (customer == null)
            {
                _logger.LogWarning("Customer profile not found for UserId: {UserId}", loggedInUserId);
                return null;
            }

            if (!customer.User.IsActive)
                throw new BadRequestException("Inactive user cannot access profile.");

            return _mapper.Map<CustomerResponseDto>(customer);
        }

        // Create customer profile after auth registration
        public async Task<CustomerResponseDto> CreateCustomerAsync(int userId, CustomerRequestDto requestDto, IFormFile? profileImage)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new NotFoundException($"User with Id {userId} not found.");

            if (user.Role != UserRole.Customer)
                throw new BadRequestException("Only users with Customer role can create customer profile.");

            if (!user.IsActive)
                throw new BadRequestException("Inactive user cannot create customer profile.");


            var existingCustomer = await _customerRepository.GetByUserIdAsync(userId);

            if (existingCustomer != null)
                throw new ConflictException("Customer profile already exists.");


            var dob = DateOnly.FromDateTime(requestDto.DateOfBirth);


            if (dob > DateOnly.FromDateTime(DateTime.UtcNow))
                throw new BadRequestException("Date of birth cannot be in the future.");


            var age = DateTime.Today.Year - requestDto.DateOfBirth.Year;

            if (requestDto.DateOfBirth.Date > DateTime.Today.AddYears(-age))
                age--;


            if (age < 18)
                throw new BadRequestException("Customer must be at least 18 years old.");


            var customer = new Customer
            {
                UserId = userId,

                DateOfBirth = dob,

                Address = requestDto.Address.Trim(),

                City = requestDto.City.Trim(),

                State = requestDto.State.Trim(),

                PinCode = requestDto.PinCode.Trim(),

                NomineeName = requestDto.NomineeName.Trim(),

                NomineeRelation = requestDto.NomineeRelation.Trim(),


                // Cloudinary URL stored here
                ProfileImageUrl = null,


                IsActive = true,

                CreatedDate = DateTime.UtcNow,

                UpdatedDate = DateTime.UtcNow
            };

            if (profileImage != null)
            {
                customer.ProfileImageUrl =
                    await _cloudinaryService.UploadImageAsync(profileImage);
            }


            await _customerRepository.AddAsync(customer);

            await _customerRepository.SaveChangesAsync();



            var createdCustomer = await _customerRepository.GetByUserIdAsync(userId);


            if (createdCustomer == null)
                throw new BadRequestException("Customer profile could not be created.");


            _logger.LogInformation(
                "Customer profile created successfully for UserId {UserId}",
                userId);


            return _mapper.Map<CustomerResponseDto>(createdCustomer);
        }

        // Update own customer profile
        public async Task<CustomerResponseDto> UpdateCustomerAsync(
          int id, int loggedInUserId, CustomerRequestDto requestDto, IFormFile? profileImage)
        {
            var customer = await _customerRepository.GetByIdAsync(id);


            if (customer == null)
                throw new NotFoundException($"Customer with Id {id} not found.");


            if (customer.UserId != loggedInUserId)
                throw new UnauthorizedAccessException("You can update only your own profile.");


            if (!customer.User.IsActive)
                throw new BadRequestException("Inactive customer cannot be updated.");



            var dob = DateOnly.FromDateTime(requestDto.DateOfBirth);


            if (dob > DateOnly.FromDateTime(DateTime.UtcNow))
                throw new BadRequestException("Date of birth cannot be in the future.");



            var age = DateTime.Today.Year - requestDto.DateOfBirth.Year;


            if (requestDto.DateOfBirth.Date > DateTime.Today.AddYears(-age))
                age--;


            if (age < 18)
                throw new BadRequestException("Customer must be at least 18 years old.");



            customer.DateOfBirth = dob;

            customer.Address = requestDto.Address.Trim();

            customer.City = requestDto.City.Trim();

            customer.State = requestDto.State.Trim();

            customer.PinCode = requestDto.PinCode.Trim();

            customer.NomineeName = requestDto.NomineeName.Trim();

            customer.NomineeRelation = requestDto.NomineeRelation.Trim();

            if (profileImage != null)
            {
                // Upload new image first
                var newImageUrl = await _cloudinaryService.UploadImageAsync(profileImage);

                // Delete old image only after successful upload
                if (!string.IsNullOrWhiteSpace(customer.ProfileImageUrl))
                {
                    await _cloudinaryService.DeleteImageAsync(customer.ProfileImageUrl);
                }

                customer.ProfileImageUrl = newImageUrl;
            }


            customer.UpdatedDate = DateTime.UtcNow;



            await _customerRepository.UpdateAsync(customer);

            await _customerRepository.SaveChangesAsync();



            var updatedCustomer =
                await _customerRepository.GetByIdAsync(id);



            if (updatedCustomer == null)
                throw new BadRequestException("Customer profile could not be updated.");



            _logger.LogInformation(
                "Customer profile updated successfully. CustomerId: {CustomerId}",
                id);



            return _mapper.Map<CustomerResponseDto>(updatedCustomer);
        }
    }
}
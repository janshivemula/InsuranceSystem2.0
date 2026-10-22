using AutoMapper;
using InsuranceSystem2._0.Services.Interfaces;
using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.Product;
using InsuranceSystem2._0.Enums;
using InsuranceSystem2._0.Exceptions;
using InsuranceSystem2._0.Models;
using InsuranceSystem2._0.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace InsuranceSystem2._0.Services.Implementations
{
    public class InsuranceProductService : IInsuranceProductService
    {
        private readonly IInsuranceProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<InsuranceProductService> _logger;

        public InsuranceProductService(IInsuranceProductRepository productRepository,
        IMapper mapper, ILogger<InsuranceProductService> logger)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // Get all products
        public async Task<PagedResponse<ProductResponseDto>> GetAllProductsAsync(ProductQueryDto query)
        {
            var pagedProducts = await _productRepository.GetAllAsync(query);

            _logger.LogInformation("Retrieved insurance products. PageNumber: {PageNumber}, PageSize: {PageSize}",
                query.PageNumber, query.PageSize);

            return new PagedResponse<ProductResponseDto>
            {
                Records = _mapper.Map<IEnumerable<ProductResponseDto>>(pagedProducts.Records),
                CurrentPage = pagedProducts.CurrentPage,
                PageSize = pagedProducts.PageSize,
                TotalRecords = pagedProducts.TotalRecords,
                TotalPages = pagedProducts.TotalPages,
                IsLastPage = pagedProducts.IsLastPage,
                SortField = pagedProducts.SortField,
                SortDirection = pagedProducts.SortDirection
            };
        }

        // Get active products
        public async Task<IEnumerable<ProductResponseDto>> GetActiveProductsAsync()
        {
            var products = await _productRepository.GetActiveProductsAsync();
            _logger.LogInformation("Retrieved active insurance products.");

            return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
        }

        // Get product by Id
        public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            _logger.LogInformation("Retrieved insurance product with Id {ProductId}.", id);

            if (product == null)
                return null;

            return _mapper.Map<ProductResponseDto>(product);
        }

        // Get product by name
        public async Task<ProductResponseDto?> GetProductByNameAsync(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new BadRequestException("Product name is required.");

            var trimmedName = productName.Trim();

            var product = await _productRepository.GetByNameAsync(trimmedName);
            _logger.LogInformation("Retrieved insurance product by name '{ProductName}'.", trimmedName);

            if (product == null)
                return null;

            return _mapper.Map<ProductResponseDto>(product);
        }

        // Create product
        public async Task<ProductResponseDto> CreateProductAsync(ProductRequestDto requestDto)
        {
            ValidateProduct(requestDto);
            var trimmedName = requestDto.ProductName.Trim();

            if (string.IsNullOrWhiteSpace(trimmedName))
                throw new BadRequestException("Product name is required.");

            if (await _productRepository.ProductNameExistsAsync(trimmedName))
                throw new ConflictException("Product with this name already exists.");

            var product = new InsuranceProduct
            {
                ProductName = trimmedName,
                ProductType = Enum.Parse<ProductType>(requestDto.ProductType.Trim(), true),
                Description = requestDto.Description.Trim(),
                IsActive = requestDto.IsActive,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Insurance product '{ProductName}' created successfully with Id {ProductId}.",
                product.ProductName, product.ProductId);

            var createdProduct = await _productRepository.GetByIdAsync(product.ProductId);

            if (createdProduct == null)
                throw new BadRequestException("Product could not be created.");

            return _mapper.Map<ProductResponseDto>(createdProduct);
        }

        // Update product
        public async Task<ProductResponseDto> UpdateProductAsync(int id, ProductRequestDto requestDto)
        {
            ValidateProduct(requestDto);

            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                throw new NotFoundException($"Product with Id {id} not found.");

            var trimmedName = requestDto.ProductName.Trim();

            if (string.IsNullOrWhiteSpace(trimmedName))
                throw new BadRequestException("Product name is required.");

            var existingProduct = await _productRepository.GetByNameAsync(trimmedName);

            if (existingProduct != null && existingProduct.ProductId != id)
                throw new ConflictException("Another product with this name already exists.");

            product.ProductName = trimmedName;
            product.ProductType = Enum.Parse<ProductType>(requestDto.ProductType.Trim(), true);
            product.Description = requestDto.Description.Trim();
            product.IsActive = requestDto.IsActive;
            product.UpdatedDate = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Insurance product with Id {ProductId} updated successfully.", id);

            var updatedProduct = await _productRepository.GetByIdAsync(id);

            return _mapper.Map<ProductResponseDto>(updatedProduct);
        }

        // Soft delete product
        public async Task SoftDeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                throw new NotFoundException($"Product with Id {id} not found.");

            if (!product.IsActive)
                throw new BadRequestException("Product is already inactive.");

            await _productRepository.SoftDeleteAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Insurance product with Id {ProductId} deactivated successfully.", id);
        }
        private static void ValidateProduct(ProductRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ProductName))
                throw new BadRequestException("Product name is required.");

            if (dto.ProductName.Trim().Length < 3)
                throw new BadRequestException("Product name must contain at least 3 characters.");

            if (dto.ProductName.Trim().Length > 100)
                throw new BadRequestException("Product name cannot exceed 100 characters.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new BadRequestException("Description is required.");

            if (dto.Description.Trim().Length < 10)
                throw new BadRequestException("Description must contain at least 10 characters.");

            if (dto.Description.Trim().Length > 500)
                throw new BadRequestException("Description cannot exceed 500 characters.");

            if (!Enum.IsDefined(typeof(ProductType), dto.ProductType))
                throw new BadRequestException("Invalid product type.");
        }
    }
}
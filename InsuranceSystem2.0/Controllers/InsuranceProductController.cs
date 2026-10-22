using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.DTOs.Product;
using InsuranceSystem2._0.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceSystem2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsuranceProductController : ControllerBase
    {
        private readonly IInsuranceProductService _productService;

        public InsuranceProductController(IInsuranceProductService productService)
        {
            _productService = productService;
        }

        // GET: api/InsuranceProduct
        [Authorize(Roles = "Admin,InternalStaff,Customer")]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] ProductQueryDto query)
        {
            var products = await _productService.GetAllProductsAsync(query);

            return Ok(new ApiResponse<PagedResponse<ProductResponseDto>>
            {
                Success = true,
                Message = "Insurance products retrieved successfully.",
                Data = products,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/InsuranceProduct/active
        [Authorize(Roles = "Admin,InternalStaff,Customer")]
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveProducts()
        {
            var products = await _productService.GetActiveProductsAsync();

            return Ok(new ApiResponse<IEnumerable<ProductResponseDto>>
            {
                Success = true,
                Message = "Active insurance products retrieved successfully.",
                Data = products,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/InsuranceProduct/5
        [Authorize(Roles = "Admin,InternalStaff,Customer")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Insurance product not found.",
                    Data = null,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<ProductResponseDto>
            {
                Success = true,
                Message = "Insurance product retrieved successfully.",
                Data = product,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET: api/InsuranceProduct/name/{productName}
        [Authorize(Roles = "Admin,InternalStaff,Customer")]
        [HttpGet("name/{productName}")]
        public async Task<IActionResult> GetProductByName(string productName)
        {
            var product = await _productService.GetProductByNameAsync(productName);

            if (product == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Insurance product not found.",
                    Data = null,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<ProductResponseDto>
            {
                Success = true,
                Message = "Insurance product retrieved successfully.",
                Data = product,
                Timestamp = DateTime.UtcNow
            });
        }

        // POST: api/InsuranceProduct
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductRequestDto requestDto)
        {
            var product = await _productService.CreateProductAsync(requestDto);

            return CreatedAtAction(nameof(GetProductById),
                new { id = product.ProductId },
                new ApiResponse<ProductResponseDto>
                {
                    Success = true,
                    Message = "Insurance product created successfully.",
                    Data = product,
                    Timestamp = DateTime.UtcNow
                });
        }

        // PUT: api/InsuranceProduct/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductRequestDto requestDto)
        {
            var product = await _productService.UpdateProductAsync(id, requestDto);

            return Ok(new ApiResponse<ProductResponseDto>
            {
                Success = true,
                Message = "Insurance product updated successfully.",
                Data = product,
                Timestamp = DateTime.UtcNow
            });
        }

        // DELETE: api/InsuranceProduct/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDeleteProduct(int id)
        {
            await _productService.SoftDeleteProductAsync(id);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Insurance product deactivated successfully.",
                Data = null,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

#region References
using BidSphere.Models.Dtos.Products;
using BidSphere.Service.Interface;
using BidSphere.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
#endregion

namespace BidSphere.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly CreateProductDtoValidator _createValidator;
        private readonly UpdateProductDtoValidator _updateValidator;

        public ProductsController(
            IProductService productService,
            CreateProductDtoValidator createValidator,
            UpdateProductDtoValidator updateValidator)
        {
            _productService = productService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        /// <summary>
        /// Get all products with optional filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetProducts(
            [FromQuery] string? status,
            [FromQuery] string? category,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var products = await _productService.GetAllProductsAsync(status, category, minPrice, maxPrice);
            return Ok(products);
        }

        /// <summary>
        /// Get only active auctions
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult> GetActiveAuctions()
        {
            var products = await _productService.GetActiveAuctionsAsync();
            return Ok(products);
        }

        /// <summary>
        /// Get product details with auction info
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }
            return Ok(product);
        }

        /// <summary>
        /// Create a new product (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateProduct(CreateProductDto createDto)
        {
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            try
            {
                var ownerId = int.Parse(userIdClaim);
                var product = await _productService.CreateProductAsync(createDto, ownerId);
                return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update a product (Admin only, no active bids)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateProduct(int id, UpdateProductDto updateDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            try
            {
                var product = await _productService.UpdateProductAsync(id, updateDto);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a product (Admin only, no active bids)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
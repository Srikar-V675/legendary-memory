using BidSphere.Controllers;
using BidSphere.Models.Dtos.Products;
using BidSphere.Service.Interface;
using BidSphere.Validators;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace BidSphere.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IExcelService> _excelServiceMock;
        private readonly Mock<CreateProductDtoValidator> _createValidatorMock;
        private readonly Mock<UpdateProductDtoValidator> _updateValidatorMock;
        private readonly Mock<ILogger<ProductsController>> _loggerMock;
        private readonly ProductsController _controller;

        public ProductControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _excelServiceMock = new Mock<IExcelService>();
            _createValidatorMock = new Mock<CreateProductDtoValidator>();
            _updateValidatorMock = new Mock<UpdateProductDtoValidator>();
            _loggerMock = new Mock<ILogger<ProductsController>>();

            _controller = new ProductsController(
                _productServiceMock.Object,
                _excelServiceMock.Object,
                _createValidatorMock.Object,
                _updateValidatorMock.Object,
                _loggerMock.Object);

            SetupUserContext();
        }

        private void SetupUserContext()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetProducts_WithoutFilters_ShouldReturnAllProducts()
        {
            // Arrange
            var products = new List<ProductDto>
            {
                new ProductDto { ProductId = 1, Name = "Product 1" },
                new ProductDto { ProductId = 2, Name = "Product 2" }
            };

            _productServiceMock.Setup(x => x.GetAllProductsAsync(null))
                .ReturnsAsync(products);

            // Act
            var result = await _controller.GetProducts(null, null, null);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(products);
        }

        [Fact]
        public async Task GetProducts_WithPagination_ShouldReturnPagedResults()
        {
            // Arrange
            var products = new List<ProductDto>
            {
                new ProductDto { ProductId = 1, Name = "Product 1" },
                new ProductDto { ProductId = 2, Name = "Product 2" },
                new ProductDto { ProductId = 3, Name = "Product 3" }
            };

            _productServiceMock.Setup(x => x.GetAllProductsAsync(null))
                .ReturnsAsync(products);

            // Act
            var result = await _controller.GetProducts(null, 1, 2);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var pagedProducts = okResult!.Value as IEnumerable<ProductDto>;
            pagedProducts.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetActiveAuctions_ShouldReturnOnlyActiveAuctions()
        {
            // Arrange
            var activeProducts = new List<ProductDto>
            {
                new ProductDto { ProductId = 1, Name = "Active Product" }
            };

            _productServiceMock.Setup(x => x.GetActiveAuctionsAsync())
                .ReturnsAsync(activeProducts);

            // Act
            var result = await _controller.GetActiveAuctions();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(activeProducts);
        }

        [Fact]
        public async Task GetProduct_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            var productId = 1;
            var product = new AuctionDetailsDto { ProductId = productId, Name = "Test Product" };

            _productServiceMock.Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(product);
        }

        [Fact]
        public async Task GetProduct_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var productId = 999;

            _productServiceMock.Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync((AuctionDetailsDto?)null);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // Note: Validator tests removed as FluentValidation's ValidateAsync cannot be mocked with Moq
        // The validators have their own comprehensive test suite in Validators/ folder

        [Fact]
        public async Task DeleteProduct_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var productId = 1;

            _productServiceMock.Setup(x => x.DeleteProductAsync(productId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteProduct(productId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _productServiceMock.Verify(x => x.DeleteProductAsync(productId), Times.Once);
        }

        [Fact]
        public async Task UploadProducts_WithValidFile_ShouldReturnUploadResult()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("products.xlsx");
            fileMock.Setup(f => f.Length).Returns(1024);

            var uploadResult = new ExcelUploadResultDto
            {
                SuccessCount = 5,
                FailedCount = 0,
                FailedRows = new List<FailedRowDto>()
            };

            _excelServiceMock.Setup(x => x.ParseProductsFromExcelAsync(fileMock.Object, 1))
                .ReturnsAsync(uploadResult);

            // Act
            var result = await _controller.UploadProducts(fileMock.Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(uploadResult);
        }

        [Fact]
        public async Task UploadProducts_WithNullFile_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.UploadProducts(null!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UploadProducts_WithInvalidFileType_ShouldReturnBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("products.txt");
            fileMock.Setup(f => f.Length).Returns(1024);

            // Act
            var result = await _controller.UploadProducts(fileMock.Object);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}

using AutoMapper;
using BidSphere.Exceptions;
using BidSphere.Models.Domain;
using BidSphere.Models.Dtos.Products;
using BidSphere.Models.Enums;
using BidSphere.Repository.Interface;
using BidSphere.Service.Implementation;
using BidSphere.Service.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Xunit;

namespace BidSphere.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IAuctionRepository> _auctionRepositoryMock;
        private readonly Mock<IAsqlParserService> _asqlParserMock;
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<IBidRepository> _bidRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _auctionRepositoryMock = new Mock<IAuctionRepository>();
            _asqlParserMock = new Mock<IAsqlParserService>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _bidRepositoryMock = new Mock<IBidRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _productService = new ProductService(
                _mapperMock.Object,
                _productRepositoryMock.Object,
                _auctionRepositoryMock.Object,
                _asqlParserMock.Object,
                _paymentRepositoryMock.Object,
                _bidRepositoryMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task GetAllProductsAsync_WithoutAsql_ShouldReturnAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, Name = "Product 1" },
                new Product { ProductId = 2, Name = "Product 2" }
            };

            var productDtos = new List<ProductDto>
            {
                new ProductDto { ProductId = 1, Name = "Product 1" },
                new ProductDto { ProductId = 2, Name = "Product 2" }
            };

            _productRepositoryMock.Setup(x => x.GetAllProductsAsync())
                .ReturnsAsync(products);

            _mapperMock.Setup(x => x.Map<IEnumerable<ProductDto>>(It.IsAny<IQueryable<Product>>()))
                .Returns(productDtos);

            // Act
            var result = await _productService.GetAllProductsAsync(null);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(productDtos);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            var productId = 1;
            var product = new Product { ProductId = productId, Name = "Test Product" };
            var auctionDetailsDto = new AuctionDetailsDto { ProductId = productId };

            _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _mapperMock.Setup(x => x.Map<AuctionDetailsDto>(product))
                .Returns(auctionDetailsDto);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result.ProductId.Should().Be(productId);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var productId = 999;

            _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateProductAsync_WithValidData_ShouldCreateProduct()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "New Product",
                StartingPrice = 100,
                AuctionDurationMinutes = 60
            };
            var ownerId = 1;

            var product = new Product { ProductId = 1, Name = "New Product" };
            var auction = new Auction { AuctionId = 1, ProductId = 1 };
            var productDto = new ProductDto { ProductId = 1, Name = "New Product" };

            _mapperMock.Setup(x => x.Map<Product>(createDto))
                .Returns(product);

            _productRepositoryMock.Setup(x => x.AddProduct(It.IsAny<Product>()))
                .ReturnsAsync(product);

            _auctionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Auction>()))
                .ReturnsAsync(auction);

            _productRepositoryMock.Setup(x => x.GetByIdAsync(product.ProductId))
                .ReturnsAsync(product);

            _mapperMock.Setup(x => x.Map<ProductDto>(product))
                .Returns(productDto);

            // Act
            var result = await _productService.CreateProductAsync(createDto, ownerId);

            // Assert
            result.Should().NotBeNull();
            result.ProductId.Should().Be(1);
            _auctionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Auction>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WithNonExistentProduct_ShouldThrowProductNotFoundException()
        {
            // Arrange
            var productId = 999;
            var updateDto = new UpdateProductDto { Name = "Updated" };

            _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ProductNotFoundException>(
                () => _productService.UpdateProductAsync(productId, updateDto)
            );
        }

        [Fact]
        public async Task UpdateProductAsync_WithActiveBids_ShouldThrowInvalidBidException()
        {
            // Arrange
            var productId = 1;
            var updateDto = new UpdateProductDto { Name = "Updated" };
            var product = new Product { ProductId = productId };

            _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _productRepositoryMock.Setup(x => x.HasActiveBidsAsync(productId))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidBidException>(
                () => _productService.UpdateProductAsync(productId, updateDto)
            );
        }

        [Fact]
        public async Task DeleteProductAsync_WithNonExistentProduct_ShouldThrowProductNotFoundException()
        {
            // Arrange
            var productId = 999;

            _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ProductNotFoundException>(
                () => _productService.DeleteProductAsync(productId)
            );
        }

        [Fact]
        public async Task DeleteProductAsync_WithActiveBids_ShouldThrowInvalidBidException()
        {
            // Arrange
            var productId = 1;
            var product = new Product { ProductId = productId };

            _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _productRepositoryMock.Setup(x => x.HasActiveBidsAsync(productId))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidBidException>(
                () => _productService.DeleteProductAsync(productId)
            );
        }

        [Fact]
        public async Task GetActiveAuctionsAsync_ShouldReturnOnlyActiveAuctions()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    Auction = new Auction { Status = AuctionStatus.Active }
                },
                new Product
                {
                    ProductId = 2,
                    Auction = new Auction { Status = AuctionStatus.Expired }
                },
                new Product
                {
                    ProductId = 3,
                    Auction = new Auction { Status = AuctionStatus.Active }
                }
            };

            var productDtos = new List<ProductDto>
            {
                new ProductDto { ProductId = 1 },
                new ProductDto { ProductId = 3 }
            };

            _productRepositoryMock.Setup(x => x.GetAllProductsAsync())
                .ReturnsAsync(products);

            _mapperMock.Setup(x => x.Map<IEnumerable<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
                .Returns(productDtos);

            // Act
            var result = await _productService.GetActiveAuctionsAsync();

            // Assert
            result.Should().HaveCount(2);
        }
    }
}

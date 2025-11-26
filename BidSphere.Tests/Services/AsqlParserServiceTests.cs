using BidSphere.Models.Domain;
using BidSphere.Service.Implementation;
using FluentAssertions;
using Xunit;

namespace BidSphere.Tests.Services
{
    public class AsqlParserServiceTests
    {
        private readonly AsqlParserService _asqlParser;
        private readonly List<Product> _testProducts;

        public AsqlParserServiceTests()
        {
            _asqlParser = new AsqlParserService();
            _testProducts = new List<Product>
            {
                new Product { ProductId = 1, Name = "iPhone", Category = "Electronics", StartingPrice = 1000 },
                new Product { ProductId = 2, Name = "Watch", Category = "Fashion", StartingPrice = 500 },
                new Product { ProductId = 3, Name = "Painting", Category = "Art", StartingPrice = 2000 },
                new Product { ProductId = 4, Name = "Laptop", Category = "Electronics", StartingPrice = 1500 }
            };
        }

        [Fact]
        public void ApplyAsqlFilter_WithEqualityOperator_ShouldFilterCorrectly()
        {
            // Arrange
            var query = _testProducts.AsQueryable();
            var asql = "category=\"Electronics\"";

            // Act
            var result = _asqlParser.ApplyAsqlFilter(query, asql).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(p => p.Category.Should().Be("Electronics"));
        }

        [Fact]
        public void ApplyAsqlFilter_WithGreaterThanOperator_ShouldFilterCorrectly()
        {
            // Arrange
            var query = _testProducts.AsQueryable();
            var asql = "startingPrice>1000";

            // Act
            var result = _asqlParser.ApplyAsqlFilter(query, asql).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(p => p.StartingPrice.Should().BeGreaterThan(1000));
        }

        [Fact]
        public void ApplyAsqlFilter_WithLessThanOperator_ShouldFilterCorrectly()
        {
            // Arrange
            var query = _testProducts.AsQueryable();
            var asql = "startingPrice<1000";

            // Act
            var result = _asqlParser.ApplyAsqlFilter(query, asql).ToList();

            // Assert
            result.Should().HaveCount(1);
            result.First().StartingPrice.Should().BeLessThan(1000);
        }

        [Fact]
        public void ApplyAsqlFilter_WithAndOperator_ShouldFilterCorrectly()
        {
            // Arrange
            var query = _testProducts.AsQueryable();
            var asql = "category=\"Electronics\" AND startingPrice>1000";

            // Act
            var result = _asqlParser.ApplyAsqlFilter(query, asql).ToList();

            // Assert
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Laptop");
        }

        [Fact]
        public void ApplyAsqlFilter_WithOrOperator_ShouldFilterCorrectly()
        {
            // Arrange
            var query = _testProducts.AsQueryable();
            var asql = "category=\"Art\" OR category=\"Fashion\"";

            // Act
            var result = _asqlParser.ApplyAsqlFilter(query, asql).ToList();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public void ApplyAsqlFilter_WithInOperator_ShouldFilterCorrectly()
        {
            // Arrange
            var query = _testProducts.AsQueryable();
            var asql = "category in [\"Electronics\", \"Art\"]";

            // Act
            var result = _asqlParser.ApplyAsqlFilter(query, asql).ToList();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public void ApplyAsqlFilter_WithNotEqualOperator_ShouldFilterCorrectly()
        {
            // Arrange
            var query = _testProducts.AsQueryable();
            var asql = "category!=\"Electronics\"";

            // Act
            var result = _asqlParser.ApplyAsqlFilter(query, asql).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.Should().NotContain(p => p.Category == "Electronics");
        }

        [Fact]
        public void ApplyAsqlFilter_WithEmptyQuery_ShouldReturnAll()
        {
            // Arrange
            var query = _testProducts.AsQueryable();
            var asql = "";

            // Act
            var result = _asqlParser.ApplyAsqlFilter(query, asql).ToList();

            // Assert
            result.Should().HaveCount(4);
        }
    }
}

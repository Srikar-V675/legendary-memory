using BidSphere.Models.Domain;
using BidSphere.Models.Enums;
using BidSphere.Repository.Interface;
using BidSphere.Service.Implementation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using OfficeOpenXml;
using Xunit;

namespace BidSphere.Tests.Services
{
    public class ExcelServiceTests : IDisposable
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IAuctionRepository> _auctionRepositoryMock;
        private readonly ExcelService _excelService;

        public ExcelServiceTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _auctionRepositoryMock = new Mock<IAuctionRepository>();
            _excelService = new ExcelService(_productRepositoryMock.Object, _auctionRepositoryMock.Object);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [Fact]
        public async Task ParseProductsFromExcelAsync_WithValidData_ShouldCreateProducts()
        {
            // Arrange
            var file = CreateValidExcelFile();
            var ownerId = 1;

            _productRepositoryMock.Setup(x => x.AddProduct(It.IsAny<Product>()))
                .ReturnsAsync((Product p) => { p.ProductId = 1; return p; });

            _auctionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Auction>()))
                .ReturnsAsync((Auction a) => { a.AuctionId = 1; return a; });

            // Act
            var result = await _excelService.ParseProductsFromExcelAsync(file, ownerId);

            // Assert
            result.Should().NotBeNull();
            result.SuccessCount.Should().BeGreaterThan(0);
            result.FailedCount.Should().Be(0);
            _productRepositoryMock.Verify(x => x.AddProduct(It.IsAny<Product>()), Times.AtLeastOnce);
            _auctionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Auction>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ParseProductsFromExcelAsync_WithEmptyFile_ShouldThrowException()
        {
            // Arrange
            var file = CreateEmptyExcelFile();
            var ownerId = 1;

            // Act
            var act = async () => await _excelService.ParseProductsFromExcelAsync(file, ownerId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*empty*");
        }

        [Fact]
        public async Task ParseProductsFromExcelAsync_WithMissingColumns_ShouldThrowException()
        {
            // Arrange
            var file = CreateExcelFileWithMissingColumns();
            var ownerId = 1;

            // Act
            var act = async () => await _excelService.ParseProductsFromExcelAsync(file, ownerId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Missing required column*");
        }

        [Fact]
        public async Task ParseProductsFromExcelAsync_WithInvalidPrice_ShouldRecordFailedRow()
        {
            // Arrange
            var file = CreateExcelFileWithInvalidPrice();
            var ownerId = 1;

            // Act
            var result = await _excelService.ParseProductsFromExcelAsync(file, ownerId);

            // Assert
            result.Should().NotBeNull();
            result.FailedCount.Should().BeGreaterThan(0);
            result.FailedRows.Should().Contain(f => f.Reason.Contains("Invalid starting price"));
        }

        [Fact]
        public async Task ParseProductsFromExcelAsync_WithInvalidDuration_ShouldRecordFailedRow()
        {
            // Arrange
            var file = CreateExcelFileWithInvalidDuration();
            var ownerId = 1;

            // Act
            var result = await _excelService.ParseProductsFromExcelAsync(file, ownerId);

            // Assert
            result.Should().NotBeNull();
            result.FailedCount.Should().BeGreaterThan(0);
            result.FailedRows.Should().Contain(f => f.Reason.Contains("Duration must be between"));
        }

        [Fact]
        public async Task ParseProductsFromExcelAsync_WithMissingName_ShouldRecordFailedRow()
        {
            // Arrange
            var file = CreateExcelFileWithMissingName();
            var ownerId = 1;

            // Act
            var result = await _excelService.ParseProductsFromExcelAsync(file, ownerId);

            // Assert
            result.Should().NotBeNull();
            result.FailedCount.Should().BeGreaterThan(0);
            result.FailedRows.Should().Contain(f => f.Reason.Contains("Name is required"));
        }

        private IFormFile CreateValidExcelFile()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            // Headers
            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "StartingPrice";
            worksheet.Cells[1, 3].Value = "Description";
            worksheet.Cells[1, 4].Value = "Category";
            worksheet.Cells[1, 5].Value = "DurationMinutes";

            // Data
            worksheet.Cells[2, 1].Value = "Product 1";
            worksheet.Cells[2, 2].Value = 100;
            worksheet.Cells[2, 3].Value = "Description 1";
            worksheet.Cells[2, 4].Value = "Electronics";
            worksheet.Cells[2, 5].Value = 60;

            var stream = new MemoryStream(package.GetAsByteArray());
            return new FormFile(stream, 0, stream.Length, "file", "test.xlsx");
        }

        private IFormFile CreateEmptyExcelFile()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");
            // Only headers, no data

            var stream = new MemoryStream(package.GetAsByteArray());
            return new FormFile(stream, 0, stream.Length, "file", "test.xlsx");
        }

        private IFormFile CreateExcelFileWithMissingColumns()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            // Only some headers
            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "StartingPrice";

            worksheet.Cells[2, 1].Value = "Product 1";
            worksheet.Cells[2, 2].Value = 100;

            var stream = new MemoryStream(package.GetAsByteArray());
            return new FormFile(stream, 0, stream.Length, "file", "test.xlsx");
        }

        private IFormFile CreateExcelFileWithInvalidPrice()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "StartingPrice";
            worksheet.Cells[1, 3].Value = "Description";
            worksheet.Cells[1, 4].Value = "Category";
            worksheet.Cells[1, 5].Value = "DurationMinutes";

            worksheet.Cells[2, 1].Value = "Product 1";
            worksheet.Cells[2, 2].Value = "invalid"; // Invalid price
            worksheet.Cells[2, 3].Value = "Description 1";
            worksheet.Cells[2, 4].Value = "Electronics";
            worksheet.Cells[2, 5].Value = 60;

            var stream = new MemoryStream(package.GetAsByteArray());
            return new FormFile(stream, 0, stream.Length, "file", "test.xlsx");
        }

        private IFormFile CreateExcelFileWithInvalidDuration()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "StartingPrice";
            worksheet.Cells[1, 3].Value = "Description";
            worksheet.Cells[1, 4].Value = "Category";
            worksheet.Cells[1, 5].Value = "DurationMinutes";

            worksheet.Cells[2, 1].Value = "Product 1";
            worksheet.Cells[2, 2].Value = 100;
            worksheet.Cells[2, 3].Value = "Description 1";
            worksheet.Cells[2, 4].Value = "Electronics";
            worksheet.Cells[2, 5].Value = 1; // Too short

            var stream = new MemoryStream(package.GetAsByteArray());
            return new FormFile(stream, 0, stream.Length, "file", "test.xlsx");
        }

        private IFormFile CreateExcelFileWithMissingName()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "StartingPrice";
            worksheet.Cells[1, 3].Value = "Description";
            worksheet.Cells[1, 4].Value = "Category";
            worksheet.Cells[1, 5].Value = "DurationMinutes";

            worksheet.Cells[2, 1].Value = ""; // Missing name
            worksheet.Cells[2, 2].Value = 100;
            worksheet.Cells[2, 3].Value = "Description 1";
            worksheet.Cells[2, 4].Value = "Electronics";
            worksheet.Cells[2, 5].Value = 60;

            var stream = new MemoryStream(package.GetAsByteArray());
            return new FormFile(stream, 0, stream.Length, "file", "test.xlsx");
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}

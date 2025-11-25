using BidSphere.Models.Domain;
using BidSphere.Models.Dtos.Products;
using BidSphere.Models.Enums;
using BidSphere.Repository.Interface;
using BidSphere.Service.Interface;
using OfficeOpenXml;

namespace BidSphere.Service.Implementation
{
    public class ExcelService : IExcelService
    {
        private readonly IProductRepository _productRepository;
        private readonly IAuctionRepository _auctionRepository;

        public ExcelService(IProductRepository productRepository, IAuctionRepository auctionRepository)
        {
            _productRepository = productRepository;
            _auctionRepository = auctionRepository;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<ExcelUploadResultDto> ParseProductsFromExcelAsync(IFormFile file, int ownerId)
        {
            var result = new ExcelUploadResultDto();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            if (rowCount < 2)
            {
                throw new Exception("Excel file is empty or has no data rows");
            }

            // Validate headers
            var headers = new Dictionary<string, int>();
            for (int col = 1; col <= worksheet.Dimension.Columns; col++)
            {
                var header = worksheet.Cells[1, col].Value?.ToString()?.Trim().ToLower();
                if (!string.IsNullOrEmpty(header))
                {
                    headers[header] = col;
                }
            }

            // Check required columns
            var requiredColumns = new[] { "name", "startingprice", "description", "category", "durationminutes" };
            foreach (var required in requiredColumns)
            {
                if (!headers.ContainsKey(required))
                {
                    throw new Exception($"Missing required column: {required}");
                }
            }

            // Process rows
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var name = worksheet.Cells[row, headers["name"]].Value?.ToString()?.Trim();
                    var startingPriceStr = worksheet.Cells[row, headers["startingprice"]].Value?.ToString();
                    var description = worksheet.Cells[row, headers["description"]].Value?.ToString()?.Trim();
                    var category = worksheet.Cells[row, headers["category"]].Value?.ToString()?.Trim();
                    var durationStr = worksheet.Cells[row, headers["durationminutes"]].Value?.ToString();

                    // Validate
                    if (string.IsNullOrEmpty(name))
                    {
                        result.FailedRows.Add(new FailedRowDto { RowNumber = row, Reason = "Name is required" });
                        continue;
                    }

                    if (!decimal.TryParse(startingPriceStr, out var startingPrice) || startingPrice <= 0)
                    {
                        result.FailedRows.Add(new FailedRowDto { RowNumber = row, Reason = "Invalid starting price" });
                        continue;
                    }

                    if (!int.TryParse(durationStr, out var duration) || duration < 2 || duration > 1440)
                    {
                        result.FailedRows.Add(new FailedRowDto { RowNumber = row, Reason = "Duration must be between 2 and 1440 minutes" });
                        continue;
                    }

                    if (string.IsNullOrEmpty(category))
                    {
                        result.FailedRows.Add(new FailedRowDto { RowNumber = row, Reason = "Category is required" });
                        continue;
                    }

                    // Create product
                    var product = new Product
                    {
                        Name = name,
                        Description = description ?? "",
                        Category = category,
                        StartingPrice = startingPrice,
                        AuctionDurationMinutes = duration,
                        OwnerId = ownerId,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createdProduct = await _productRepository.AddProduct(product);

                    // Create auction
                    var auction = new Auction
                    {
                        ProductId = createdProduct.ProductId,
                        StartTime = DateTime.UtcNow,
                        ExpiryTime = DateTime.UtcNow.AddMinutes(duration),
                        Status = AuctionStatus.Active,
                        ExtensionCount = 0
                    };

                    await _auctionRepository.CreateAsync(auction);

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailedRows.Add(new FailedRowDto { RowNumber = row, Reason = ex.Message });
                }
            }

            result.FailedCount = result.FailedRows.Count;
            return result;
        }
    }
}

using BidSphere.Models.Dtos.Products;

namespace BidSphere.Service.Interface
{
    public interface IExcelService
    {
        Task<ExcelUploadResultDto> ParseProductsFromExcelAsync(IFormFile file, int ownerId);
    }
}

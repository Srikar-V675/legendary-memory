using BidSphere.Models.Dtos.Products;

namespace BidSphere.Service.Interface
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? status, string? category, decimal? minPrice, decimal? maxPrice);
        Task<IEnumerable<ProductDto>> GetActiveAuctionsAsync();
        Task<AuctionDetailsDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto createDto, int ownerId);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateDto);
        Task DeleteProductAsync(int id);
        Task ForceFinalizeAuctionAsync(int id);
    }
}

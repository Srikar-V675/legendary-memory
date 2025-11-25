using BidSphere.Models.Domain;

namespace BidSphere.Repository.Interface
{
    public interface IProductRepository
    {
        Task<Product> AddProduct(Product product);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product> UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<bool> HasActiveBidsAsync(int productId);
    }
}

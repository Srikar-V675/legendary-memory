using BidSphere.Models.Domain;

namespace BidSphere.Repository.Interface
{
    public interface IProductRepository
    {
        /// <summary>
        /// Adds new product
        /// </summary>
        /// <param name="product"></param>
        /// <returns>Newly added product</returns>
        Task<Product> AddProduct(Product product);

        /// <summary>
        /// Gets all products
        /// </summary>
        /// <returns>List of products</returns>
        IEnumerable<Product> GetAllProducts();
    }
}

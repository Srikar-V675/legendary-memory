#region References
using BidSphere.Repository.Interface;
using BidSphere.Data;
using BidSphere.Models.Domain;
#endregion

namespace BidSphere.Repository.Implementation
{
    public class ProductRepository: IProductRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductRepository(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        ///<inheritdoc/>
        public IEnumerable<Product> GetAllProducts()
        {
            return _dbContext.Products;
        }

        ///<inheritdoc/>
        public async Task<Product> AddProduct(Product product)
        {
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            return product;
        }
    }
}
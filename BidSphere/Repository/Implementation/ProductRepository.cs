#region References
using BidSphere.Repository.Interface;
using BidSphere.Data;
using BidSphere.Models.Domain;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Product> AddProduct(Product product)
        {
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _dbContext.Products
                .Include(p => p.Auction)
                    .ThenInclude(a => a.HighestBid)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _dbContext.Products
                .Include(p => p.Auction)
                    .ThenInclude(a => a.Bids)
                        .ThenInclude(b => b.Bidder)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product != null)
            {
                _dbContext.Products.Remove(product);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> HasActiveBidsAsync(int productId)
        {
            return await _dbContext.Bids
                .AnyAsync(b => b.Auction.ProductId == productId);
        }
    }
}
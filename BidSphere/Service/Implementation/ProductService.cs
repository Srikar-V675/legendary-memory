using AutoMapper;
using BidSphere.Repository.Interface;
using BidSphere.Models.Domain;
using BidSphere.Models.Dtos.Products;
using BidSphere.Models.Enums;
using BidSphere.Service.Interface;

namespace BidSphere.Service.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;
        private readonly IAuctionRepository _auctionRepository;

        public ProductService(
            IMapper mapper,
            IProductRepository productRepository,
            IAuctionRepository auctionRepository)
        {
            _mapper = mapper;
            _productRepository = productRepository;
            _auctionRepository = auctionRepository;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? status, string? category, decimal? minPrice, decimal? maxPrice)
        {
            var products = await _productRepository.GetAllProductsAsync();
            // TODO: Implement ASQL filtering in Phase 5 (Milestone 3)
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetActiveAuctionsAsync()
        {
            var products = await _productRepository.GetAllProductsAsync();
            var activeProducts = products.Where(p => p.Auction != null && p.Auction.Status == AuctionStatus.Active);
            return _mapper.Map<IEnumerable<ProductDto>>(activeProducts);
        }

        public async Task<AuctionDetailsDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;

            return _mapper.Map<AuctionDetailsDto>(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto, int ownerId)
        {
            var product = _mapper.Map<Product>(createDto);
            product.OwnerId = ownerId;
            product.CreatedAt = DateTime.UtcNow;

            var createdProduct = await _productRepository.AddProduct(product);

            // Auto-create auction
            var auction = new Auction
            {
                ProductId = createdProduct.ProductId,
                StartTime = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMinutes(createDto.AuctionDurationMinutes),
                Status = AuctionStatus.Active,
                ExtensionCount = 0
            };

            await _auctionRepository.CreateAsync(auction);

            // Reload product with auction
            var productWithAuction = await _productRepository.GetByIdAsync(createdProduct.ProductId);
            return _mapper.Map<ProductDto>(productWithAuction);
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            // Check if product has active bids
            if (await _productRepository.HasActiveBidsAsync(id))
            {
                throw new Exception("Cannot update product with active bids");
            }

            product.Name = updateDto.Name;
            product.Description = updateDto.Description;
            product.Category = updateDto.Category;
            product.StartingPrice = updateDto.StartingPrice;

            var updatedProduct = await _productRepository.UpdateAsync(product);
            return _mapper.Map<ProductDto>(updatedProduct);
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            // Check if product has active bids
            if (await _productRepository.HasActiveBidsAsync(id))
            {
                throw new Exception("Cannot delete product with active bids");
            }

            await _productRepository.DeleteAsync(id);
        }

        public async Task ForceFinalizeAuctionAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            if (product.Auction == null)
            {
                throw new Exception("No auction found for this product");
            }

            if (product.Auction.Status == AuctionStatus.Completed || product.Auction.Status == AuctionStatus.Failed)
            {
                throw new Exception("Auction is already finalized");
            }

            // Force finalize - mark as expired
            product.Auction.Status = AuctionStatus.Expired;
            product.Auction.ExpiryTime = DateTime.UtcNow;
            await _auctionRepository.UpdateAsync(product.Auction);
        }
    }
}
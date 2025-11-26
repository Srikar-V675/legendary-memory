using AutoMapper;
using BidSphere.Repository.Interface;
using BidSphere.Models.Domain;
using BidSphere.Models.Dtos.Products;
using BidSphere.Models.Enums;
using BidSphere.Service.Interface;
using BidSphere.Exceptions;

namespace BidSphere.Service.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;
        private readonly IAuctionRepository _auctionRepository;
        private readonly IAsqlParserService _asqlParser;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBidRepository _bidRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductService(
            IMapper mapper,
            IProductRepository productRepository,
            IAuctionRepository auctionRepository,
            IAsqlParserService asqlParser,
            IPaymentRepository paymentRepository,
            IBidRepository bidRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _productRepository = productRepository;
            _auctionRepository = auctionRepository;
            _asqlParser = asqlParser;
            _paymentRepository = paymentRepository;
            _bidRepository = bidRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? asql)
        {
            var products = await _productRepository.GetAllProductsAsync();
            var query = products.AsQueryable();

            if (!string.IsNullOrEmpty(asql))
            {
                query = _asqlParser.ApplyAsqlFilter(query, asql);
            }

            return _mapper.Map<IEnumerable<ProductDto>>(query);
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
                throw new ProductNotFoundException(id);
            }

            // Check if product has active bids
            if (await _productRepository.HasActiveBidsAsync(id))
            {
                throw new InvalidBidException("Cannot update product with active bids");
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
                throw new ProductNotFoundException(id);
            }

            // Check if product has active bids
            if (await _productRepository.HasActiveBidsAsync(id))
            {
                throw new InvalidBidException("Cannot delete product with active bids");
            }

            await _productRepository.DeleteAsync(id);
        }

        public async Task ForceFinalizeAuctionAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new ProductNotFoundException(id);
            }

            if (product.Auction == null)
            {
                throw new AuctionNotFoundException($"No auction found for product {id}");
            }

            if (product.Auction.Status == AuctionStatus.Completed || product.Auction.Status == AuctionStatus.Failed)
            {
                throw new InvalidBidException("Auction is already finalized");
            }

            // Force finalize - mark as expired
            product.Auction.Status = AuctionStatus.Expired;
            product.Auction.ExpiryTime = DateTime.UtcNow;
            await _auctionRepository.UpdateAsync(product.Auction);
        }

        public async Task<object> ConfirmPaymentAsync(int productId, ConfirmPaymentDto dto, bool testInstantFail)
        {
            // Get current user ID
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            var userId = int.Parse(userIdClaim);

            // Get product and auction
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new ProductNotFoundException(productId);
            }

            if (product.Auction == null)
            {
                throw new AuctionNotFoundException($"No auction found for product {productId}");
            }

            var auction = product.Auction;

            // Verify auction is expired
            if (auction.Status != AuctionStatus.Expired)
            {
                throw new PaymentException("Auction is not in expired status. Payment can only be confirmed for expired auctions.");
            }

            // Get pending payment
            var pendingPayment = await _paymentRepository.GetPendingPaymentAsync(auction.AuctionId);
            if (pendingPayment == null)
            {
                // Check if user had a failed payment attempt
                var allPayments = await _paymentRepository.GetByAuctionIdAsync(auction.AuctionId);
                var userFailedPayment = allPayments.FirstOrDefault(p => p.BidderId == userId && p.Status == PaymentStatus.Failed);

                if (userFailedPayment != null)
                {
                    throw new PaymentException("Your payment attempt has already failed. The auction has moved to the next bidder.");
                }

                throw new PaymentException("No pending payment found for this auction. The auction may have been completed or cancelled.");
            }

            // Verify user is the current eligible bidder
            if (pendingPayment.BidderId != userId)
            {
                // Check if this user had a failed attempt
                var allPayments = await _paymentRepository.GetByAuctionIdAsync(auction.AuctionId);
                var userFailedPayment = allPayments.FirstOrDefault(p => p.BidderId == userId && p.Status == PaymentStatus.Failed);

                if (userFailedPayment != null)
                {
                    throw new PaymentException("Your payment attempt has already failed. The auction has moved to the next bidder.");
                }

                throw new UnauthorizedAccessException("You are not the current eligible bidder for this auction");
            }

            // Test instant fail mode
            if (testInstantFail)
            {
                pendingPayment.Status = PaymentStatus.Failed;
                await _paymentRepository.UpdateAsync(pendingPayment);
                return new { message = "Payment failed (test mode)", status = "Failed" };
            }

            // Get the current bidder's bid amount (not necessarily the highest bid if retries occurred)
            var allBids = await _bidRepository.GetByAuctionIdAsync(auction.AuctionId);
            var currentBidderBid = allBids.FirstOrDefault(b => b.BidderId == pendingPayment.BidderId);

            if (currentBidderBid == null)
            {
                throw new PaymentException("No bid found for current bidder");
            }

            // Verify amount matches the current bidder's bid amount
            if (dto.ConfirmedAmount != currentBidderBid.Amount)
            {
                throw new PaymentException($"Confirmed amount ({dto.ConfirmedAmount:C}) does not match your bid amount ({currentBidderBid.Amount:C})");
            }

            // Mark payment as success
            pendingPayment.Status = PaymentStatus.Success;
            pendingPayment.ConfirmedAmount = dto.ConfirmedAmount;
            pendingPayment.ConfirmedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(pendingPayment);

            // Mark auction as completed
            auction.Status = AuctionStatus.Completed;
            await _auctionRepository.UpdateAsync(auction);

            return new { message = "Payment confirmed successfully", auctionStatus = "Completed" };
        }
    }
}
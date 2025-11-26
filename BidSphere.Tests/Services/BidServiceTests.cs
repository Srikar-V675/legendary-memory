using AutoMapper;
using BidSphere.Exceptions;
using BidSphere.Models.Domain;
using BidSphere.Models.Dtos.Bids;
using BidSphere.Models.Enums;
using BidSphere.Repository.Interface;
using BidSphere.Service.Implementation;
using FluentAssertions;
using Moq;
using Xunit;

namespace BidSphere.Tests.Services
{
    public class BidServiceTests
    {
        private readonly Mock<IBidRepository> _bidRepositoryMock;
        private readonly Mock<IAuctionRepository> _auctionRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly BidService _bidService;

        public BidServiceTests()
        {
            _bidRepositoryMock = new Mock<IBidRepository>();
            _auctionRepositoryMock = new Mock<IAuctionRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _mapperMock = new Mock<IMapper>();

            _bidService = new BidService(
                _bidRepositoryMock.Object,
                _auctionRepositoryMock.Object,
                _productRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task PlaceBidAsync_WithValidBid_ShouldSucceed()
        {
            // Arrange
            var userId = 2;
            var bidDto = new PlaceBidDto { AuctionId = 1, Amount = 150 };

            var product = new Product
            {
                ProductId = 1,
                OwnerId = 1,
                StartingPrice = 100,
                Auction = new Auction
                {
                    AuctionId = 1,
                    Status = AuctionStatus.Active,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(10)
                }
            };

            var createdBid = new Bid
            {
                BidId = 1,
                AuctionId = 1,
                BidderId = userId,
                Amount = 150,
                Timestamp = DateTime.UtcNow
            };

            _productRepositoryMock.Setup(x => x.GetByIdAsync(bidDto.AuctionId))
                .ReturnsAsync(product);

            _bidRepositoryMock.Setup(x => x.GetHighestBidAsync(product.Auction.AuctionId))
                .ReturnsAsync((Bid?)null);

            _bidRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Bid>()))
                .ReturnsAsync(createdBid);

            _auctionRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Auction>()))
                .ReturnsAsync(product.Auction);

            _bidRepositoryMock.Setup(x => x.GetByAuctionIdAsync(product.Auction.AuctionId))
                .ReturnsAsync(new List<Bid> { createdBid });

            _mapperMock.Setup(x => x.Map<BidDto>(It.IsAny<Bid>()))
                .Returns(new BidDto { BidId = 1, Amount = 150 });

            // Act
            var result = await _bidService.PlaceBidAsync(userId, bidDto);

            // Assert
            result.Should().NotBeNull();
            result.Amount.Should().Be(150);
            _bidRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Bid>()), Times.Once);
        }

        [Fact]
        public async Task PlaceBidAsync_WithNonExistentProduct_ShouldThrowProductNotFoundException()
        {
            // Arrange
            var userId = 2;
            var bidDto = new PlaceBidDto { AuctionId = 999, Amount = 150 };

            _productRepositoryMock.Setup(x => x.GetByIdAsync(bidDto.AuctionId))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ProductNotFoundException>(
                () => _bidService.PlaceBidAsync(userId, bidDto)
            );
        }

        [Fact]
        public async Task PlaceBidAsync_OnOwnProduct_ShouldThrowUnauthorizedBidException()
        {
            // Arrange
            var userId = 1;
            var bidDto = new PlaceBidDto { AuctionId = 1, Amount = 150 };

            var product = new Product
            {
                ProductId = 1,
                OwnerId = 1, // Same as userId
                Auction = new Auction
                {
                    AuctionId = 1,
                    Status = AuctionStatus.Active
                }
            };

            _productRepositoryMock.Setup(x => x.GetByIdAsync(bidDto.AuctionId))
                .ReturnsAsync(product);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedBidException>(
                () => _bidService.PlaceBidAsync(userId, bidDto)
            );
        }

        [Fact]
        public async Task PlaceBidAsync_OnInactiveAuction_ShouldThrowInvalidBidException()
        {
            // Arrange
            var userId = 2;
            var bidDto = new PlaceBidDto { AuctionId = 1, Amount = 150 };

            var product = new Product
            {
                ProductId = 1,
                OwnerId = 1,
                Auction = new Auction
                {
                    AuctionId = 1,
                    Status = AuctionStatus.Expired
                }
            };

            _productRepositoryMock.Setup(x => x.GetByIdAsync(bidDto.AuctionId))
                .ReturnsAsync(product);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidBidException>(
                () => _bidService.PlaceBidAsync(userId, bidDto)
            );
        }

        [Fact]
        public async Task PlaceBidAsync_WithLowerAmount_ShouldThrowInvalidBidException()
        {
            // Arrange
            var userId = 2;
            var bidDto = new PlaceBidDto { AuctionId = 1, Amount = 50 };

            var product = new Product
            {
                ProductId = 1,
                OwnerId = 1,
                StartingPrice = 100,
                Auction = new Auction
                {
                    AuctionId = 1,
                    Status = AuctionStatus.Active
                }
            };

            var highestBid = new Bid { Amount = 150 };

            _productRepositoryMock.Setup(x => x.GetByIdAsync(bidDto.AuctionId))
                .ReturnsAsync(product);

            _bidRepositoryMock.Setup(x => x.GetHighestBidAsync(product.Auction.AuctionId))
                .ReturnsAsync(highestBid);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidBidException>(
                () => _bidService.PlaceBidAsync(userId, bidDto)
            );
        }

        [Fact]
        public async Task GetBidsByAuctionAsync_ShouldReturnBids()
        {
            // Arrange
            var auctionId = 1;
            var bids = new List<Bid>
            {
                new Bid { BidId = 1, Amount = 100 },
                new Bid { BidId = 2, Amount = 150 }
            };

            var bidDtos = new List<BidDto>
            {
                new BidDto { BidId = 1, Amount = 100 },
                new BidDto { BidId = 2, Amount = 150 }
            };

            _bidRepositoryMock.Setup(x => x.GetByAuctionIdAsync(auctionId))
                .ReturnsAsync(bids);

            _mapperMock.Setup(x => x.Map<IEnumerable<BidDto>>(bids))
                .Returns(bidDtos);

            // Act
            var result = await _bidService.GetBidsByAuctionAsync(auctionId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(bidDtos);
        }
    }
}

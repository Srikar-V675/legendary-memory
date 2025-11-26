using BidSphere.Controllers;
using BidSphere.Models.Domain;
using BidSphere.Models.Enums;
using BidSphere.Repository.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace BidSphere.Tests.Controllers
{
    public class TransactionsControllerTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<ILogger<TransactionsController>> _loggerMock;
        private readonly TransactionsController _controller;

        public TransactionsControllerTests()
        {
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _loggerMock = new Mock<ILogger<TransactionsController>>();
            _controller = new TransactionsController(_paymentRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetTransactions_AsUser_ShouldReturnOnlyOwnTransactions()
        {
            // Arrange
            var userId = 1;
            var userPayments = new List<PaymentAttempt>
            {
                new PaymentAttempt
                {
                    PaymentId = 1,
                    BidderId = userId,
                    AuctionId = 1,
                    Status = PaymentStatus.Success,
                    AttemptNumber = 1,
                    AttemptTime = DateTime.UtcNow,
                    Auction = new Auction
                    {
                        AuctionId = 1,
                        Product = new Product { Name = "Product 1" },
                        HighestBid = new Bid { Amount = 100 }
                    },
                    Bidder = new User { UserName = "user@example.com" }
                }
            };

            SetupUserContext(userId.ToString(), "User");

            _paymentRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(userPayments);

            // Act
            var result = await _controller.GetTransactions();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            _paymentRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetTransactions_AsAdmin_ShouldReturnAllTransactions()
        {
            // Arrange
            var allPayments = new List<PaymentAttempt>
            {
                new PaymentAttempt
                {
                    PaymentId = 1,
                    BidderId = 1,
                    AuctionId = 1,
                    Status = PaymentStatus.Success,
                    AttemptNumber = 1,
                    AttemptTime = DateTime.UtcNow,
                    Auction = new Auction
                    {
                        AuctionId = 1,
                        Product = new Product { Name = "Product 1" },
                        HighestBid = new Bid { Amount = 100 }
                    },
                    Bidder = new User { UserName = "user1@example.com" }
                },
                new PaymentAttempt
                {
                    PaymentId = 2,
                    BidderId = 2,
                    AuctionId = 2,
                    Status = PaymentStatus.Pending,
                    AttemptNumber = 1,
                    AttemptTime = DateTime.UtcNow,
                    Auction = new Auction
                    {
                        AuctionId = 2,
                        Product = new Product { Name = "Product 2" },
                        HighestBid = new Bid { Amount = 200 }
                    },
                    Bidder = new User { UserName = "user2@example.com" }
                }
            };

            SetupUserContext("1", "Admin");

            _paymentRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(allPayments);

            // Act
            var result = await _controller.GetTransactions();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            _paymentRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTransactions_WithNoTransactions_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = 1;
            SetupUserContext(userId.ToString(), "User");

            _paymentRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(new List<PaymentAttempt>());

            // Act
            var result = await _controller.GetTransactions();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var transactions = okResult!.Value as IEnumerable<object>;
            transactions.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTransactions_WithNullAuction_ShouldHandleGracefully()
        {
            // Arrange
            var userId = 1;
            var userPayments = new List<PaymentAttempt>
            {
                new PaymentAttempt
                {
                    PaymentId = 1,
                    BidderId = userId,
                    AuctionId = 1,
                    Status = PaymentStatus.Success,
                    AttemptNumber = 1,
                    AttemptTime = DateTime.UtcNow,
                    Auction = null, // Null auction
                    Bidder = new User { UserName = "user@example.com" }
                }
            };

            SetupUserContext(userId.ToString(), "User");

            _paymentRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(userPayments);

            // Act
            var result = await _controller.GetTransactions();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        private void SetupUserContext(string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
    }
}

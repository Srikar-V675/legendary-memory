using BidSphere.Models.Dtos.Bids;
using BidSphere.Validators;
using FluentAssertions;
using Xunit;

namespace BidSphere.Tests.Validators
{
    public class PlaceBidDtoValidatorTests
    {
        private readonly PlaceBidDtoValidator _validator;

        public PlaceBidDtoValidatorTests()
        {
            _validator = new PlaceBidDtoValidator();
        }

        [Fact]
        public async Task Validate_WithValidBid_ShouldPass()
        {
            // Arrange
            var dto = new PlaceBidDto
            {
                AuctionId = 1,
                Amount = 100
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Validate_WithZeroAmount_ShouldFail()
        {
            // Arrange
            var dto = new PlaceBidDto
            {
                AuctionId = 1,
                Amount = 0
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Amount");
        }

        [Fact]
        public async Task Validate_WithNegativeAmount_ShouldFail()
        {
            // Arrange
            var dto = new PlaceBidDto
            {
                AuctionId = 1,
                Amount = -50
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Validate_WithZeroAuctionId_ShouldFail()
        {
            // Arrange
            var dto = new PlaceBidDto
            {
                AuctionId = 0,
                Amount = 100
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "AuctionId");
        }
    }
}

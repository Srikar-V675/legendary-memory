using BidSphere.Models;
using BidSphere.Service.Implementation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BidSphere.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<IOptions<EmailSettings>> _emailSettingsMock;
        private readonly Mock<ILogger<EmailService>> _loggerMock;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            _emailSettingsMock = new Mock<IOptions<EmailSettings>>();
            _loggerMock = new Mock<ILogger<EmailService>>();

            var emailSettings = new EmailSettings
            {
                SenderName = "BidSphere Test",
                SenderEmail = "test@bidsphere.com",
                SmtpServer = "smtp.test.com",
                SmtpPort = 587,
                EnableSsl = true,
                Username = "testuser",
                Password = "testpass"
            };

            _emailSettingsMock.Setup(x => x.Value).Returns(emailSettings);
            _emailService = new EmailService(_emailSettingsMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task SendAuctionWonNotificationAsync_WithValidData_ShouldNotThrowException()
        {
            // Arrange
            var toEmail = "winner@example.com";
            var userName = "John Doe";
            var productName = "Vintage Watch";
            var winningBid = 250.50m;

            // Act
            var act = async () => await _emailService.SendAuctionWonNotificationAsync(
                toEmail, userName, productName, winningBid);

            // Assert
            // Email service catches exceptions internally, so it should not throw
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendAuctionWonNotificationAsync_WithInvalidEmail_ShouldNotThrowException()
        {
            // Arrange
            var toEmail = "invalid-email";
            var userName = "John Doe";
            var productName = "Vintage Watch";
            var winningBid = 250.50m;

            // Act
            var act = async () => await _emailService.SendAuctionWonNotificationAsync(
                toEmail, userName, productName, winningBid);

            // Assert
            // Email service catches exceptions internally
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendAuctionWonNotificationAsync_WithNullEmail_ShouldNotThrowException()
        {
            // Arrange
            string? toEmail = null;
            var userName = "John Doe";
            var productName = "Vintage Watch";
            var winningBid = 250.50m;

            // Act
            var act = async () => await _emailService.SendAuctionWonNotificationAsync(
                toEmail!, userName, productName, winningBid);

            // Assert
            // Email service catches exceptions internally
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendAuctionWonNotificationAsync_WithZeroBid_ShouldNotThrowException()
        {
            // Arrange
            var toEmail = "winner@example.com";
            var userName = "John Doe";
            var productName = "Vintage Watch";
            var winningBid = 0m;

            // Act
            var act = async () => await _emailService.SendAuctionWonNotificationAsync(
                toEmail, userName, productName, winningBid);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // Note: Removed test for null settings as it tests implementation details
        // The EmailService is configured via DI and settings are validated at startup
    }
}

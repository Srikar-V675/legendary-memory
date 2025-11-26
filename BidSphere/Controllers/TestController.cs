using BidSphere.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidSphere.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<TestController> _logger;

        public TestController(IEmailService emailService, ILogger<TestController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Test email sending functionality
        /// </summary>
        /// <param name="toEmail">Email address to send test email to</param>
        [HttpPost("send-test-email")]
        public async Task<IActionResult> SendTestEmail([FromQuery] string toEmail = "test@example.com")
        {
            try
            {
                _logger.LogInformation("Sending test email to {Email}", toEmail);

                await _emailService.SendAuctionWonNotificationAsync(
                    toEmail: toEmail,
                    userName: toEmail, // Using email as name
                    productName: "Test Product - Vintage Watch",
                    winningBid: 250.50m
                );

                return Ok(new
                {
                    success = true,
                    message = $"Test email sent successfully to {toEmail}",
                    note = "Check your Mailtrap inbox to see the email"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email to {Email}", toEmail);
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to send test email",
                    error = ex.Message,
                    note = "Check your EmailSettings in appsettings.json and ensure Mailtrap credentials are correct"
                });
            }
        }

        /// <summary>
        /// Test email with custom data
        /// </summary>
        [HttpPost("send-custom-email")]
        public async Task<IActionResult> SendCustomEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                _logger.LogInformation("Sending custom test email to {Email}", request.ToEmail);

                await _emailService.SendAuctionWonNotificationAsync(
                    toEmail: request.ToEmail,
                    userName: request.UserName ?? request.ToEmail,
                    productName: request.ProductName ?? "Test Product",
                    winningBid: request.WinningBid ?? 100.00m
                );

                return Ok(new
                {
                    success = true,
                    message = $"Custom test email sent successfully to {request.ToEmail}",
                    details = new
                    {
                        to = request.ToEmail,
                        userName = request.UserName ?? request.ToEmail,
                        product = request.ProductName ?? "Test Product",
                        amount = request.WinningBid ?? 100.00m
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send custom test email to {Email}", request.ToEmail);
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to send custom test email",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get current email configuration (without sensitive data)
        /// </summary>
        [HttpGet("email-config")]
        public IActionResult GetEmailConfig([FromServices] Microsoft.Extensions.Options.IOptions<BidSphere.Models.EmailSettings> emailSettings)
        {
            var settings = emailSettings.Value;

            return Ok(new
            {
                smtpServer = settings.SmtpServer,
                smtpPort = settings.SmtpPort,
                senderEmail = settings.SenderEmail,
                senderName = settings.SenderName,
                enableSsl = settings.EnableSsl,
                username = settings.Username?.Length > 0 ? $"{settings.Username.Substring(0, Math.Min(4, settings.Username.Length))}***" : "Not set",
                passwordSet = !string.IsNullOrEmpty(settings.Password),
                note = "Password and full username are hidden for security"
            });
        }
    }

    public class TestEmailRequest
    {
        public string ToEmail { get; set; } = "test@example.com";
        public string? UserName { get; set; }
        public string? ProductName { get; set; }
        public decimal? WinningBid { get; set; }
    }
}

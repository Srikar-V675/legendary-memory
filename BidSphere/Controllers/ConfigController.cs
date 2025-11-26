using BidSphere.Constants;
using BidSphere.Models.Dtos.Config;
using BidSphere.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidSphere.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ConfigController : ControllerBase
    {
        private readonly AuctionConfigDtoValidator _validator;
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(AuctionConfigDtoValidator validator, ILogger<ConfigController> logger)
        {
            _validator = validator;
            _logger = logger;
        }

        /// <summary>
        /// Get current auction configuration
        /// </summary>
        [HttpGet]
        public IActionResult GetConfig()
        {
            var config = new AuctionConfigDto
            {
                AntiSnipingThresholdSeconds = AuctionConfig.AntiSnipingThresholdSeconds,
                ExtensionDurationSeconds = AuctionConfig.ExtensionDurationSeconds,
                PaymentTimeoutSeconds = AuctionConfig.PaymentTimeoutSeconds,
                MaxPaymentAttempts = AuctionConfig.MaxPaymentAttempts
            };

            return Ok(new
            {
                config,
                message = "Current auction configuration. Changes apply immediately but reset on app restart.",
                lastUpdated = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Update auction configuration at runtime (Admin only)
        /// </summary>
        /// <remarks>
        /// Changes apply immediately to all background services and new auctions.
        /// Note: Configuration resets to appsettings.json values on app restart.
        /// </remarks>
        [HttpPut]
        public async Task<IActionResult> UpdateConfig([FromBody] AuctionConfigDto configDto)
        {
            // Validate
            var validationResult = await _validator.ValidateAsync(configDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            // Store old values for logging
            var oldConfig = new
            {
                AntiSnipingThresholdSeconds = AuctionConfig.AntiSnipingThresholdSeconds,
                ExtensionDurationSeconds = AuctionConfig.ExtensionDurationSeconds,
                PaymentTimeoutSeconds = AuctionConfig.PaymentTimeoutSeconds,
                MaxPaymentAttempts = AuctionConfig.MaxPaymentAttempts
            };

            // Update configuration
            AuctionConfig.AntiSnipingThresholdSeconds = configDto.AntiSnipingThresholdSeconds;
            AuctionConfig.ExtensionDurationSeconds = configDto.ExtensionDurationSeconds;
            AuctionConfig.PaymentTimeoutSeconds = configDto.PaymentTimeoutSeconds;
            AuctionConfig.MaxPaymentAttempts = configDto.MaxPaymentAttempts;

            _logger.LogInformation(
                "Auction configuration updated. Old: {@OldConfig}, New: {@NewConfig}",
                oldConfig,
                configDto
            );

            return Ok(new
            {
                message = "Configuration updated successfully. Changes applied immediately.",
                oldConfig,
                newConfig = configDto,
                warning = "Configuration will reset to appsettings.json values on app restart.",
                updatedAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Reset configuration to default values from appsettings.json
        /// </summary>
        [HttpPost("reset")]
        public IActionResult ResetConfig([FromServices] IConfiguration configuration)
        {
            var auctionSettings = configuration.GetSection("AuctionSettings");

            var oldConfig = new
            {
                AntiSnipingThresholdSeconds = AuctionConfig.AntiSnipingThresholdSeconds,
                ExtensionDurationSeconds = AuctionConfig.ExtensionDurationSeconds,
                PaymentTimeoutSeconds = AuctionConfig.PaymentTimeoutSeconds,
                MaxPaymentAttempts = AuctionConfig.MaxPaymentAttempts
            };

            // Reset to defaults from appsettings
            AuctionConfig.AntiSnipingThresholdSeconds = auctionSettings.GetValue<int>("AntiSnipingThresholdSeconds", 60);
            AuctionConfig.ExtensionDurationSeconds = auctionSettings.GetValue<int>("ExtensionDurationSeconds", 60);
            AuctionConfig.PaymentTimeoutSeconds = auctionSettings.GetValue<int>("PaymentTimeoutSeconds", 60);
            AuctionConfig.MaxPaymentAttempts = auctionSettings.GetValue<int>("MaxPaymentAttempts", 3);

            var newConfig = new
            {
                AntiSnipingThresholdSeconds = AuctionConfig.AntiSnipingThresholdSeconds,
                ExtensionDurationSeconds = AuctionConfig.ExtensionDurationSeconds,
                PaymentTimeoutSeconds = AuctionConfig.PaymentTimeoutSeconds,
                MaxPaymentAttempts = AuctionConfig.MaxPaymentAttempts
            };

            _logger.LogInformation(
                "Auction configuration reset to defaults. Old: {@OldConfig}, New: {@NewConfig}",
                oldConfig,
                newConfig
            );

            return Ok(new
            {
                message = "Configuration reset to default values from appsettings.json",
                oldConfig,
                newConfig,
                resetAt = DateTime.UtcNow
            });
        }
    }
}

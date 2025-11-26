using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using BidSphere.Models;
using BidSphere.Service.Interface;

namespace BidSphere.Service.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendAuctionWonNotificationAsync(string toEmail, string userName, string productName, decimal winningBid)
        {
            try
            {
                var subject = "Congratulations! You Won the Auction - BidSphere";
                var body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                            .container {{ max-width: 600px; margin: 20px auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                            .header {{ background-color: #2196F3; color: white; padding: 30px; text-align: center; }}
                            .header h1 {{ margin: 0; font-size: 28px; }}
                            .content {{ padding: 30px; }}
                            .content h2 {{ color: #333; margin-top: 0; }}
                            .content p {{ color: #666; line-height: 1.6; }}
                            .highlight {{ background-color: #e3f2fd; padding: 15px; border-left: 4px solid #2196F3; margin: 20px 0; }}
                            .footer {{ background-color: #f9f9f9; padding: 20px; text-align: center; color: #888; font-size: 12px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🎯 BidSphere</h1>
                            </div>
                            <div class='content'>
                                <h2>🎉 Congratulations! You Won!</h2>
                                <p>Hi <strong>{userName}</strong>,</p>
                                <p>Great news! You won the auction!</p>
                                <div class='highlight'>
                                    <p><strong>Product:</strong> {productName}</p>
                                    <p><strong>Your Winning Bid:</strong> ${winningBid:F2}</p>
                                </div>
                                <p>Please proceed with payment to complete your purchase.</p>
                                <p>Thank you for using BidSphere! 🎊</p>
                            </div>
                            <div class='footer'>
                                <p>&copy; 2024 BidSphere. All rights reserved.</p>
                                <p>This is an automated message, please do not reply.</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.EnableSsl);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Auction won email sent successfully to {toEmail} - Product: {productName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send auction won email to {toEmail}: {ex.Message}");
                // Don't throw - we don't want email failures to break the application
            }
        }
    }
}

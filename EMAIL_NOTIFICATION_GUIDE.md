# Email Notification Implementation Guide

## 🎯 Overview

This guide explains how to implement email notifications in BidSphere using **MailKit** (recommended) or **SMTP**.

---

## 📧 Email Service Options

### Option 1: MailKit (Recommended)

- Modern, cross-platform library
- Better security and performance
- Supports OAuth2, SSL/TLS
- Active maintenance

### Option 2: System.Net.Mail.SmtpClient

- Built into .NET
- Simpler but deprecated
- Less secure
- Not recommended for production

**We'll use MailKit for this project.**

---

## 📦 Required NuGet Package

```bash
dotnet add package MailKit
dotnet add package MimeKit
```

---

## ⚙️ Configuration (appsettings.json)

Add email settings to `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@bidsphere.com",
    "SenderName": "BidSphere",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true
  }
}
```

### For Gmail:

- **SMTP Server:** smtp.gmail.com
- **Port:** 587 (TLS) or 465 (SSL)
- **Password:** Use App Password (not your regular password)
- **Enable 2FA** and generate App Password at: https://myaccount.google.com/apppasswords

### For Outlook/Hotmail:

- **SMTP Server:** smtp-mail.outlook.com
- **Port:** 587

### For SendGrid (Production):

- **SMTP Server:** smtp.sendgrid.net
- **Port:** 587
- **Username:** apikey
- **Password:** Your SendGrid API key

### For Mailtrap (Testing):

- **SMTP Server:** smtp.mailtrap.io
- **Port:** 2525
- **Username/Password:** From Mailtrap dashboard
- **Great for testing without sending real emails**

---

## 🏗️ Implementation Structure

### Files to Create:

```
BidSphere/
├── Models/
│   └── EmailSettings.cs                    # Configuration model
├── Service/
│   ├── Interface/
│   │   └── IEmailService.cs                # Email service interface
│   └── Implementation/
│       └── EmailService.cs                 # Email service implementation
└── appsettings.json                        # Email configuration
```

---

## 📝 Step-by-Step Implementation

### Step 1: Create EmailSettings Model

**File:** `Models/EmailSettings.cs`

```csharp
namespace BidSphere.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
    }
}
```

### Step 2: Create IEmailService Interface

**File:** `Service/Interface/IEmailService.cs`

```csharp
namespace BidSphere.Service.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendBidPlacedNotificationAsync(string toEmail, string userName, string productName, decimal bidAmount);
        Task SendOutbidNotificationAsync(string toEmail, string userName, string productName, decimal newBidAmount);
        Task SendAuctionWonNotificationAsync(string toEmail, string userName, string productName, decimal winningBid);
        Task SendAuctionEndedNotificationAsync(string toEmail, string ownerName, string productName, string winnerName, decimal finalPrice);
    }
}
```

### Step 3: Create EmailService Implementation

**File:** `Service/Implementation/EmailService.cs`

```csharp
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

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
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

                _logger.LogInformation($"Email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {toEmail}: {ex.Message}");
                throw;
            }
        }

        public async Task SendBidPlacedNotificationAsync(string toEmail, string userName, string productName, decimal bidAmount)
        {
            var subject = "Bid Placed Successfully";
            var body = $@"
                <h2>Bid Placed Successfully</h2>
                <p>Hi {userName},</p>
                <p>Your bid of <strong>${bidAmount}</strong> has been placed on <strong>{productName}</strong>.</p>
                <p>Good luck!</p>
                <p>- BidSphere Team</p>
            ";
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendOutbidNotificationAsync(string toEmail, string userName, string productName, decimal newBidAmount)
        {
            var subject = "You've Been Outbid!";
            var body = $@"
                <h2>You've Been Outbid</h2>
                <p>Hi {userName},</p>
                <p>Someone placed a higher bid of <strong>${newBidAmount}</strong> on <strong>{productName}</strong>.</p>
                <p>Place a new bid to stay in the game!</p>
                <p>- BidSphere Team</p>
            ";
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendAuctionWonNotificationAsync(string toEmail, string userName, string productName, decimal winningBid)
        {
            var subject = "Congratulations! You Won the Auction";
            var body = $@"
                <h2>Congratulations!</h2>
                <p>Hi {userName},</p>
                <p>You won the auction for <strong>{productName}</strong> with a bid of <strong>${winningBid}</strong>!</p>
                <p>Please proceed with payment to complete your purchase.</p>
                <p>- BidSphere Team</p>
            ";
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendAuctionEndedNotificationAsync(string toEmail, string ownerName, string productName, string winnerName, decimal finalPrice)
        {
            var subject = "Your Auction Has Ended";
            var body = $@"
                <h2>Auction Ended</h2>
                <p>Hi {ownerName},</p>
                <p>Your auction for <strong>{productName}</strong> has ended.</p>
                <p>Winner: <strong>{winnerName}</strong></p>
                <p>Final Price: <strong>${finalPrice}</strong></p>
                <p>- BidSphere Team</p>
            ";
            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
```

### Step 4: Register Services in Program.cs

```csharp
// Add EmailSettings configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register EmailService
builder.Services.AddScoped<IEmailService, EmailService>();
```

---

## 🔔 When to Send Notifications

### 1. Bid Placed (BidService)

**Trigger:** User places a bid
**Recipients:**

- Bidder (confirmation)
- Previous highest bidder (outbid notification)

```csharp
// In BidService.PlaceBidAsync()
await _emailService.SendBidPlacedNotificationAsync(
    bidder.Email,
    bidder.UserName,
    product.Name,
    bidAmount
);

// Notify previous highest bidder
if (previousHighestBid != null)
{
    await _emailService.SendOutbidNotificationAsync(
        previousBidder.Email,
        previousBidder.UserName,
        product.Name,
        bidAmount
    );
}
```

### 2. Auction Won (AuctionExpiryMonitor)

**Trigger:** Auction ends with bids
**Recipients:**

- Winner
- Product owner

```csharp
// In AuctionExpiryMonitor.ProcessExpiredAuctionsAsync()
await _emailService.SendAuctionWonNotificationAsync(
    winner.Email,
    winner.UserName,
    product.Name,
    highestBid.BidAmount
);

await _emailService.SendAuctionEndedNotificationAsync(
    owner.Email,
    owner.UserName,
    product.Name,
    winner.UserName,
    highestBid.BidAmount
);
```

### 3. Auction Ended Without Bids (AuctionExpiryMonitor)

**Trigger:** Auction ends with no bids
**Recipients:** Product owner

```csharp
await _emailService.SendEmailAsync(
    owner.Email,
    "Auction Ended - No Bids",
    $"Your auction for {product.Name} ended without any bids."
);
```

---

## 🧪 Testing Email Service

### Option 1: Mailtrap (Recommended for Testing)

1. Sign up at https://mailtrap.io (free)
2. Get SMTP credentials from inbox
3. Update appsettings.json:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.mailtrap.io",
    "SmtpPort": 2525,
    "Username": "your-mailtrap-username",
    "Password": "your-mailtrap-password",
    "SenderEmail": "test@bidsphere.com",
    "SenderName": "BidSphere",
    "EnableSsl": true
  }
}
```

4. All emails will be caught by Mailtrap (no real emails sent)

### Option 2: Gmail (For Real Testing)

1. Enable 2FA on your Google account
2. Generate App Password: https://myaccount.google.com/apppasswords
3. Update appsettings.json:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-16-char-app-password",
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "BidSphere",
    "EnableSsl": true
  }
}
```

### Option 3: Mock Service (Unit Testing)

Create a mock email service that logs instead of sending:

```csharp
public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string body)
    {
        _logger.LogInformation($"[MOCK EMAIL] To: {toEmail}, Subject: {subject}");
        return Task.CompletedTask;
    }

    // Implement other methods similarly...
}
```

Register in Program.cs:

```csharp
// For development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailService, MockEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, EmailService>();
}
```

---

## 🔒 Security Best Practices

### 1. Never Commit Credentials

Add to `.gitignore`:

```
appsettings.json
appsettings.Development.json
appsettings.Production.json
```

### 2. Use Environment Variables (Production)

```csharp
// In Program.cs
builder.Configuration.AddEnvironmentVariables();
```

Set environment variables:

```bash
export EmailSettings__SmtpServer="smtp.gmail.com"
export EmailSettings__Username="your-email@gmail.com"
export EmailSettings__Password="your-app-password"
```

### 3. Use User Secrets (Development)

```bash
dotnet user-secrets init
dotnet user-secrets set "EmailSettings:Username" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "your-app-password"
```

### 4. Use Azure Key Vault (Production)

Store sensitive data in Azure Key Vault and reference in configuration.

---

## 📊 Email Templates

You can create reusable HTML templates:

**File:** `Templates/EmailTemplates.cs`

```csharp
public static class EmailTemplates
{
    public static string BidPlaced(string userName, string productName, decimal bidAmount)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .footer {{ text-align: center; padding: 10px; color: #888; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>BidSphere</h1>
                    </div>
                    <div class='content'>
                        <h2>Bid Placed Successfully</h2>
                        <p>Hi {userName},</p>
                        <p>Your bid of <strong>${bidAmount}</strong> has been placed on <strong>{productName}</strong>.</p>
                        <p>Good luck!</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2024 BidSphere. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
        ";
    }
}
```

---

## 🚨 Error Handling

### Retry Logic for Failed Emails

```csharp
public async Task SendEmailWithRetryAsync(string toEmail, string subject, string body, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            await SendEmailAsync(toEmail, subject, body);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Email send attempt {i + 1} failed: {ex.Message}");
            if (i == maxRetries - 1)
            {
                _logger.LogError($"Failed to send email after {maxRetries} attempts");
                throw;
            }
            await Task.Delay(1000 * (i + 1)); // Exponential backoff
        }
    }
}
```

### Queue Failed Emails

Store failed emails in database for later retry:

```csharp
public class FailedEmail
{
    public int Id { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
    public int RetryCount { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
```

---

## 📋 Summary

### Required Packages:

- MailKit
- MimeKit

### Configuration Needed:

- SMTP server details
- Username/password (App Password for Gmail)
- Sender email and name

### Files to Create:

1. `Models/EmailSettings.cs`
2. `Service/Interface/IEmailService.cs`
3. `Service/Implementation/EmailService.cs`
4. Update `appsettings.json`
5. Update `Program.cs`

### Integration Points:

- BidService (bid placed, outbid notifications)
- AuctionExpiryMonitor (auction won, auction ended)

### Testing Options:

- **Mailtrap** (best for testing, no real emails)
- **Gmail** (real emails, needs App Password)
- **Mock Service** (logs only, for unit tests)

---

## 🎯 Next Steps

1. Install MailKit and MimeKit packages
2. Create EmailSettings model
3. Create IEmailService and EmailService
4. Configure appsettings.json with SMTP settings
5. Register service in Program.cs
6. Integrate into BidService and AuctionExpiryMonitor
7. Test with Mailtrap or Gmail

Ready to implement when you're on your other machine!

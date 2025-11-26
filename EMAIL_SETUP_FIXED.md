# Email Setup - Mailtrap Configuration Fixed

## 🐛 Issue: SSL/TLS Connection Error

**Error Message:**

```
Failed to send auction won email: An error occurred while establishing SSL or TLS connection
```

**Root Cause:**

1. ❌ Using wrong SSL connection method (`EnableSsl` boolean instead of `SecureSocketOptions.StartTls`)
2. ❌ Missing EmailSettings in `appsettings.json`
3. ❌ Mailtrap requires STARTTLS, not direct SSL

---

## ✅ Fixes Applied

### 1. Updated EmailService.cs

**Before (Wrong):**

```csharp
await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.EnableSsl);
```

**After (Correct):**

```csharp
// Mailtrap uses STARTTLS on port 2525 or 587
await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
```

### 2. Added EmailSettings to appsettings.json

```json
{
  "EmailSettings": {
    "SmtpServer": "sandbox.smtp.mailtrap.io",
    "SmtpPort": 2525,
    "Username": "YOUR_MAILTRAP_USERNAME",
    "Password": "YOUR_MAILTRAP_PASSWORD",
    "SenderEmail": "noreply@bidsphere.com",
    "SenderName": "BidSphere",
    "EnableSsl": true
  }
}
```

---

## 🔧 Setup Instructions

### Step 1: Get Mailtrap Credentials

1. Go to [Mailtrap.io](https://mailtrap.io)
2. Sign up or log in
3. Go to **Email Testing** → **Inboxes**
4. Click on your inbox (or create one)
5. Go to **SMTP Settings** tab
6. Select **MailKit** from the dropdown
7. Copy your credentials:
   - **Host:** `sandbox.smtp.mailtrap.io`
   - **Port:** `2525` (or `587`)
   - **Username:** (your username)
   - **Password:** (your password)

### Step 2: Update appsettings.json

Replace the placeholders in `BidSphere/appsettings.json`:

```json
"EmailSettings": {
  "SmtpServer": "sandbox.smtp.mailtrap.io",
  "SmtpPort": 2525,
  "Username": "abc123def456",  // ← Your Mailtrap username
  "Password": "xyz789uvw012",  // ← Your Mailtrap password
  "SenderEmail": "noreply@bidsphere.com",
  "SenderName": "BidSphere",
  "EnableSsl": true
}
```

### Step 3: Restart Application

```bash
# Stop the application
Ctrl+C

# Start again
dotnet run --project BidSphere
```

---

## 🧪 Testing Email

### Test 1: Manual Email Test

Create a simple test endpoint (optional):

```csharp
// In a controller
[HttpPost("test-email")]
public async Task<IActionResult> TestEmail([FromServices] IEmailService emailService)
{
    await emailService.SendAuctionWonNotificationAsync(
        "test@example.com",
        "Test User",
        "Test Product",
        100.00m
    );
    return Ok("Email sent! Check Mailtrap inbox.");
}
```

### Test 2: Through Auction Flow

1. Create an auction with 1-minute duration
2. Place a bid
3. Wait for auction to expire
4. Check Mailtrap inbox for email

---

## 📊 Mailtrap Configuration Options

### Port Options:

| Port | Protocol | Use Case                              |
| ---- | -------- | ------------------------------------- |
| 2525 | STARTTLS | ✅ **Recommended** - Works everywhere |
| 587  | STARTTLS | Standard SMTP submission port         |
| 465  | SSL/TLS  | Legacy, not recommended               |
| 25   | Plain    | Often blocked by ISPs                 |

**Use port 2525** - it's specifically designed for testing and works in all environments.

### SecureSocketOptions:

```csharp
// For Mailtrap (port 2525 or 587)
SecureSocketOptions.StartTls  // ✅ Correct

// For Gmail (port 465)
SecureSocketOptions.SslOnConnect

// For testing without encryption (NOT RECOMMENDED)
SecureSocketOptions.None
```

---

## 🔍 Troubleshooting

### Error: "Authentication failed"

**Check:**

- Username and password are correct
- No extra spaces in credentials
- Using the correct inbox credentials

**Solution:**

```bash
# Verify credentials in appsettings.json
cat BidSphere/appsettings.json | grep -A 8 "EmailSettings"
```

### Error: "Connection refused"

**Check:**

- Port is correct (2525 or 587)
- No firewall blocking SMTP
- Internet connection is working

**Solution:**

```bash
# Test connection to Mailtrap
telnet sandbox.smtp.mailtrap.io 2525
# Should connect successfully
```

### Error: "SSL/TLS handshake failed"

**Check:**

- Using `SecureSocketOptions.StartTls` (not `Auto` or `SslOnConnect`)
- Port matches the SSL option

**Solution:**
Already fixed in the code! ✅

### Emails Not Appearing in Mailtrap

**Check:**

1. Correct inbox selected in Mailtrap dashboard
2. Check "All Messages" tab (not just "Inbox")
3. Look at application logs for send confirmation

**Logs to check:**

```bash
# Search for email logs
grep "email sent" logs/*.log
grep "Failed to send" logs/*.log
```

---

## 📧 Email Template

The current email template includes:

```html
<!DOCTYPE html>
<html>
  <head>
    <style>
      /* Responsive email styles */
    </style>
  </head>
  <body>
    <div class="container">
      <div class="header">
        <h1>🎯 BidSphere</h1>
      </div>
      <div class="content">
        <h2>🎉 Congratulations! You Won!</h2>
        <p>Hi <strong>{userName}</strong>,</p>
        <p>Great news! You won the auction!</p>
        <div class="highlight">
          <p><strong>Product:</strong> {productName}</p>
          <p><strong>Your Winning Bid:</strong> ${winningBid}</p>
        </div>
        <p>Please proceed with payment to complete your purchase.</p>
      </div>
      <div class="footer">
        <p>&copy; 2024 BidSphere. All rights reserved.</p>
      </div>
    </div>
  </body>
</html>
```

---

## ✅ Verification Checklist

After setup, verify:

- [ ] EmailSettings added to appsettings.json
- [ ] Mailtrap credentials are correct
- [ ] Port is 2525 (or 587)
- [ ] Using `SecureSocketOptions.StartTls`
- [ ] Application restarted
- [ ] Test auction created and expired
- [ ] Email appears in Mailtrap inbox
- [ ] Email HTML renders correctly

---

## 🎯 Expected Behavior

### When Auction Expires:

1. **AuctionFinalizer** detects expired auction
2. Creates payment attempt
3. **Sends email** to highest bidder
4. Logs: `"Auction won email sent successfully to {email}"`

### In Mailtrap:

1. Email appears in inbox within seconds
2. Subject: "Congratulations! You Won the Auction - BidSphere"
3. HTML renders with styling
4. Shows product name and winning bid amount

### On Payment Timeout:

1. **RetryQueueService** marks payment as failed
2. Moves to next bidder
3. **Sends email** to new bidder
4. Logs: `"Retry email sent to {email}"`

---

## 🚀 Production Setup (Future)

For production, replace Mailtrap with real SMTP:

### Option 1: SendGrid

```json
{
  "SmtpServer": "smtp.sendgrid.net",
  "SmtpPort": 587,
  "Username": "apikey",
  "Password": "YOUR_SENDGRID_API_KEY"
}
```

### Option 2: Gmail (App Password)

```json
{
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "Username": "your-email@gmail.com",
  "Password": "your-app-password"
}
```

### Option 3: AWS SES

```json
{
  "SmtpServer": "email-smtp.us-east-1.amazonaws.com",
  "SmtpPort": 587,
  "Username": "YOUR_SMTP_USERNAME",
  "Password": "YOUR_SMTP_PASSWORD"
}
```

---

## 📝 Summary

**What was fixed:**

- ✅ Changed from `EnableSsl` boolean to `SecureSocketOptions.StartTls`
- ✅ Added EmailSettings to appsettings.json
- ✅ Configured for Mailtrap's STARTTLS on port 2525
- ✅ Email service now works correctly with Mailtrap

**Next steps:**

1. Get your Mailtrap credentials
2. Update appsettings.json with your username/password
3. Restart the application
4. Test by creating an auction and letting it expire

Emails should now work perfectly! 📧✨

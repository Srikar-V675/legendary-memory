# Email Testing Endpoints

## 🧪 Quick Email Testing

I've created test endpoints so you can test email functionality without running the full auction flow.

---

## 📍 Available Endpoints

### 1. Simple Test Email (GET/POST)

**Endpoint:** `POST /api/test/send-test-email`

**Description:** Sends a test email with default values

**Usage:**

```bash
# Simple test with default email
curl -X POST http://localhost:8080/api/test/send-test-email

# Test with your email
curl -X POST "http://localhost:8080/api/test/send-test-email?toEmail=your-email@example.com"
```

**Response:**

```json
{
  "success": true,
  "message": "Test email sent successfully to test@example.com",
  "note": "Check your Mailtrap inbox to see the email"
}
```

---

### 2. Custom Test Email (POST with Body)

**Endpoint:** `POST /api/test/send-custom-email`

**Description:** Sends a test email with custom data

**Request Body:**

```json
{
  "toEmail": "winner@example.com",
  "userName": "John Doe",
  "productName": "Vintage Rolex Watch",
  "winningBid": 1500.0
}
```

**Usage:**

```bash
curl -X POST http://localhost:8080/api/test/send-custom-email \
  -H "Content-Type: application/json" \
  -d '{
    "toEmail": "winner@example.com",
    "userName": "John Doe",
    "productName": "Vintage Rolex Watch",
    "winningBid": 1500.00
  }'
```

**Response:**

```json
{
  "success": true,
  "message": "Custom test email sent successfully to winner@example.com",
  "details": {
    "to": "winner@example.com",
    "userName": "John Doe",
    "product": "Vintage Rolex Watch",
    "amount": 1500.0
  }
}
```

---

### 3. Check Email Configuration (GET)

**Endpoint:** `GET /api/test/email-config`

**Description:** Shows current email configuration (without exposing sensitive data)

**Usage:**

```bash
curl http://localhost:8080/api/test/email-config
```

**Response:**

```json
{
  "smtpServer": "sandbox.smtp.mailtrap.io",
  "smtpPort": 2525,
  "senderEmail": "noreply@bidsphere.com",
  "senderName": "BidSphere",
  "enableSsl": true,
  "username": "abc1***",
  "passwordSet": true,
  "note": "Password and full username are hidden for security"
}
```

---

## 🚀 Quick Start Testing

### Step 1: Start Your Application

```bash
cd BidSphere
dotnet run
```

### Step 2: Test Email Configuration

```bash
# Check if email settings are loaded
curl http://localhost:8080/api/test/email-config
```

**Expected Output:**

- `smtpServer`: "sandbox.smtp.mailtrap.io"
- `smtpPort`: 2525
- `username`: Should show first 4 characters + "\*\*\*"
- `passwordSet`: true

**If you see issues:**

- Username shows "Not set" → Update appsettings.json
- passwordSet is false → Add password to appsettings.json

### Step 3: Send Test Email

```bash
# Send simple test email
curl -X POST http://localhost:8080/api/test/send-test-email
```

**Expected Output:**

```json
{
  "success": true,
  "message": "Test email sent successfully to test@example.com"
}
```

### Step 4: Check Mailtrap Inbox

1. Go to [mailtrap.io](https://mailtrap.io)
2. Login and go to your inbox
3. You should see the email with:
   - Subject: "Congratulations! You Won the Auction - BidSphere"
   - Product: "Test Product - Vintage Watch"
   - Amount: $250.50

---

## 🐛 Troubleshooting

### Error: "Failed to send test email"

**Check the error message in response:**

#### Error: "Authentication failed"

```json
{
  "success": false,
  "error": "Authentication failed"
}
```

**Solution:**

1. Verify Mailtrap credentials in appsettings.json
2. Check username and password are correct
3. No extra spaces in credentials

```bash
# Verify settings
cat BidSphere/appsettings.json | grep -A 8 "EmailSettings"
```

#### Error: "Connection refused" or "Unable to connect"

```json
{
  "success": false,
  "error": "Connection refused"
}
```

**Solution:**

1. Check internet connection
2. Verify SMTP server and port
3. Check firewall settings

```bash
# Test connection to Mailtrap
telnet sandbox.smtp.mailtrap.io 2525
```

#### Error: "SSL/TLS handshake failed"

```json
{
  "success": false,
  "error": "SSL handshake failed"
}
```

**Solution:**
This should be fixed now with `SecureSocketOptions.StartTls`, but if you still see it:

1. Verify port is 2525 (not 465 or 25)
2. Check EmailService.cs uses `SecureSocketOptions.StartTls`

---

## 📧 Testing Different Scenarios

### Test 1: Basic Email

```bash
curl -X POST http://localhost:8080/api/test/send-test-email
```

### Test 2: Email to Your Address

```bash
curl -X POST "http://localhost:8080/api/test/send-test-email?toEmail=your-email@example.com"
```

### Test 3: Custom Product and Amount

```bash
curl -X POST http://localhost:8080/api/test/send-custom-email \
  -H "Content-Type: application/json" \
  -d '{
    "toEmail": "test@example.com",
    "productName": "iPhone 15 Pro",
    "winningBid": 999.99
  }'
```

### Test 4: Multiple Emails

```bash
# Send to multiple addresses to test
for email in test1@example.com test2@example.com test3@example.com; do
  curl -X POST "http://localhost:8080/api/test/send-test-email?toEmail=$email"
  echo ""
done
```

---

## 🎯 Using Swagger UI

If you have Swagger enabled, you can also test from the browser:

1. Go to `http://localhost:8080/swagger`
2. Find **Test** section
3. Expand `POST /api/test/send-test-email`
4. Click "Try it out"
5. Enter email address (optional)
6. Click "Execute"
7. Check response and Mailtrap inbox

---

## 📊 Expected Email Content

When you receive the test email in Mailtrap, it should look like:

```
Subject: Congratulations! You Won the Auction - BidSphere

┌─────────────────────────────────┐
│       🎯 BidSphere              │
└─────────────────────────────────┘

🎉 Congratulations! You Won!

Hi test@example.com,

Great news! You won the auction!

┌─────────────────────────────────┐
│ Product: Test Product - Vintage Watch
│ Your Winning Bid: $250.50
└─────────────────────────────────┘

Please proceed with payment to complete your purchase.

Thank you for using BidSphere! 🎊

────────────────────────────────────
© 2024 BidSphere. All rights reserved.
This is an automated message, please do not reply.
```

---

## 🔒 Security Note

The test endpoints are **public** (no authentication required) for easy testing.

**For production:**

1. Add `[Authorize(Roles = "Admin")]` to test endpoints
2. Or remove the TestController entirely
3. Or move to a separate test project

---

## ✅ Success Checklist

After testing, verify:

- [ ] `/api/test/email-config` shows correct settings
- [ ] `/api/test/send-test-email` returns success
- [ ] Email appears in Mailtrap inbox within seconds
- [ ] Email HTML renders correctly
- [ ] Product name and amount display correctly
- [ ] No errors in application logs

---

## 🚀 Next Steps

Once email testing works:

1. ✅ Email service is configured correctly
2. ✅ Can proceed with auction testing
3. ✅ Emails will be sent automatically when:
   - Auction expires (to highest bidder)
   - Payment times out (to next bidder)
   - Payment retry occurs

You can now test the full auction flow with confidence that emails will work! 📧✨

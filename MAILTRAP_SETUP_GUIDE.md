# 📧 Mailtrap Email Setup Guide

## ✅ What Was Implemented

Email notifications are now integrated into BidSphere using **MailKit** and configured for **Mailtrap** testing.

### Files Created:

1. ✅ `Models/EmailSettings.cs` - Email configuration model
2. ✅ `Service/Interface/IEmailService.cs` - Email service interface
3. ✅ `Service/Implementation/EmailService.cs` - Email service with HTML templates
4. ✅ Updated `appsettings.json` - Email configuration (with dummy credentials)
5. ✅ Updated `Program.cs` - Service registration
6. ✅ Updated `BidService.cs` - Bid placed & outbid notifications
7. ✅ Updated `AuctionExpiryMonitor.cs` - Auction won & ended notifications

### Package Installed:

- ✅ MailKit 4.14.1 (includes MimeKit)

---

## 🚨 WHAT YOU NEED TO DO

### Step 1: Sign Up for Mailtrap (5 minutes)

1. Go to: https://mailtrap.io
2. Click "Sign Up" (free account)
3. Verify your email
4. Login to dashboard

### Step 2: Get Your SMTP Credentials

1. In Mailtrap dashboard, go to **"Email Testing"** → **"Inboxes"**
2. Click on **"My Inbox"** (or create a new inbox)
3. Click on **"SMTP Settings"** tab
4. You'll see credentials like this:

```
Host: sandbox.smtp.mailtrap.io
Port: 2525
Username: abc123def456789
Password: xyz987uvw654321
Auth: PLAIN
TLS: Optional (STARTTLS on all ports)
```

### Step 3: Update appsettings.json

Open `BidSphere/appsettings.json` and replace the dummy credentials:

**FIND THIS:**

```json
"EmailSettings": {
  "SmtpServer": "sandbox.smtp.mailtrap.io",
  "SmtpPort": 2525,
  "SenderEmail": "noreply@bidsphere.com",
  "SenderName": "BidSphere",
  "Username": "YOUR_MAILTRAP_USERNAME",
  "Password": "YOUR_MAILTRAP_PASSWORD",
  "EnableSsl": true
}
```

**REPLACE WITH YOUR CREDENTIALS:**

```json
"EmailSettings": {
  "SmtpServer": "sandbox.smtp.mailtrap.io",
  "SmtpPort": 2525,
  "SenderEmail": "noreply@bidsphere.com",
  "SenderName": "BidSphere",
  "Username": "abc123def456789",  ← YOUR USERNAME HERE
  "Password": "xyz987uvw654321",  ← YOUR PASSWORD HERE
  "EnableSsl": true
}
```

### Step 4: Test It!

Run your application and test email notifications:

#### Test 1: Bid Placed Notification

1. Login as a user
2. Place a bid on any active auction
3. Check Mailtrap inbox - you should see "Bid Placed Successfully" email

#### Test 2: Outbid Notification

1. Login as User A, place a bid
2. Login as User B, place a higher bid
3. Check Mailtrap inbox - User A should receive "You've Been Outbid!" email

#### Test 3: Auction Won Notification

1. Wait for an auction to expire (or manually set expiry time to past)
2. AuctionExpiryMonitor will run (every 10 seconds)
3. Check Mailtrap inbox - winner should receive "Congratulations! You Won" email
4. Product owner should receive "Your Auction Has Ended" email

---

## 📧 Email Notifications Implemented

### 1. Bid Placed (BidService)

**Trigger:** User places a bid
**Recipient:** Bidder
**Subject:** "Bid Placed Successfully - BidSphere"
**Content:** Confirmation with product name and bid amount

### 2. Outbid (BidService)

**Trigger:** Someone places a higher bid
**Recipient:** Previous highest bidder
**Subject:** "You've Been Outbid! - BidSphere"
**Content:** Alert with new highest bid amount

### 3. Auction Won (AuctionExpiryMonitor)

**Trigger:** Auction expires with bids
**Recipient:** Winner
**Subject:** "Congratulations! You Won the Auction - BidSphere"
**Content:** Winning confirmation with final price

### 4. Auction Ended (AuctionExpiryMonitor)

**Trigger:** Auction expires with bids
**Recipient:** Product owner
**Subject:** "Your Auction Has Ended - BidSphere"
**Content:** Summary with winner name and final price

### 5. Auction Ended - No Bids (AuctionExpiryMonitor)

**Trigger:** Auction expires without bids
**Recipient:** Product owner
**Subject:** "Auction Ended - No Bids"
**Content:** Notification that auction ended without bids

---

## 🎨 Email Templates

All emails use professional HTML templates with:

- Responsive design
- Color-coded headers (green for success, orange for alerts, blue for wins)
- Clean formatting
- BidSphere branding
- Footer with copyright

---

## 🔍 How to View Emails in Mailtrap

1. Login to Mailtrap dashboard
2. Go to "Email Testing" → "Inboxes" → "My Inbox"
3. You'll see all "sent" emails listed
4. Click on any email to view:
   - HTML preview (how it looks)
   - Text version
   - Raw source
   - Spam analysis
   - Headers

**Note:** Emails are NOT sent to real inboxes - they're all caught by Mailtrap!

---

## 🚨 Important Notes

### Email Failures Won't Break Your App

- Email sending is wrapped in try-catch blocks
- If email fails, the bid/auction still works
- Errors are logged but don't throw exceptions

### Background Service Timing

- AuctionExpiryMonitor runs every 10 seconds
- Checks for expired auctions
- Sends notifications automatically

### Testing Tips

1. Use different user accounts to test outbid notifications
2. Create short-duration auctions (2-5 minutes) for quick testing
3. Check Mailtrap inbox after each action
4. Look at email HTML to see how they render

---

## 🔒 Security Notes

### DO NOT COMMIT CREDENTIALS

Your `appsettings.json` contains sensitive credentials. Make sure it's in `.gitignore`:

```
# .gitignore
appsettings.json
appsettings.Development.json
appsettings.Production.json
```

### For Production

When deploying to production:

1. Use environment variables instead of appsettings.json
2. Switch from Mailtrap to real email service (Gmail, SendGrid, etc.)
3. Update SMTP settings accordingly

---

## 🐛 Troubleshooting

### Email Not Sending?

1. Check Mailtrap credentials are correct
2. Check application logs for errors
3. Verify EmailService is registered in Program.cs
4. Check Mailtrap inbox (not your real email!)

### Can't Find Emails in Mailtrap?

1. Make sure you're looking at the correct inbox
2. Refresh the page
3. Check "All Messages" tab
4. Verify credentials match your inbox

### Connection Errors?

1. Verify SmtpServer: `sandbox.smtp.mailtrap.io`
2. Verify Port: `2525`
3. Check internet connection
4. Try EnableSsl: `false` if connection fails

---

## 📋 Quick Checklist

Before testing, make sure:

- [ ] Signed up for Mailtrap account
- [ ] Got SMTP credentials from dashboard
- [ ] Updated `appsettings.json` with real credentials
- [ ] Application is running
- [ ] You have test user accounts
- [ ] You have active auctions to bid on

---

## 🎯 What's Next?

Once email notifications are working:

1. Test all notification types
2. Verify HTML rendering looks good
3. Check spam score in Mailtrap
4. Consider adding more notification types (payment confirmations, etc.)

---

## 📞 Need Help?

If emails aren't working:

1. Check application logs for errors
2. Verify Mailtrap credentials
3. Test with a simple bid first
4. Check Mailtrap dashboard for connection attempts

**Remember:** All emails go to Mailtrap, not real inboxes. This is perfect for testing!

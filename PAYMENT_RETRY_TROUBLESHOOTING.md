# Payment Retry Logic - Troubleshooting Guide

## 🔄 Payment Flow Overview

### Step 1: Auction Expires

- **AuctionExpiryMonitor** marks auctions as `Expired` (runs every 30 seconds)

### Step 2: Payment Initiation

- **AuctionFinalizer** detects expired auctions without payment attempts (runs every 10 seconds)
- Creates first `PaymentAttempt` with `AttemptNumber = 1`
- Sends email to highest bidder
- Status: `Pending`

### Step 3: Payment Timeout Detection

- **RetryQueueService** checks for timed-out payments (runs every 10 seconds)
- Timeout threshold: `AuctionConfig.PaymentTimeoutSeconds` (default: 60 seconds)
- If payment is still `Pending` after timeout → triggers retry

### Step 4: Retry Logic

- Marks current payment as `Failed`
- If `AttemptNumber < MaxPaymentAttempts` (default: 3):
  - Gets next highest bidder (excluding all failed bidders)
  - Creates new `PaymentAttempt` with incremented `AttemptNumber`
  - Sends email to new bidder
- If no more bidders or max attempts reached:
  - Marks auction as `Failed`

### Step 5: Payment Confirmation

- User confirms payment via API: `PUT /api/products/{id}/confirm`
- Updates payment status to `Success`
- Records `ConfirmedAmount` and `ConfirmedAt`

---

## 🐛 Common Issues & Solutions

### Issue 1: Background Services Not Running

**Symptoms:**

- Auctions stay in `Active` status even after expiry
- No payment attempts created
- No emails sent

**Check:**

```bash
# Verify services are registered in Program.cs
grep -A 5 "AddHostedService" BidSphere/Program.cs
```

**Solution:**
Ensure all three background services are registered:

```csharp
builder.Services.AddHostedService<AuctionExpiryMonitor>();
builder.Services.AddHostedService<AuctionFinalizer>();
builder.Services.AddHostedService<RetryQueueService>();
```

---

### Issue 2: Navigation Property Null Reference

**Symptoms:**

```
NullReferenceException: Object reference not set to an instance of an object
at BidSphere.BackgroundServices.AuctionFinalizer.FinalizeExpiredAuctions()
```

**Root Cause:**

- Missing `.Include()` statements for navigation properties
- Auction.HighestBid, Auction.Product, or Bid.Bidder is null

**Solution:**
Already implemented in AuctionFinalizer:

```csharp
var expiredAuctions = await context.Auctions
    .Include(a => a.Product)
    .Include(a => a.HighestBid)
        .ThenInclude(b => b.Bidder)
    .Include(a => a.PaymentAttempts)
    .Where(a => a.Status == AuctionStatus.Expired && !a.PaymentAttempts.Any())
    .ToListAsync();
```

**Additional Check:**
Verify HighestBid is set when auction expires:

```csharp
// In AuctionExpiryMonitor
auction.HighestBidId = highestBid?.BidId;
```

---

### Issue 3: Email Service Errors

**Symptoms:**

```
Failed to send auction won email to user@example.com: ...
```

**Root Cause:**

- Invalid SMTP settings
- Network connectivity issues
- Email address is null or invalid

**Solution:**

1. Check `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpServer": "sandbox.smtp.mailtrap.io",
    "SmtpPort": 2525,
    "Username": "your-username",
    "Password": "your-password",
    "SenderEmail": "noreply@bidsphere.com",
    "SenderName": "BidSphere",
    "EnableSsl": true
  }
}
```

2. Email service catches exceptions internally - check logs:

```bash
# Search for email errors in logs
grep "Failed to send" logs/*.log
```

---

### Issue 4: Payment Timeout Not Triggering

**Symptoms:**

- Payments stay in `Pending` status forever
- No retry attempts created

**Root Cause:**

- `PaymentTimeoutSeconds` is too high
- RetryQueueService not running
- Timezone issues with DateTime comparison

**Check:**

```csharp
// In RetryQueueService
var timeoutThreshold = DateTime.UtcNow.AddSeconds(-AuctionConfig.PaymentTimeoutSeconds);
```

**Solution:**

1. Verify timeout configuration:

```bash
# Check current timeout value
curl http://localhost:5000/api/config
```

2. Adjust timeout for testing:

```bash
curl -X PUT http://localhost:5000/api/config \
  -H "Content-Type: application/json" \
  -d '{
    "paymentTimeoutSeconds": 30,
    "maxPaymentAttempts": 3
  }'
```

3. Ensure using UTC time consistently:

```csharp
// Always use DateTime.UtcNow, never DateTime.Now
AttemptTime = DateTime.UtcNow
```

---

### Issue 5: Duplicate Payment Attempts

**Symptoms:**

- Multiple payment attempts created for same bidder
- Attempt numbers skip or duplicate

**Root Cause:**

- Race condition between AuctionFinalizer and RetryQueueService
- Missing check for existing payment attempts

**Solution:**
Already implemented - AuctionFinalizer checks:

```csharp
.Where(a => a.Status == AuctionStatus.Expired && !a.PaymentAttempts.Any())
```

---

### Issue 6: No Next Bidder Found

**Symptoms:**

```
No more bidders available for auction 123, marking as FAILED
```

**Root Cause:**

- Only one bidder participated
- All bidders have failed payment attempts

**Expected Behavior:**
This is correct - auction should be marked as `Failed`

**Verify:**

```sql
-- Check bid history
SELECT * FROM Bids WHERE AuctionId = 123 ORDER BY Amount DESC;

-- Check payment attempts
SELECT * FROM PaymentAttempts WHERE AuctionId = 123 ORDER BY AttemptNumber;
```

---

### Issue 7: Max Attempts Reached

**Symptoms:**

```
Max payment attempts (3) reached for auction 123, marking as FAILED
```

**Root Cause:**

- All retry attempts exhausted
- No successful payment

**Expected Behavior:**
This is correct - auction should be marked as `Failed` after max attempts

**Adjust Max Attempts:**

```bash
curl -X PUT http://localhost:5000/api/config \
  -H "Content-Type: application/json" \
  -d '{
    "maxPaymentAttempts": 5
  }'
```

---

### Issue 8: Database Concurrency Issues

**Symptoms:**

```
DbUpdateConcurrencyException: The database operation was expected to affect 1 row(s) but actually affected 0 row(s)
```

**Root Cause:**

- Multiple services trying to update same entity
- Entity was modified between read and update

**Solution:**
Add retry logic with exponential backoff (if needed):

```csharp
// In background services
try
{
    await paymentRepository.UpdateAsync(payment);
}
catch (DbUpdateConcurrencyException ex)
{
    _logger.LogWarning(ex, "Concurrency conflict updating payment {PaymentId}, retrying...", payment.PaymentId);
    // Reload entity and retry
    await _context.Entry(payment).ReloadAsync();
    await _context.SaveChangesAsync();
}
```

---

## 🧪 Testing the Payment Flow

### Test Scenario 1: Successful Payment on First Attempt

```bash
# 1. Create auction that expires in 1 minute
curl -X POST http://localhost:5000/api/products \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Product",
    "startingPrice": 100,
    "description": "Test",
    "category": "Electronics",
    "auctionDurationMinutes": 1
  }'

# 2. Place bid
curl -X POST http://localhost:5000/api/bids \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "auctionId": 1,
    "amount": 150
  }'

# 3. Wait for auction to expire (1 minute + 30 seconds for monitor)

# 4. Check payment attempt created
curl http://localhost:5000/api/transactions \
  -H "Authorization: Bearer $USER_TOKEN"

# 5. Confirm payment
curl -X PUT http://localhost:5000/api/products/1/confirm \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "confirmedAmount": 150
  }'
```

### Test Scenario 2: Payment Timeout and Retry

```bash
# 1. Set short timeout for testing
curl -X PUT http://localhost:5000/api/config \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "paymentTimeoutSeconds": 30
  }'

# 2. Create auction with multiple bidders
# ... (create auction, place multiple bids)

# 3. Wait for auction to expire

# 4. DON'T confirm payment - let it timeout (30 seconds)

# 5. Check retry attempt created
curl http://localhost:5000/api/transactions \
  -H "Authorization: Bearer $ADMIN_TOKEN"

# Should see:
# - Attempt #1: Failed (first bidder)
# - Attempt #2: Pending (second bidder)
```

### Test Scenario 3: Instant Fail for Testing

```bash
# Use testInstantFail parameter to simulate payment failure
curl -X PUT http://localhost:5000/api/products/1/confirm?testInstantFail=true \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "confirmedAmount": 150
  }'

# This immediately marks payment as Failed and triggers retry
```

---

## 📊 Monitoring & Debugging

### Check Background Service Logs

```bash
# Filter logs by service
grep "Auction Finalizer" logs/app.log
grep "Retry Queue Service" logs/app.log
grep "Auction Expiry Monitor" logs/app.log
```

### Key Log Messages

**AuctionFinalizer:**

```
✅ "Finalized {Count} expired auctions"
✅ "Created payment attempt #1 for auction {AuctionId}"
✅ "Auction won email sent to {Email}"
⚠️ "Auction {AuctionId} expired without bids, skipping payment flow"
❌ "Failed to finalize auction {AuctionId}"
```

**RetryQueueService:**

```
✅ "Created payment attempt #{AttemptNumber} for auction {AuctionId}, new bidder {BidderId}"
✅ "Retry email sent to {Email}"
⚠️ "No more bidders available for auction {AuctionId}, marking as FAILED"
⚠️ "Max payment attempts ({MaxAttempts}) reached for auction {AuctionId}"
❌ "Error processing timed out payments"
```

### Database Queries for Debugging

```sql
-- Check auction status
SELECT AuctionId, Status, ExpiryTime, HighestBidId
FROM Auctions
WHERE Status = 'Expired'
ORDER BY ExpiryTime DESC;

-- Check payment attempts
SELECT pa.PaymentId, pa.AuctionId, pa.BidderId, pa.Status,
       pa.AttemptNumber, pa.AttemptTime, pa.ConfirmedAt
FROM PaymentAttempts pa
ORDER BY pa.AuctionId, pa.AttemptNumber;

-- Check failed payments
SELECT pa.*, a.Status as AuctionStatus
FROM PaymentAttempts pa
JOIN Auctions a ON pa.AuctionId = a.AuctionId
WHERE pa.Status = 'Failed'
ORDER BY pa.AttemptTime DESC;

-- Check pending payments older than timeout
SELECT pa.*,
       DATEDIFF(SECOND, pa.AttemptTime, GETUTCDATE()) as SecondsSinceAttempt
FROM PaymentAttempts pa
WHERE pa.Status = 'Pending'
  AND DATEDIFF(SECOND, pa.AttemptTime, GETUTCDATE()) > 60;
```

---

## 🔧 Configuration Reference

### AuctionConfig Constants

Located in: `BidSphere/Constants/AuctionConfig.cs`

```csharp
public static class AuctionConfig
{
    public static int AntiSnipingThresholdSeconds { get; set; } = 60;
    public static int ExtensionDurationSeconds { get; set; } = 60;
    public static int PaymentTimeoutSeconds { get; set; } = 60;
    public static int MaxPaymentAttempts { get; set; } = 3;
}
```

### Background Service Intervals

- **AuctionExpiryMonitor**: 30 seconds
- **AuctionFinalizer**: 10 seconds
- **RetryQueueService**: 10 seconds

---

## 🎯 Quick Diagnostic Checklist

When payment retry logic isn't working:

- [ ] All 3 background services registered in Program.cs
- [ ] Background services are running (check logs)
- [ ] Auction has `HighestBidId` set when it expires
- [ ] Email settings configured correctly
- [ ] `PaymentTimeoutSeconds` is reasonable (60 seconds default)
- [ ] Using `DateTime.UtcNow` consistently
- [ ] Navigation properties loaded with `.Include()`
- [ ] Database has multiple bids for testing retries
- [ ] No database concurrency errors in logs

---

## 📞 Need More Help?

Share the following information:

1. **Error message** (full stack trace)
2. **Logs** from background services
3. **Database state**:
   - Auction status
   - Payment attempts
   - Bid history
4. **Configuration** (`GET /api/config`)
5. **Steps to reproduce** the issue

This will help diagnose the specific problem you're encountering!

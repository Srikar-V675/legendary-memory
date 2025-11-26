# Background Services Flow Diagram

## 🔄 Complete Auction Lifecycle

```
┌─────────────────────────────────────────────────────────────────┐
│                    AUCTION LIFECYCLE                            │
└─────────────────────────────────────────────────────────────────┘

1. ACTIVE AUCTION
   ↓
   User places bids
   ↓
   Time passes...
   ↓

2. EXPIRY DETECTION (AuctionExpiryMonitor - every 10s)
   ↓
   Checks: ExpiryTime <= Now && Status = Active
   ↓
   Marks as: EXPIRED
   ↓
   Logs: "Auction {id} marked as EXPIRED"
   ↓

3. FINALIZATION (AuctionFinalizer - every 10s)
   ↓
   Checks: Status = Expired && No payment attempts
   ↓
   Has bids? ──NO──> Skip (no payment needed)
   ↓ YES
   Creates: PaymentAttempt #1 (Pending)
   ↓
   Sends: Email to Winner #1
   ↓
   Logs: "Created payment attempt #1"
   ↓

4. PAYMENT WAITING (60 seconds)
   ↓
   Winner confirms? ──YES──> COMPLETED ✅
   ↓ NO (timeout)

5. RETRY QUEUE (RetryQueueService - every 10s)
   ↓
   Checks: Pending payments older than 60s
   ↓
   Marks: Payment #1 as Failed
   ↓
   Gets: Next highest bidder
   ↓
   Creates: PaymentAttempt #2 (Pending)
   ↓
   Sends: Email to Winner #2
   ↓
   Logs: "Created payment attempt #2"
   ↓

6. RETRY WAITING (60 seconds)
   ↓
   Winner #2 confirms? ──YES──> COMPLETED ✅
   ↓ NO (timeout)

7. FINAL RETRY (RetryQueueService)
   ↓
   Marks: Payment #2 as Failed
   ↓
   Gets: Next highest bidder
   ↓
   Creates: PaymentAttempt #3 (Pending)
   ↓
   Sends: Email to Winner #3
   ↓

8. LAST CHANCE (60 seconds)
   ↓
   Winner #3 confirms? ──YES──> COMPLETED ✅
   ↓ NO (timeout)

9. ALL ATTEMPTS FAILED
   ↓
   Marks: Payment #3 as Failed
   ↓
   Marks: Auction as FAILED ❌
   ↓
   Logs: "Max attempts reached, auction FAILED"
```

---

## 🔧 Service Responsibilities

### 1️⃣ AuctionExpiryMonitor

```
┌─────────────────────────────────┐
│   AuctionExpiryMonitor          │
│   (Runs every 10 seconds)       │
├─────────────────────────────────┤
│ Input:                          │
│   - Active auctions             │
│   - Current time                │
├─────────────────────────────────┤
│ Process:                        │
│   1. Find expired auctions      │
│   2. Mark as EXPIRED            │
│   3. Save changes               │
├─────────────────────────────────┤
│ Output:                         │
│   - Auctions marked EXPIRED     │
│   - Logs: "Marked X auctions"   │
└─────────────────────────────────┘
```

### 2️⃣ AuctionFinalizer

```
┌─────────────────────────────────┐
│   AuctionFinalizer              │
│   (Runs every 10 seconds)       │
├─────────────────────────────────┤
│ Input:                          │
│   - Expired auctions            │
│   - No payment attempts         │
├─────────────────────────────────┤
│ Process:                        │
│   1. Check for bids             │
│   2. Create payment attempt #1  │
│   3. Send email to winner       │
│   4. Log actions                │
├─────────────────────────────────┤
│ Output:                         │
│   - Payment attempt created     │
│   - Email sent                  │
│   - Logs: "Finalized X auctions"│
└─────────────────────────────────┘
```

### 3️⃣ RetryQueueService

```
┌─────────────────────────────────┐
│   RetryQueueService             │
│   (Runs every 10 seconds)       │
├─────────────────────────────────┤
│ Input:                          │
│   - Pending payments            │
│   - Timeout threshold (60s)     │
├─────────────────────────────────┤
│ Process:                        │
│   1. Find timed-out payments    │
│   2. Mark as Failed             │
│   3. Get next bidder            │
│   4. Create new payment attempt │
│   5. Send email to new winner   │
│   6. Or mark auction as FAILED  │
├─────────────────────────────────┤
│ Output:                         │
│   - New payment attempt         │
│   - Email sent                  │
│   - Or auction marked FAILED    │
└─────────────────────────────────┘
```

---

## ⏱️ Timeline Example

```
Time    | Service              | Action
--------|----------------------|--------------------------------
10:00   | User                 | Places final bid ($200)
10:02   | AuctionExpiryMonitor | Marks auction as EXPIRED
10:02   | AuctionFinalizer     | Creates payment #1, emails winner
10:02   | Winner #1            | Receives email
10:03   | RetryQueueService    | Checks (payment still pending, < 60s)
...     | ...                  | ...
11:02   | RetryQueueService    | Timeout! Marks payment #1 as Failed
11:02   | RetryQueueService    | Creates payment #2, emails winner #2
11:02   | Winner #2            | Receives email
11:05   | Winner #2            | Confirms payment ✅
11:05   | API                  | Marks auction as COMPLETED
```

---

## 🔍 Service Interaction

```
┌──────────────────┐
│  Active Auction  │
└────────┬─────────┘
         │
         ↓ (time passes)
┌──────────────────┐
│ ExpiryMonitor    │──> Marks as EXPIRED
└────────┬─────────┘
         │
         ↓ (next cycle)
┌──────────────────┐
│ AuctionFinalizer │──> Creates payment #1
└────────┬─────────┘     Sends email
         │
         ↓ (60 seconds)
┌──────────────────┐
│ RetryQueue       │──> Checks timeout
└────────┬─────────┘
         │
         ├──> Confirmed? ──YES──> COMPLETED ✅
         │
         └──> Timeout? ──YES──> Create payment #2
                                 Repeat...
```

---

## 📊 Status Transitions

```
ACTIVE
  ↓ (AuctionExpiryMonitor)
EXPIRED
  ↓ (AuctionFinalizer)
EXPIRED + Payment #1 Pending
  ↓ (User confirms OR RetryQueue timeout)
  ├──> COMPLETED ✅
  └──> Payment #1 Failed → Payment #2 Pending
         ↓ (User confirms OR RetryQueue timeout)
         ├──> COMPLETED ✅
         └──> Payment #2 Failed → Payment #3 Pending
                ↓ (User confirms OR RetryQueue timeout)
                ├──> COMPLETED ✅
                └──> Payment #3 Failed → FAILED ❌
```

---

## 🚨 Error Scenarios

### Scenario 1: Email Fails

```
AuctionFinalizer
  ↓
Creates payment attempt ✅
  ↓
Sends email ❌ (fails)
  ↓
Logs error
  ↓
Payment attempt still exists
  ↓
RetryQueue will handle timeout
```

### Scenario 2: No More Bidders

```
RetryQueue
  ↓
Payment #1 times out
  ↓
Tries to get next bidder
  ↓
No more bidders found
  ↓
Marks auction as FAILED ❌
```

### Scenario 3: Database Error

```
AuctionExpiryMonitor
  ↓
Finds expired auctions
  ↓
Tries to save changes ❌
  ↓
Logs error
  ↓
Retries next cycle (10s)
```

---

## 🎯 Key Points

1. **Independent Services:** Each service can fail without affecting others
2. **Idempotent:** Services can run multiple times safely
3. **Retry Logic:** Built-in retry with exponential backoff
4. **Logging:** Comprehensive logging at each step
5. **Error Isolation:** Errors in one auction don't affect others

---

## 📝 Monitoring Checklist

Watch for these log messages:

### AuctionExpiryMonitor:

- ✅ "Auction Expiry Monitor started"
- ✅ "Auction {id} marked as EXPIRED"
- ✅ "Marked {count} auctions as expired"

### AuctionFinalizer:

- ✅ "Auction Finalizer started"
- ✅ "Created payment attempt #1"
- ✅ "Auction won email sent to {email}"
- ✅ "Finalized {count} expired auctions"

### RetryQueueService:

- ✅ "Retry Queue Service started"
- ✅ "Processing timed out payment {id}"
- ✅ "Created payment attempt #{number}"
- ✅ "Retry email sent to {email}"
- ⚠️ "Max payment attempts reached"
- ⚠️ "No more bidders available"

---

## ✅ All Services Working Together!

Three independent services working in harmony to handle the complete auction lifecycle from expiry to payment completion or failure.

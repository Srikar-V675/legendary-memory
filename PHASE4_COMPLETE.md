# Phase 4 Complete: Bidding & Anti-Sniping

## ✅ What We Implemented

### 1. Validator

- **PlaceBidDtoValidator** - Validates AuctionId > 0 and Amount > 0

### 2. Bid Repository

- **IBidRepository** and **BidRepository**
- CreateAsync - Save new bid
- GetByAuctionIdAsync - Get all bids for auction (ordered by timestamp)
- GetHighestBidAsync - Get current highest bid
- GetNextHighestBidderAsync - Get next highest (for payment retry in Phase 6)

### 3. Bid Service

- **IBidService** and **BidService**
- **PlaceBidAsync** - Complete bid placement logic:
  - Validates auction exists and is active
  - Validates user is not product owner
  - Validates bid > current highest
  - **Anti-sniping**: Extends auction if bid within last 60 seconds
  - Creates bid and updates auction highest bid
- **GetBidsByAuctionAsync** - Returns all bids for an auction

### 4. Bids Controller

- **POST /api/bids** [Authorize] - Place a bid
- **GET /api/bids/{auctionId}** - Get all bids for auction (public)

### 5. Background Service

- **AuctionExpiryMonitor** - Runs every 10 seconds
- Finds auctions where Status=Active and ExpiryTime <= Now
- Updates status to Expired
- Logs each expired auction

### 6. Updated Auction Repository

- Added **GetExpiredAuctionsAsync** method

---

## 🔐 Authorization

| Endpoint                  | Auth Required | Notes                  |
| ------------------------- | ------------- | ---------------------- |
| POST /api/bids            | Yes           | Any authenticated user |
| GET /api/bids/{auctionId} | No            | Public endpoint        |

---

## ✅ Validations

### Input Validation (FluentValidation)

- AuctionId must be > 0
- Amount must be > 0

### Business Validation (Service Layer)

- Auction must exist
- Auction must be ACTIVE
- Bid amount must be > current highest bid (or starting price if no bids)
- User cannot bid on their own product

---

## 🎯 Anti-Sniping Logic

**Threshold:** 60 seconds (last minute)
**Extension:** 60 seconds (1 minute)

**How it works:**

```csharp
var timeRemaining = auction.ExpiryTime - DateTime.UtcNow;
if (timeRemaining.TotalSeconds < 60 && timeRemaining.TotalSeconds > 0)
{
    auction.ExpiryTime = auction.ExpiryTime.AddMinutes(1);
    auction.ExtensionCount++;
    await _auctionRepository.UpdateAsync(auction);
}
```

**Features:**

- Automatically extends auction by 1 minute
- Can extend multiple times
- Tracks extension count
- Only extends if time remaining < 60 seconds

---

## 🔄 Background Service

**AuctionExpiryMonitor:**

- Runs continuously every 10 seconds
- Checks for expired auctions
- Updates status from Active → Expired
- Logs: "Auction {id} expired"

**You can see it in logs:**

```
info: BidSphere.BackgroundServices.AuctionExpiryMonitor[0]
      Auction Expiry Monitor started
```

---

## 🧪 Testing Guide

### 1. Login as Admin

```bash
POST /login?useCookies=false
{
  "email": "admin@bidsphere.com",
  "password": "Admin@123"
}
# Copy accessToken
```

### 2. Authorize in Swagger

- Click "Authorize" button
- Enter: `Bearer {your-token}`

### 3. Place a Bid

```bash
POST /api/bids
Authorization: Bearer {token}

{
  "auctionId": 1,
  "amount": 6000
}

# Should succeed if:
# - Amount > starting price ($5000)
# - Auction is active
# - You're not the product owner
```

### 4. View Bids

```bash
GET /api/bids/1

# Should return list of bids with:
# - BidId
# - AuctionId
# - BidderId
# - BidderEmail
# - Amount
# - Timestamp
```

### 5. Test Anti-Sniping

```bash
# Create a product with 2-minute duration
POST /api/products
{
  "name": "Test Product",
  "description": "For anti-sniping test",
  "category": "Electronics",
  "startingPrice": 100,
  "auctionDurationMinutes": 2
}

# Wait 1 minute 30 seconds
# Place a bid
POST /api/bids
{
  "auctionId": {new-auction-id},
  "amount": 150
}

# Check product details
GET /api/products/{id}
# ExpiryTime should be extended by 1 minute
# ExtensionCount should be 1
```

### 6. Test Validations

**Bid too low:**

```bash
POST /api/bids
{
  "auctionId": 1,
  "amount": 4000  # Less than current highest
}
# Should fail: "Bid must be higher than current highest bid of $5000"
```

**Bid on own product:**

```bash
# Login as admin (product owner)
POST /api/bids
{
  "auctionId": 1,
  "amount": 10000
}
# Should fail: "Cannot bid on your own product"
```

**Bid on expired auction:**

```bash
# Wait for auction to expire (or use finalize endpoint)
POST /api/bids
{
  "auctionId": 1,
  "amount": 6000
}
# Should fail: "Auction is not active"
```

---

## 📊 What Happens When You Bid

1. **Validation:**

   - Input validation (FluentValidation)
   - Business validation (Service layer)

2. **Anti-Sniping Check:**

   - If < 60 seconds remaining → extend by 60 seconds

3. **Create Bid:**

   - Save bid to database
   - Set timestamp

4. **Update Auction:**

   - Set HighestBidId to new bid
   - Save auction

5. **Return:**
   - Return BidDto with all details

---

## 🎯 Current Status

✅ Bid placement working
✅ Anti-sniping extension working
✅ Background service monitoring auctions
✅ All validations working
✅ Authorization working
✅ Can view bids for any auction
✅ Highest bid tracking working

---

## 📋 Next Steps (Phase 5)

**Excel Upload & ASQL Parser**

1. Excel upload for bulk product creation
2. ASQL parser for advanced filtering
3. Pagination for list endpoints

---

## 🎉 Phase 4 Complete!

All bidding functionality is working:

- Users can place bids
- Anti-sniping extends auctions automatically
- Background service marks expired auctions
- All validations in place
- Ready for Phase 5!

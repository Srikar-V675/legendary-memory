# Phase 4 Plan: Bidding & Anti-Sniping (Milestone 2)

## 🎯 Goal

Implement bid placement with validations and dynamic auction extension (anti-sniping logic).

---

## 📋 What We'll Build

### 1. DTOs (2 files)

**Location:** `Models/Dtos/Bids/`

#### PlaceBidDto (Already exists)

```csharp
- AuctionId (int)
- Amount (decimal)
```

#### BidDto (Already exists)

```csharp
- BidId (int)
- AuctionId (int)
- BidderId (int)
- BidderEmail (string)
- Amount (decimal)
- Timestamp (DateTime)
```

**Status:** ✅ Already created in Phase 1

---

### 2. Validator (1 file)

**Location:** `Validators/PlaceBidDtoValidator.cs`

**Validation Rules:**

- AuctionId: Required, must be > 0
- Amount: Required, must be > 0

**Note:** Business validations (bid > highest, auction active, etc.) will be in service layer

---

### 3. Bid Repository (2 files)

**Location:** `Repository/Interface/IBidRepository.cs` and `Repository/Implementation/BidRepository.cs`

**Methods to implement:**

```csharp
Task<Bid> CreateAsync(Bid bid)
Task<IEnumerable<Bid>> GetByAuctionIdAsync(int auctionId)
Task<Bid?> GetHighestBidAsync(int auctionId)
Task<Bid?> GetNextHighestBidderAsync(int auctionId, int excludeBidderId)
Task<IEnumerable<Bid>> GetAllAsync() // For filtering later
```

**Key Points:**

- GetHighestBidAsync: Order by Amount DESC, take first
- GetNextHighestBidderAsync: Exclude current bidder, order by Amount DESC, take first
- Include Bidder and Auction in queries for complete data

---

### 4. Bid Service (2 files)

**Location:** `Service/Interface/IBidService.cs` and `Service/Implementation/BidService.cs`

**Methods to implement:**

```csharp
Task<BidDto> PlaceBidAsync(int userId, PlaceBidDto bidDto)
Task<IEnumerable<BidDto>> GetBidsByAuctionAsync(int auctionId)
```

**PlaceBidAsync Business Logic:**

1. **Get Auction:**

   - Fetch auction by AuctionId
   - Include Product and HighestBid

2. **Validations (throw exceptions if fail):**

   - Auction must exist
   - Auction status must be ACTIVE
   - Bid amount must be > current highest bid (or starting price if no bids)
   - User cannot bid on their own product (check Product.OwnerId != userId)

3. **Anti-Sniping Check:**

   ```csharp
   var timeRemaining = auction.ExpiryTime - DateTime.UtcNow;
   if (timeRemaining.TotalSeconds < 60) // Last minute
   {
       auction.ExpiryTime = auction.ExpiryTime.AddMinutes(1);
       auction.ExtensionCount++;
       await _auctionRepository.UpdateAsync(auction);
   }
   ```

4. **Create Bid:**

   - Create new Bid entity
   - Set Timestamp = DateTime.UtcNow
   - Save bid

5. **Update Auction:**

   - Set auction.HighestBidId = newBid.BidId
   - Update auction

6. **Return:**
   - Map to BidDto and return

**GetBidsByAuctionAsync:**

- Get all bids for auction
- Order by Timestamp DESC (newest first)
- Map to BidDto list

---

### 5. Bids Controller (1 file)

**Location:** `Controllers/BidsController.cs`

**Endpoints:**

#### POST /api/bids [Authorize]

```csharp
- Get userId from JWT claims
- Validate PlaceBidDto using validator
- Call BidService.PlaceBidAsync
- Return 201 Created with BidDto
- Catch exceptions and return 400 BadRequest
```

#### GET /api/bids/{auctionId}

```csharp
- Call BidService.GetBidsByAuctionAsync
- Return 200 OK with list of BidDto
- Public endpoint (no auth required)
```

**Authorization:**

- POST requires authentication (any logged-in user)
- GET is public

---

### 6. Background Service (1 file)

**Location:** `BackgroundServices/AuctionExpiryMonitor.cs`

**Purpose:** Monitor auctions and mark expired ones

**Implementation:**

```csharp
public class AuctionExpiryMonitor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckExpiredAuctions();
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task CheckExpiredAuctions()
    {
        // Get all ACTIVE auctions where ExpiryTime <= Now
        // Update status to EXPIRED
        // Log each expired auction
    }
}
```

**Logic:**

1. Run every 10 seconds
2. Query: `Status == Active && ExpiryTime <= DateTime.UtcNow`
3. For each expired auction:
   - Update Status = AuctionStatus.Expired
   - Save to database
   - Log: "Auction {id} expired"

**Note:** Payment flow will be added in Phase 6

---

### 7. Update Auction Repository

**Location:** `Repository/Implementation/AuctionRepository.cs`

**Add method:**

```csharp
Task<IEnumerable<Auction>> GetExpiredAuctionsAsync()
{
    return await _context.Auctions
        .Where(a => a.Status == AuctionStatus.Active &&
                    a.ExpiryTime <= DateTime.UtcNow)
        .ToListAsync();
}
```

---

### 8. AutoMapper Updates

**Location:** `Mapper/ProductMapper.cs` (or create new BidMapper.cs)

**Mappings needed:**

```csharp
CreateMap<Bid, BidDto>()
    .ForMember(dest => dest.BidderEmail, opt => opt.MapFrom(src => src.Bidder.Email));
```

**Status:** ✅ Already added in Phase 3

---

### 9. Register Services in Program.cs

**Add to DI container:**

```csharp
// Repositories
builder.Services.AddScoped<IBidRepository, BidRepository>();

// Services
builder.Services.AddScoped<IBidService, BidService>();

// Background Services
builder.Services.AddHostedService<AuctionExpiryMonitor>();
```

---

## 🔐 Authorization Summary

| Endpoint                  | Auth Required | Role Required          | Notes                     |
| ------------------------- | ------------- | ---------------------- | ------------------------- |
| POST /api/bids            | Yes           | Any authenticated user | Cannot bid on own product |
| GET /api/bids/{auctionId} | No            | Public                 | Anyone can view bids      |

---

## ✅ Validation Summary

### Input Validation (FluentValidation)

- AuctionId > 0
- Amount > 0

### Business Validation (Service Layer)

- Auction exists
- Auction is ACTIVE
- Bid amount > current highest (or starting price)
- User is not product owner

---

## 🎯 Anti-Sniping Logic

### Threshold: 60 seconds (last minute)

### Extension: 60 seconds (1 minute)

**Flow:**

```
1. User places bid
2. Check: ExpiryTime - Now < 60 seconds?
3. If YES:
   - ExpiryTime = ExpiryTime + 60 seconds
   - ExtensionCount++
   - Update auction
4. Continue with bid placement
```

**Multiple Extensions:**

- Can happen multiple times
- Each bid in last minute extends by 1 minute
- ExtensionCount tracks how many times extended

---

## 📊 Database Changes

### Bid Table (Already exists from Phase 1)

- BidId (PK)
- AuctionId (FK)
- BidderId (FK)
- Amount
- Timestamp

### Auction Table Updates

- HighestBidId will be updated when bids placed
- ExpiryTime will be updated for anti-sniping
- ExtensionCount will increment

---

## 🧪 Testing Scenarios

### Scenario 1: Place Valid Bid

```
1. Login as user
2. POST /api/bids { auctionId: 1, amount: 6000 }
3. Should succeed
4. GET /api/bids/1 should show new bid
5. GET /api/products/1 should show updated highest bid
```

### Scenario 2: Bid Too Low

```
1. Current highest: $5000
2. POST /api/bids { auctionId: 1, amount: 4000 }
3. Should fail: "Bid must be higher than current highest bid"
```

### Scenario 3: Bid on Own Product

```
1. Login as admin (product owner)
2. POST /api/bids { auctionId: 1, amount: 10000 }
3. Should fail: "Cannot bid on your own product"
```

### Scenario 4: Bid on Expired Auction

```
1. Wait for auction to expire
2. POST /api/bids { auctionId: 1, amount: 6000 }
3. Should fail: "Auction is not active"
```

### Scenario 5: Anti-Sniping

```
1. Auction expires in 30 seconds
2. POST /api/bids { auctionId: 1, amount: 6000 }
3. Should succeed
4. Auction expiry should extend by 1 minute
5. ExtensionCount should be 1
```

### Scenario 6: Multiple Extensions

```
1. Place bid with 30 seconds left → extends to 1:30
2. Place another bid with 30 seconds left → extends to 2:30
3. ExtensionCount should be 2
```

### Scenario 7: Background Service

```
1. Create auction with 1 minute duration
2. Wait 1 minute
3. Background service should mark as EXPIRED
4. Verify status changed
```

---

## 📝 Implementation Order

### Step 1: Validator (5 mins)

- Create PlaceBidDtoValidator
- Simple validation rules

### Step 2: Bid Repository (15 mins)

- Create IBidRepository interface
- Implement BidRepository
- All CRUD methods

### Step 3: Bid Service (30 mins)

- Create IBidService interface
- Implement BidService
- PlaceBidAsync with all validations
- Anti-sniping logic
- GetBidsByAuctionAsync

### Step 4: Bids Controller (15 mins)

- Create BidsController
- POST /api/bids endpoint
- GET /api/bids/{auctionId} endpoint
- Validation and error handling

### Step 5: Background Service (15 mins)

- Create AuctionExpiryMonitor
- Implement expiry check logic
- Register as hosted service

### Step 6: Update Auction Repository (5 mins)

- Add GetExpiredAuctionsAsync method

### Step 7: Register Services (5 mins)

- Update Program.cs with DI registrations

### Step 8: Testing (15 mins)

- Test all scenarios
- Verify anti-sniping works
- Verify background service works

**Total Time: ~1.5 hours**

---

## 🚨 Important Notes

### Anti-Sniping Configuration

For now, hardcode 60 seconds. In Phase 7, we'll make it configurable via AuctionConfig.

### Background Service

- Runs continuously every 10 seconds
- Uses scoped services (need to create scope)
- Logs expired auctions

### Error Messages

Keep them simple and clear:

- "Auction not found"
- "Auction is not active"
- "Bid must be higher than current highest bid of $X"
- "Cannot bid on your own product"

### Highest Bid Tracking

- Auction.HighestBidId always points to current highest bid
- Makes it easy to get current highest without querying all bids

---

## ✅ Deliverables Checklist

After Phase 4, we should have:

- [ ] PlaceBidDtoValidator created
- [ ] IBidRepository and BidRepository created
- [ ] IBidService and BidService created
- [ ] BidsController with 2 endpoints
- [ ] AuctionExpiryMonitor background service
- [ ] GetExpiredAuctionsAsync in AuctionRepository
- [ ] All services registered in Program.cs
- [ ] Can place bids successfully
- [ ] Anti-sniping extends auctions
- [ ] Background service marks expired auctions
- [ ] All validations working
- [ ] Authorization working (users can't bid on own products)

---

## 🔄 What's NOT in Phase 4

These will be in later phases:

- ❌ Bid filtering (Phase 5 - ASQL)
- ❌ Payment flow (Phase 6)
- ❌ Email notifications (Phase 6)
- ❌ Retry logic (Phase 6)
- ❌ Configurable thresholds (Phase 7)

---

Ready to implement Phase 4!

# Phase 3 Clarifications

## 1. Filtering Removed (For Now)

**Question:** Why remove filtering if it was working?

**Answer:** You're right! We'll implement **ASQL (Auction Search Query Language)** parser in Phase 5 (Milestone 3). The basic filtering was just temporary.

**Current State:**

```csharp
public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(...)
{
    var products = await _productRepository.GetAllProductsAsync();
    // TODO: Implement ASQL filtering in Phase 5 (Milestone 3)
    return _mapper.Map<IEnumerable<ProductDto>>(products);
}
```

**Phase 5 Will Add:**

- ASQL parser service
- Support for: `category="Electronics" AND price>1000`
- Support for: `category in ["Electronics", "Art"]`
- Support for: `status="Active" OR status="Expired"`

---

## 2. Expiry Time - How It Works

**Question:** How does expiry time update? Is it a special function or attribute?

**Answer:** **No special attribute!** It's manual code logic. Here's the timeline:

### Phase 3 (Current) - Set Once

```csharp
// When auction is created
var auction = new Auction
{
    StartTime = DateTime.UtcNow,
    ExpiryTime = DateTime.UtcNow.AddMinutes(durationMinutes), // Set once
    Status = AuctionStatus.Active
};
```

**Expiry time is STATIC** - doesn't change after creation (yet).

### Phase 4 (Next) - Anti-Sniping Updates

```csharp
// When bid is placed
public async Task PlaceBidAsync(PlaceBidDto bidDto)
{
    var auction = await _auctionRepository.GetByIdAsync(bidDto.AuctionId);

    // Check if bid is within last minute
    var timeRemaining = auction.ExpiryTime - DateTime.UtcNow;
    if (timeRemaining < TimeSpan.FromSeconds(60))
    {
        // MANUALLY UPDATE expiry time
        auction.ExpiryTime = auction.ExpiryTime.AddMinutes(1);
        auction.ExtensionCount++;
        await _auctionRepository.UpdateAsync(auction); // Save to DB
    }

    // Save bid...
}
```

**Expiry time is DYNAMIC** - manually updated when needed.

### Display Logic (Already Working)

```csharp
// In ProductMapper
RemainingTimeMinutes = (int?)(src.Auction.ExpiryTime - DateTime.UtcNow).TotalMinutes
```

This **calculates** remaining time on-the-fly for display. It doesn't update the database.

---

## Summary

### Expiry Time Lifecycle:

1. **Creation (Phase 3):**

   - Set: `ExpiryTime = StartTime + Duration`
   - Stored in database
   - Static value

2. **Anti-Sniping (Phase 4):**

   - Check: Is bid within last minute?
   - Update: `ExpiryTime = ExpiryTime + 1 minute`
   - Save to database
   - Can happen multiple times

3. **Display (Phase 3 & 4):**

   - Calculate: `RemainingTime = ExpiryTime - Now`
   - Shown to users
   - Not stored, just calculated

4. **Expiry Check (Phase 4 - Background Service):**
   - Check: Is `ExpiryTime < Now`?
   - Update: `Status = Expired`
   - Trigger payment flow

---

## No Magic Attributes!

**Common misconception:** Some frameworks have attributes like `[AutoUpdate]` or `[Computed]`.

**Reality in our code:**

- ✅ We manually set `ExpiryTime` when creating auction
- ✅ We manually update `ExpiryTime` when extending (Phase 4)
- ✅ We manually check `ExpiryTime` in background service (Phase 4)
- ❌ No automatic updates
- ❌ No special attributes
- ❌ No database triggers

**It's all explicit code!** This makes it:

- Easy to understand
- Easy to debug
- Easy to test
- Predictable behavior

---

## Phase 4 Preview

In Phase 4, we'll add:

1. **Bid Placement Logic:**

   - Validate bid amount
   - Check auction status
   - **Check time remaining → Extend if needed**
   - Update highest bid
   - Save bid

2. **Background Service:**

   - Run every 10 seconds
   - Find auctions where `ExpiryTime < Now`
   - Update status to Expired
   - Trigger payment flow

3. **Anti-Sniping:**
   - If bid within last 60 seconds
   - Extend by 60 seconds
   - Increment extension count
   - Update auction in database

All manual, all explicit, all simple!

# Dashboard Setup Guide

## 🎯 What Was Created

### Backend API

- **File:** `BidSphere/Controllers/DashboardController.cs`
- **Endpoint:** `GET /api/dashboard`
- **Access:** Public (no authentication required)

### Frontend Dashboard

- **File:** `frontend/index.html`
- **Type:** Single-page vanilla JavaScript dashboard
- **Auto-refresh:** Every 10 seconds

---

## 🚀 Quick Start (3 Steps)

### Step 1: Start BidSphere API

```bash
cd BidSphere
dotnet run
```

**Verify API is running:**

```bash
curl http://localhost:8080/api/dashboard
```

### Step 2: Open Dashboard

**Option A: Direct (Easiest)**

```bash
# Just open the HTML file in your browser
open frontend/index.html
```

**Option B: With Python Server**

```bash
cd frontend
python3 -m http.server 3000
# Open http://localhost:3000 in browser
```

**Option C: With Node.js**

```bash
cd frontend
npx http-server -p 3000
# Open http://localhost:3000 in browser
```

### Step 3: Watch It Update!

The dashboard will:

- ✅ Load data immediately
- ✅ Auto-refresh every 10 seconds
- ✅ Show real-time metrics

---

## 📊 Dashboard Features

### Metrics Displayed

1. **Auction Counts**

   - Active auctions
   - Expired auctions
   - Completed auctions
   - Failed auctions

2. **Payment Metrics**

   - Pending payments
   - Successful payments
   - Failed payments
   - Payment success rate (%)

3. **Financial**

   - Total revenue

4. **Top Bidders**

   - Top 5 bidders by activity
   - Shows: Email, Total Bids, Total Amount, Average Bid

5. **Recent Auctions**
   - Last 5 auctions
   - Shows: ID, Product, Status, Highest Bid, Bid Count, Expiry

---

## 🎨 Dashboard Preview

```
┌─────────────────────────────────────────────┐
│         🎯 BidSphere Dashboard              │
│      Real-time Auction System Metrics       │
└─────────────────────────────────────────────┘

Active: 10  |  Pending: 2  |  Completed: 7  |  Failed: 1
Revenue: $1,250  |  Success Rate: 85% ████████░░

🏆 Top Bidders
┌────────────────────────────────────────────┐
│ user@example.com    15 bids    $1,500     │
│ user2@example.com   12 bids    $1,200     │
└────────────────────────────────────────────┘

📊 Recent Auctions
┌────────────────────────────────────────────┐
│ #5  Vintage Watch   Active    $200   3    │
│ #4  iPhone 15       Complete  $500   8    │
└────────────────────────────────────────────┘
```

---

## 🔧 API Endpoint Details

### GET /api/dashboard

**URL:** `http://localhost:8080/api/dashboard`

**Response Example:**

```json
{
  "activeCount": 10,
  "expiredCount": 3,
  "completedCount": 7,
  "failedCount": 1,
  "totalAuctions": 21,
  "pendingPayment": 2,
  "successfulPayments": 15,
  "failedPayments": 3,
  "paymentSuccessRate": 83.33,
  "totalRevenue": 1250.5,
  "topBidders": [
    {
      "bidderId": 1,
      "bidderEmail": "user@example.com",
      "totalBids": 15,
      "totalAmount": 1500.0,
      "averageBid": 100.0
    }
  ],
  "recentAuctions": [
    {
      "auctionId": 5,
      "productName": "Vintage Watch",
      "status": "Active",
      "startTime": "2024-11-26T10:00:00Z",
      "expiryTime": "2024-11-26T11:00:00Z",
      "highestBid": 200.0,
      "bidCount": 3
    }
  ],
  "lastUpdated": "2024-11-26T10:30:00Z"
}
```

---

## 🧪 Testing the Dashboard

### Test 1: Verify API Works

```bash
# Test the endpoint
curl http://localhost:8080/api/dashboard | jq

# Should return JSON with metrics
```

### Test 2: Open Dashboard

```bash
# Open in browser
open frontend/index.html

# Should see:
# - Metrics cards with numbers
# - Top bidders table
# - Recent auctions table
# - "Last updated" timestamp
```

### Test 3: Verify Auto-Refresh

1. Open dashboard
2. Watch "Last updated" time
3. Should update every 10 seconds
4. Metrics should refresh automatically

### Test 4: Create Test Data

```bash
# Create an auction
curl -X POST http://localhost:8080/api/products \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Product",
    "startingPrice": 100,
    "description": "Test",
    "category": "Electronics",
    "auctionDurationMinutes": 60
  }'

# Place a bid
curl -X POST http://localhost:8080/api/bids \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "auctionId": 1,
    "amount": 150
  }'

# Refresh dashboard - should see new data!
```

---

## 🎨 Customization

### Change Refresh Interval

Edit `frontend/index.html` line 268:

```javascript
const REFRESH_INTERVAL = 15000; // 15 seconds instead of 10
```

### Change API URL

Edit `frontend/index.html` line 267:

```javascript
const API_URL = "http://your-server:8080/api/dashboard";
```

### Change Colors

Edit the CSS in `frontend/index.html`:

```css
/* Change background gradient */
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);

/* Change metric colors */
.metric-card.active .value {
  color: #4caf50;
}
```

---

## 🐛 Troubleshooting

### Issue 1: Dashboard shows "Failed to fetch dashboard data"

**Solution:**

```bash
# 1. Check if API is running
curl http://localhost:8080/api/dashboard

# 2. Check CORS is enabled (should be by default)
# 3. Check browser console for errors (F12)
```

### Issue 2: Dashboard shows all zeros

**Solution:**

```bash
# Check if database has data
# Create some test auctions and bids
# Refresh dashboard
```

### Issue 3: Dashboard not auto-refreshing

**Solution:**

- Check browser console for JavaScript errors
- Verify "Last updated" time is changing
- Try refreshing the page (F5)

### Issue 4: CORS Error

**Solution:**
CORS should already be enabled in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ...

app.UseCors("AllowAll");
```

---

## 📱 Mobile Access

The dashboard is responsive! Access from:

- Desktop browser
- Tablet
- Mobile phone

Just open `http://your-ip:3000` (if using Python/Node server)

---

## 🔒 Security Notes

**Current Setup:**

- ✅ Dashboard is public (no auth)
- ✅ Good for development/testing
- ❌ NOT suitable for production as-is

**For Production:**

1. Add `[Authorize(Roles = "Admin")]` to DashboardController
2. Implement JWT authentication in frontend
3. Use HTTPS
4. Add rate limiting

---

## 📊 What Metrics Mean

### Active Count

- Auctions currently accepting bids
- Status = Active

### Pending Payment

- Payments waiting for confirmation
- Status = Pending

### Completed Count

- Auctions successfully completed with payment
- Status = Completed

### Failed Count

- Auctions that failed (no payment after retries)
- Status = Failed

### Payment Success Rate

- (Successful Payments / Total Payments) × 100
- Higher is better!

### Total Revenue

- Sum of all confirmed payments
- Only includes successful payments

---

## ✅ Verification Checklist

Before using dashboard:

- [ ] BidSphere API running on port 8080
- [ ] Can access http://localhost:8080/api/dashboard
- [ ] CORS enabled in Program.cs
- [ ] Database has some test data
- [ ] Browser supports modern JavaScript
- [ ] Dashboard opens without errors
- [ ] Metrics display correctly
- [ ] Auto-refresh works (watch timestamp)

---

## 🎯 Next Steps

1. **Start the API** - `dotnet run` in BidSphere folder
2. **Open Dashboard** - Open `frontend/index.html` in browser
3. **Create Test Data** - Create auctions and bids
4. **Watch It Update** - Dashboard refreshes every 10 seconds

---

## 📝 Files Created

```
BidSphere/
└── Controllers/
    └── DashboardController.cs    # API endpoint

frontend/
├── index.html                    # Dashboard page
└── README.md                     # Detailed docs
```

---

## 🎉 You're Done!

The dashboard is ready to use. Just:

1. Start the API
2. Open the HTML file
3. Watch your metrics in real-time!

Enjoy your new dashboard! 📊✨

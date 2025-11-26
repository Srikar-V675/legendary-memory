# Runtime Configuration API Guide

## 🎯 Purpose

The Config API allows admins to update auction configuration at runtime without restarting the application.

---

## 📋 How It Works

### Configuration Flow:

```
1. App Starts
   ↓
2. Program.cs loads values from appsettings.json
   ↓
3. Values stored in AuctionConfig static class
   ↓
4. Background services use AuctionConfig
   ↓
5. Admin calls PUT /api/config (runtime update)
   ↓
6. AuctionConfig values updated immediately
   ↓
7. All services use new values instantly
   ↓
8. App Restart → Values reset to appsettings.json
```

### Why This Design?

**appsettings.json:**

- Stores default/initial values
- Loaded once on startup
- Version controlled

**AuctionConfig (static class):**

- Runtime values used by all services
- Can be updated without restart
- Fast access (no DI overhead)

**Config API:**

- Allows runtime updates
- Admin only
- Changes apply immediately
- Doesn't persist (resets on restart)

---

## 📡 API Endpoints

### 1. Get Current Configuration

```http
GET /api/config
Authorization: Bearer {admin-token}
```

**Response:**

```json
{
  "config": {
    "antiSnipingThresholdSeconds": 60,
    "extensionDurationSeconds": 60,
    "paymentTimeoutSeconds": 60,
    "maxPaymentAttempts": 3
  },
  "message": "Current auction configuration. Changes apply immediately but reset on app restart.",
  "lastUpdated": "2024-11-26T10:00:00Z"
}
```

---

### 2. Update Configuration

```http
PUT /api/config
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "antiSnipingThresholdSeconds": 30,
  "extensionDurationSeconds": 120,
  "paymentTimeoutSeconds": 90,
  "maxPaymentAttempts": 5
}
```

**Response:**

```json
{
  "message": "Configuration updated successfully. Changes applied immediately.",
  "oldConfig": {
    "antiSnipingThresholdSeconds": 60,
    "extensionDurationSeconds": 60,
    "paymentTimeoutSeconds": 60,
    "maxPaymentAttempts": 3
  },
  "newConfig": {
    "antiSnipingThresholdSeconds": 30,
    "extensionDurationSeconds": 120,
    "paymentTimeoutSeconds": 90,
    "maxPaymentAttempts": 5
  },
  "warning": "Configuration will reset to appsettings.json values on app restart.",
  "updatedAt": "2024-11-26T10:05:00Z"
}
```

**Validations:**

- `antiSnipingThresholdSeconds`: 10-300 seconds
- `extensionDurationSeconds`: 10-600 seconds
- `paymentTimeoutSeconds`: 30-300 seconds
- `maxPaymentAttempts`: 1-5 attempts

---

### 3. Reset to Defaults

```http
POST /api/config/reset
Authorization: Bearer {admin-token}
```

**Response:**

```json
{
  "message": "Configuration reset to default values from appsettings.json",
  "oldConfig": {
    "antiSnipingThresholdSeconds": 30,
    "extensionDurationSeconds": 120,
    "paymentTimeoutSeconds": 90,
    "maxPaymentAttempts": 5
  },
  "newConfig": {
    "antiSnipingThresholdSeconds": 60,
    "extensionDurationSeconds": 60,
    "paymentTimeoutSeconds": 60,
    "maxPaymentAttempts": 3
  },
  "resetAt": "2024-11-26T10:10:00Z"
}
```

---

## 🧪 Testing

### Test 1: Get Current Config

```bash
# Login as admin
POST /api/auth/login
{
  "email": "admin@bidsphere.com",
  "password": "Admin@123"
}

# Get config
GET /api/config
Authorization: Bearer {admin-token}
```

**Expected:** Current configuration values

---

### Test 2: Update Config

```bash
# Update config
PUT /api/config
Authorization: Bearer {admin-token}
{
  "antiSnipingThresholdSeconds": 30,
  "extensionDurationSeconds": 120,
  "paymentTimeoutSeconds": 90,
  "maxPaymentAttempts": 5
}
```

**Expected:** Success message with old and new values

---

### Test 3: Verify Changes Apply Immediately

```bash
# Create auction with 2-minute duration
POST /api/products
{
  "name": "Test Product",
  "startingPrice": 100,
  "auctionDurationMinutes": 2
}

# Place bid within last 30 seconds (new threshold)
# Should extend auction by 120 seconds (new extension)
```

**Expected:** Auction extends by 120 seconds (not 60)

---

### Test 4: Invalid Values

```bash
PUT /api/config
{
  "antiSnipingThresholdSeconds": 5,  // Too low (min 10)
  "paymentTimeoutSeconds": 400       // Too high (max 300)
}
```

**Expected:** 400 Bad Request with validation errors

---

### Test 5: Reset to Defaults

```bash
POST /api/config/reset
Authorization: Bearer {admin-token}
```

**Expected:** Values reset to appsettings.json defaults

---

### Test 6: Non-Admin Access

```bash
# Login as regular user
POST /api/auth/login
{
  "email": "user1@bidsphere.com",
  "password": "User@123"
}

# Try to get config
GET /api/config
Authorization: Bearer {user-token}
```

**Expected:** 403 Forbidden

---

## 🔍 Configuration Parameters Explained

### AntiSnipingThresholdSeconds

**What:** Time before auction end when bids trigger extension
**Default:** 60 seconds
**Range:** 10-300 seconds
**Example:** If set to 30, bids in last 30 seconds extend auction

### ExtensionDurationSeconds

**What:** How much time is added when anti-sniping triggers
**Default:** 60 seconds
**Range:** 10-600 seconds
**Example:** If set to 120, auction extends by 2 minutes

### PaymentTimeoutSeconds

**What:** How long winner has to confirm payment
**Default:** 60 seconds
**Range:** 30-300 seconds
**Example:** If set to 90, winner has 90 seconds to pay

### MaxPaymentAttempts

**What:** Maximum retry attempts for payment
**Default:** 3 attempts
**Range:** 1-5 attempts
**Example:** If set to 5, system tries 5 bidders before marking FAILED

---

## 📊 Use Cases

### Use Case 1: Reduce Anti-Sniping Window

**Scenario:** Too many auction extensions
**Solution:**

```json
{
  "antiSnipingThresholdSeconds": 30,
  "extensionDurationSeconds": 30
}
```

### Use Case 2: Give More Payment Time

**Scenario:** Winners timing out too often
**Solution:**

```json
{
  "paymentTimeoutSeconds": 120
}
```

### Use Case 3: More Retry Attempts

**Scenario:** Many auctions failing after 3 attempts
**Solution:**

```json
{
  "maxPaymentAttempts": 5
}
```

### Use Case 4: Quick Testing

**Scenario:** Testing payment flow quickly
**Solution:**

```json
{
  "paymentTimeoutSeconds": 30,
  "maxPaymentAttempts": 2
}
```

---

## ⚠️ Important Notes

### Changes Apply Immediately

- No restart required
- All background services use new values instantly
- Active auctions continue with old rules
- New auctions use new rules

### Changes Don't Persist

- Values reset on app restart
- To make permanent: update appsettings.json
- Runtime changes are for testing/temporary adjustments

### Logging

All configuration changes are logged:

```
Auction configuration updated.
Old: { antiSnipingThresholdSeconds: 60, ... }
New: { antiSnipingThresholdSeconds: 30, ... }
```

---

## 🚨 Validation Rules

| Parameter                   | Min | Max | Default |
| --------------------------- | --- | --- | ------- |
| AntiSnipingThresholdSeconds | 10  | 300 | 60      |
| ExtensionDurationSeconds    | 10  | 600 | 60      |
| PaymentTimeoutSeconds       | 30  | 300 | 60      |
| MaxPaymentAttempts          | 1   | 5   | 3       |

**Why these limits?**

- **Min values:** Prevent system overload
- **Max values:** Prevent unreasonable delays
- **Defaults:** Balanced for typical use

---

## 🎯 Best Practices

### 1. Test Changes in Development

```bash
# Update config
PUT /api/config { ... }

# Test with short auction
POST /api/products { "auctionDurationMinutes": 2 }

# Verify behavior
# Reset if needed
POST /api/config/reset
```

### 2. Monitor Logs

```bash
# Watch for config changes
tail -f logs/app.log | grep "configuration updated"
```

### 3. Document Production Changes

Keep track of runtime changes for troubleshooting

### 4. Reset After Testing

```bash
POST /api/config/reset
```

---

## ✅ Summary

**What We Have:**

- ✅ appsettings.json - Default values
- ✅ AuctionConfig - Runtime values
- ✅ Config API - Runtime updates (Admin only)
- ✅ Validation - Prevents invalid values
- ✅ Logging - Tracks all changes

**Benefits:**

- No restart required for config changes
- Admin can adjust behavior on the fly
- Safe validation prevents bad values
- Changes apply immediately to all services

**Limitations:**

- Changes don't persist (reset on restart)
- To persist: update appsettings.json manually

---

## 📝 Files Created

1. `Models/Dtos/Config/AuctionConfigDto.cs` - DTO
2. `Validators/AuctionConfigDtoValidator.cs` - Validation
3. `Controllers/ConfigController.cs` - API endpoints

---

## 🎯 Ready to Use!

The Config API is now available at:

- GET /api/config
- PUT /api/config
- POST /api/config/reset

All endpoints require Admin role.

# Quick Start Testing Guide

## 🚀 Get Started in 5 Minutes

### Step 1: Update Email Config (Optional)

If you want to test WITH email, update `appsettings.Development.json`:

```json
"EmailSettings": {
  "Username": "YOUR_MAILTRAP_USERNAME",
  "Password": "YOUR_MAILTRAP_PASSWORD"
}
```

If you want to test WITHOUT email, leave it as is (invalid credentials).

### Step 2: Create Migration

```bash
dotnet ef migrations add "AddPaymentAttempts"
dotnet ef database update
```

### Step 3: Run Application

```bash
dotnet run
```

### Step 4: Open Swagger

```
http://localhost:8080/swagger
```

---

## 🧪 Quick Test: Payment Flow

### 1. Login as Admin

```http
POST /api/auth/login
{
  "email": "admin@bidsphere.com",
  "password": "Admin@123"
}
```

**Copy the token!**

### 2. Create Short Auction

```http
POST /api/products
Authorization: Bearer {admin-token}
{
  "name": "Test iPhone",
  "description": "Testing payment flow",
  "category": "Electronics",
  "startingPrice": 100,
  "auctionDurationMinutes": 2
}
```

### 3. Login as User 1

```http
POST /api/auth/login
{
  "email": "user1@bidsphere.com",
  "password": "User@123"
}
```

### 4. Place Bid

```http
POST /api/bids
Authorization: Bearer {user1-token}
{
  "auctionId": 1,
  "amount": 150
}
```

### 5. Wait 2+ Minutes

Watch the logs for:

```
Auction 1 marked as EXPIRED
Created payment attempt #1 for auction 1
```

### 6. Check Transaction

```http
GET /api/transactions
Authorization: Bearer {user1-token}
```

**Expected:**

```json
[
  {
    "paymentId": 1,
    "status": "Pending",
    "attemptNumber": 1
  }
]
```

### 7. Confirm Payment

```http
PUT /api/products/1/confirm
Authorization: Bearer {user1-token}
{
  "confirmedAmount": 150
}
```

**Expected:**

```json
{
  "message": "Payment confirmed successfully",
  "auctionStatus": "Completed"
}
```

### 8. Verify Auction Completed

```http
GET /api/products/1
```

**Expected:** `status: "Completed"`

---

## ✅ Success!

If all steps worked, your payment flow is working correctly!

---

## 🧪 Quick Test: Retry Queue

### 1-4. Same as above

### 5. Wait 2+ Minutes (Auction Expires)

### 6. DON'T Confirm Payment

### 7. Wait 60+ Seconds

### 8. Check Logs

```
Processing timed out payment 1
Created payment attempt #2
```

### 9. Check Transactions

```http
GET /api/transactions
Authorization: Bearer {admin-token}
```

**Expected:**

```json
[
  {
    "paymentId": 1,
    "status": "Failed",
    "attemptNumber": 1
  },
  {
    "paymentId": 2,
    "status": "Pending",
    "attemptNumber": 2
  }
]
```

---

## ✅ Retry Queue Works!

---

## 🚨 Common Issues

### Issue: Migration Fails

```bash
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add "Initial"
dotnet ef database update
```

### Issue: No Logs Showing

Check console output - background services log every 10 seconds

### Issue: Payment Not Created

Check that auction actually expired (wait full duration + 10 seconds)

---

## 📚 Full Documentation

- `PAYMENT_TESTING_GUIDE.md` - Complete testing guide
- `MILESTONE4_VALIDATION.md` - Validation checklist
- `MILESTONE4_FINAL_STATUS.md` - Complete status report

---

## ✅ You're Ready!

Milestone 4 is complete and ready for production testing!

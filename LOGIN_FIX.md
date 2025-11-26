# Login Issue - Quick Fix

## ❌ Your Current Request (WRONG)

```bash
curl -X 'POST' \
  'http://localhost:8080/login' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "admin@bidservice.com",
  "password": "Admin@123"
}'
```

**Problems:**

1. ❌ Wrong email: `admin@bidservice.com` (should be `admin@bidsphere.com`)
2. ❌ Wrong endpoint: `/login` (should be `/login?useCookies=false`)

---

## ✅ Correct Request

### Option 1: Using Identity API Endpoint (JWT)

```bash
curl -X 'POST' \
  'http://localhost:8080/login?useCookies=false' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "admin@bidsphere.com",
  "password": "Admin@123"
}'
```

**Response:**

```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "refreshToken": "..."
}
```

---

### Option 2: Using Custom Auth Controller (Recommended)

This is cleaner and returns just the token:

```bash
curl -X 'POST' \
  'http://localhost:8080/api/auth/login' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "admin@bidsphere.com",
  "password": "Admin@123"
}'
```

**Response:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "admin@bidsphere.com",
  "role": "Admin"
}
```

---

## 🔍 Why It Failed

### Issue 1: Wrong Email

- You used: `admin@bidservice.com`
- Correct: `admin@bidsphere.com`
- The seeded admin user has email `admin@bidsphere.com`

### Issue 2: Wrong Endpoint

- You used: `/login`
- This is the Identity API endpoint that expects cookies by default
- For JWT, you need: `/login?useCookies=false`
- Or use custom endpoint: `/api/auth/login`

---

## 📋 Available Endpoints

### Identity API Endpoints (Built-in)

```
POST /register
POST /login?useCookies=false
POST /refresh
GET  /confirmEmail
POST /resendConfirmationEmail
POST /forgotPassword
POST /resetPassword
POST /manage/2fa
GET  /manage/info
POST /manage/info
```

### Custom Auth Endpoints (Recommended)

```
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/profile
PUT  /api/auth/profile
```

---

## ✅ Correct Login Flow

### Step 1: Login

```bash
curl -X 'POST' \
  'http://localhost:8080/api/auth/login' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "admin@bidsphere.com",
  "password": "Admin@123"
}'
```

### Step 2: Copy Token

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJhZG1pbkBiaWRzcGhlcmUuY29tIiwicm9sZSI6IkFkbWluIiwibmJmIjoxNzAwMDAwMDAwLCJleHAiOjE3MDAwMDM2MDAsImlhdCI6MTcwMDAwMDAwMH0.xxx"
}
```

### Step 3: Use Token

```bash
curl -X 'GET' \
  'http://localhost:8080/api/products' \
  -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'
```

---

## 🧪 Test All Users

### Admin User

```bash
curl -X 'POST' \
  'http://localhost:8080/api/auth/login' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "admin@bidsphere.com",
  "password": "Admin@123"
}'
```

### User 1

```bash
curl -X 'POST' \
  'http://localhost:8080/api/auth/login' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "user1@bidsphere.com",
  "password": "User@123"
}'
```

### User 2

```bash
curl -X 'POST' \
  'http://localhost:8080/api/auth/login' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "user2@bidsphere.com",
  "password": "User@123"
}'
```

---

## 🔍 Debugging

### Check if users exist:

```bash
# Try to register (should fail if user exists)
curl -X 'POST' \
  'http://localhost:8080/api/auth/register' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "admin@bidsphere.com",
  "password": "Admin@123",
  "confirmPassword": "Admin@123"
}'
```

If you get "Email already exists" - users are seeded correctly!

---

## 🚨 Common Mistakes

### Mistake 1: Wrong Email Domain

```bash
❌ admin@bidservice.com
✅ admin@bidsphere.com
```

### Mistake 2: Missing Query Parameter

```bash
❌ POST /login
✅ POST /login?useCookies=false
✅ POST /api/auth/login (better)
```

### Mistake 3: Wrong Password

```bash
❌ admin@123
✅ Admin@123 (capital A)
```

### Mistake 4: Using Cookies Instead of JWT

```bash
❌ POST /login (returns cookies)
✅ POST /login?useCookies=false (returns JWT)
✅ POST /api/auth/login (returns JWT)
```

---

## ✅ Quick Fix

**Just change your curl command to:**

```bash
curl -X 'POST' \
  'http://localhost:8080/api/auth/login' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "admin@bidsphere.com",
  "password": "Admin@123"
}'
```

**That's it!** 🎉

# Authentication Endpoints Guide

## Overview

Using **ASP.NET Core Identity API Endpoints** with custom wrapper endpoints for our requirements.

---

## Available Endpoints

### 1. Register (Custom)

**Endpoint:** `POST /api/auth/register`
**Description:** Register new user with custom Role and CreatedAt fields
**Auth Required:** No

**Request:**

```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:**

```json
{
  "message": "User registered successfully. Please login to get your token."
}
```

---

### 2. Login (Identity Built-in)

**Endpoint:** `POST /login?useCookies=false`
**Description:** Login and get JWT token (provided by Identity)
**Auth Required:** No

**Request:**

```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:**

```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "refreshToken": "CfDJ8..."
}
```

**Note:** Add `?useCookies=false` to get JWT token instead of cookies

---

### 3. Get Profile (Custom)

**Endpoint:** `GET /api/auth/profile`
**Description:** Get current user profile
**Auth Required:** Yes (Bearer token)

**Headers:**

```
Authorization: Bearer {your-access-token}
```

**Response:**

```json
{
  "userId": 1,
  "email": "user@example.com",
  "role": "User",
  "createdAt": "2025-11-26T10:30:00Z"
}
```

---

### 4. Update Profile (Custom)

**Endpoint:** `PUT /api/auth/profile`
**Description:** Update user email
**Auth Required:** Yes (Bearer token)

**Headers:**

```
Authorization: Bearer {your-access-token}
```

**Request:**

```json
{
  "email": "newemail@example.com"
}
```

**Response:**

```json
{
  "userId": 1,
  "email": "newemail@example.com",
  "role": "User",
  "createdAt": "2025-11-26T10:30:00Z"
}
```

---

## Additional Identity Endpoints (Built-in)

These are automatically available via `MapIdentityApi<User>()`:

### Refresh Token

**Endpoint:** `POST /refresh`
**Description:** Refresh access token using refresh token

### Manage Info

**Endpoint:** `GET /manage/info`
**Description:** Get user info (alternative to our /api/auth/profile)

**Endpoint:** `POST /manage/info`
**Description:** Update user info (alternative to our /api/auth/profile)

### Two-Factor Authentication

**Endpoint:** `POST /manage/2fa`
**Description:** Manage 2FA settings

### Password Management

**Endpoint:** `POST /forgotPassword`
**Endpoint:** `POST /resetPassword`

### Email Confirmation

**Endpoint:** `GET /confirmEmail`
**Endpoint:** `POST /resendConfirmationEmail`

---

## Testing Flow in Swagger

### Step 1: Register

```http
POST /api/auth/register
{
  "email": "test@example.com",
  "password": "test123"
}
```

### Step 2: Login

```http
POST /login?useCookies=false
{
  "email": "test@example.com",
  "password": "test123"
}
```

Copy the `accessToken` from response.

### Step 3: Authorize in Swagger

1. Click "Authorize" button at top
2. Enter: `Bearer {paste-your-access-token}`
3. Click "Authorize"

### Step 4: Access Protected Endpoints

```http
GET /api/auth/profile
(Token automatically included)
```

---

## Password Requirements

- Minimum 6 characters
- At least one digit
- At least one lowercase letter
- No uppercase required
- No special characters required

---

## How It Works

1. **Identity API Endpoints** (`MapIdentityApi<User>()`) provides:

   - `/login` - Returns JWT token
   - `/refresh` - Refresh token
   - `/manage/*` - User management endpoints

2. **Custom AuthController** provides:

   - `/api/auth/register` - Sets Role=User and CreatedAt
   - `/api/auth/profile` - Returns our UserDto format
   - `/api/auth/profile` (PUT) - Updates email

3. **JWT Authentication** is handled automatically by Identity
   - No manual token generation needed
   - No custom JwtHelper needed
   - Works out of the box!

---

## Key Benefits

✅ Less code (no AuthService, no manual JWT generation)
✅ Identity handles password hashing automatically
✅ JWT tokens work out of the box
✅ Refresh tokens included
✅ Custom endpoints for our specific requirements
✅ Follows document specifications

---

## Next Steps

Ready to add authorization to Product endpoints:

- `[Authorize]` - Requires any authenticated user
- `[Authorize(Roles = "Admin")]` - Requires Admin role
- `[Authorize(Roles = "User")]` - Requires User role

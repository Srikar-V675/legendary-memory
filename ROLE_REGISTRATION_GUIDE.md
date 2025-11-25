# Role Registration & Update Guide

## Overview

You can now specify roles during registration and update them in profile. This is useful for testing and development.

---

## Register with Different Roles

### Register as User (Default)

```json
POST /api/auth/register
{
  "email": "user@example.com",
  "password": "test123"
}
```

Role defaults to "User" if not specified.

### Register as Admin

```json
POST /api/auth/register
{
  "email": "admin@example.com",
  "password": "admin123",
  "role": "Admin"
}
```

### Register as Guest

```json
POST /api/auth/register
{
  "email": "guest@example.com",
  "password": "guest123",
  "role": "Guest"
}
```

**Valid Roles:** Admin, User, Guest (case-insensitive)

---

## Update User Role

### Change Role via Profile Update

```json
PUT /api/auth/profile
Authorization: Bearer {your-token}

{
  "email": "user@example.com",
  "role": "Admin"
}
```

**What happens:**

1. Removes user from old Identity role
2. Updates `User.Role` enum property
3. Adds user to new Identity role
4. Both systems stay synchronized

---

## Testing Scenarios

### Scenario 1: Create Admin User

```bash
# 1. Register as Admin
POST /api/auth/register
{
  "email": "admin@test.com",
  "password": "admin123",
  "role": "Admin"
}

# 2. Login
POST /login?useCookies=false
{
  "email": "admin@test.com",
  "password": "admin123"
}

# 3. Use token to access admin endpoints
GET /api/products [Authorize(Roles = "Admin")]
Authorization: Bearer {token}
```

### Scenario 2: Promote User to Admin

```bash
# 1. Register as User
POST /api/auth/register
{
  "email": "user@test.com",
  "password": "user123"
}

# 2. Login and get token
POST /login?useCookies=false
{
  "email": "user@test.com",
  "password": "user123"
}

# 3. Update role to Admin
PUT /api/auth/profile
Authorization: Bearer {old-token}
{
  "email": "user@test.com",
  "role": "Admin"
}

# 4. Login again to get new token with Admin role
POST /login?useCookies=false
{
  "email": "user@test.com",
  "password": "user123"
}

# 5. New token has Admin role claim
```

### Scenario 3: Create Multiple Test Users

```bash
# Admin
POST /api/auth/register
{ "email": "admin@test.com", "password": "test123", "role": "Admin" }

# Regular User
POST /api/auth/register
{ "email": "user1@test.com", "password": "test123", "role": "User" }

# Another User
POST /api/auth/register
{ "email": "user2@test.com", "password": "test123", "role": "User" }

# Guest
POST /api/auth/register
{ "email": "guest@test.com", "password": "test123", "role": "Guest" }
```

---

## Important Notes

### 1. Role Changes Require Re-login

When you update a user's role, the JWT token still has the old role claim. You need to login again to get a new token with the updated role.

**Example:**

```bash
# User has "User" role, gets token
POST /login → Token has role="User"

# Update role to "Admin"
PUT /api/auth/profile { "role": "Admin" }

# Old token still has role="User"
# Need to login again to get new token with role="Admin"
POST /login → New token has role="Admin"
```

### 2. Both Systems Stay Synchronized

- `User.Role` enum property updated
- Identity role updated (removed from old, added to new)
- JWT claims updated on next login

### 3. Validation

Invalid roles return error:

```json
{
  "message": "Invalid role. Valid roles: Admin, User, Guest"
}
```

---

## Production Considerations

**⚠️ Security Warning:**
In production, you should NOT allow users to set their own roles!

**Better approach for production:**

1. Remove `role` parameter from RegisterDto
2. Always default to "User" role
3. Create separate admin endpoint to promote users:
   ```csharp
   [Authorize(Roles = "Admin")]
   [HttpPut("users/{id}/role")]
   public async Task<ActionResult> UpdateUserRole(int id, string role)
   ```

**For now (development/testing):**

- ✅ Keep role parameter for easy testing
- ✅ Can create admin users quickly
- ✅ Can test role-based authorization

---

## Quick Test Commands

### Create Admin User

```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@test.com","password":"admin123","role":"Admin"}'
```

### Login as Admin

```bash
curl -X POST "http://localhost:8080/login?useCookies=false" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@test.com","password":"admin123"}'
```

### Get Profile

```bash
curl -X GET http://localhost:8080/api/auth/profile \
  -H "Authorization: Bearer {your-token}"
```

### Update Role

```bash
curl -X PUT http://localhost:8080/api/auth/profile \
  -H "Authorization: Bearer {your-token}" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@test.com","role":"User"}'
```

---

## Summary

✅ **Register with role** - Optional `role` parameter in RegisterDto
✅ **Update role** - Include `role` in UserDto for PUT /api/auth/profile
✅ **Synchronized** - Both User.Role enum and Identity roles updated
✅ **Validated** - Invalid roles return error
✅ **Easy testing** - Can create Admin users for testing

**Next:** Use these admin users to test role-based authorization on Product endpoints!

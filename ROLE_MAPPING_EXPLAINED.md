# Role Mapping Explanation

## The Problem You Asked About

**Question:** Are the roles used in Identity configured to the custom roles we needed? Are they mapped?

**Answer:** NOW they are! Here's what we fixed:

---

## Before the Fix ❌

### Two Separate Role Systems (NOT CONNECTED):

1. **User.Role property** (UserRole enum)

   - Stored on User entity
   - Values: Admin, User, Guest
   - Just a property, not used by Identity

2. **Identity Roles** (IdentityRole<int>)
   - Separate table in database
   - Used by `[Authorize(Roles = "Admin")]`
   - NOT connected to User.Role property

**Problem:** When you used `[Authorize(Roles = "Admin")]`, it checked Identity's role table, which was empty! Your User.Role property was ignored.

---

## After the Fix ✅

### Synchronized Role Systems:

1. **User.Role property** (UserRole enum)

   - Still stored on User entity
   - Easy to access: `user.Role`
   - Used for display and logic

2. **Identity Roles** (IdentityRole<int>)
   - Seeded on app startup (Admin, User, Guest)
   - User is added to Identity role during registration
   - Used by `[Authorize(Roles = "Admin")]`

**Solution:** Both systems stay in sync!

---

## What We Changed

### 1. Seed Identity Roles on Startup (Program.cs)

```csharp
// Seed Identity Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

    string[] roles = { "Admin", "User", "Guest" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
        }
    }
}
```

**What this does:**

- Creates "Admin", "User", "Guest" roles in Identity's role table
- Runs every time app starts
- Only creates if they don't exist

### 2. Assign User to Identity Role During Registration (AuthController.cs)

```csharp
var user = new User
{
    UserName = registerDto.Email,
    Email = registerDto.Email,
    Role = UserRole.User,  // Set enum property
    CreatedAt = DateTime.UtcNow
};

var result = await _userManager.CreateAsync(user, registerDto.Password);

// Add user to Identity role (syncs with User.Role property)
await _userManager.AddToRoleAsync(user, user.Role.ToString());
```

**What this does:**

- Sets `user.Role = UserRole.User` (enum property)
- Adds user to Identity's "User" role
- Both systems now have the same role!

### 3. Updated Swagger with Bearer Token Support (Program.cs)

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BidSphere API",
        Version = "v1",
        Description = "Auction Management System API with JWT Authentication"
    });

    // Add JWT Bearer authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement { ... });
});
```

**What this does:**

- Adds "Authorize" button to Swagger UI
- Shows lock icons on protected endpoints
- Automatically includes Bearer token in requests

---

## How It Works Now

### Registration Flow:

1. User registers via `/api/auth/register`
2. `User.Role = UserRole.User` (enum property set)
3. User added to Identity's "User" role
4. Both systems synchronized ✅

### Authorization Flow:

1. User logs in via `/login?useCookies=false`
2. JWT token includes role claim
3. Controller has `[Authorize(Roles = "User")]`
4. Identity checks: Is user in "User" role? ✅ Yes!
5. Access granted

### Accessing Role in Code:

```csharp
// Option 1: From User entity (enum)
var user = await _userManager.FindByIdAsync(userId);
var role = user.Role; // UserRole.User (enum)

// Option 2: From Identity roles (string)
var roles = await _userManager.GetRolesAsync(user);
var roleString = roles.FirstOrDefault(); // "User" (string)

// Option 3: From JWT claims
var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value; // "User" (string)
```

---

## Testing in Swagger

### 1. Open Swagger

Go to: http://localhost:8080/swagger

### 2. Register a User

```http
POST /api/auth/register
{
  "email": "test@example.com",
  "password": "test123"
}
```

User is now in "User" role (both enum and Identity)

### 3. Login

```http
POST /login?useCookies=false
{
  "email": "test@example.com",
  "password": "test123"
}
```

Copy the `accessToken`

### 4. Authorize in Swagger

1. Click "Authorize" button (top right)
2. Enter: `Bearer {paste-token-here}`
3. Click "Authorize"
4. Click "Close"

### 5. Test Protected Endpoint

```http
GET /api/auth/profile
```

Should work! Lock icon shows it's protected.

---

## Role-Based Authorization Examples

### Require Any Authenticated User

```csharp
[Authorize]
public async Task<ActionResult> GetProfile() { ... }
```

### Require Specific Role

```csharp
[Authorize(Roles = "Admin")]
public async Task<ActionResult> CreateProduct() { ... }
```

### Require Multiple Roles (OR)

```csharp
[Authorize(Roles = "Admin,User")]
public async Task<ActionResult> PlaceBid() { ... }
```

### Check Role in Code

```csharp
if (User.IsInRole("Admin"))
{
    // Admin-only logic
}
```

---

## Summary

✅ **User.Role enum** - Easy to access, stored on User entity
✅ **Identity Roles** - Used by `[Authorize(Roles = "...")]`
✅ **Synchronized** - Both systems stay in sync
✅ **Seeded on startup** - Admin, User, Guest roles created automatically
✅ **Assigned on registration** - Users added to Identity role
✅ **Swagger configured** - Bearer token input with metadata

**Result:** Role-based authorization now works correctly! 🎉

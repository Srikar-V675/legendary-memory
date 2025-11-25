# BidSphere - Implementation Plan

## Project Overview

Building an auction management system with .NET 8 in **1 day**. Focus on working code, not perfection.

---

## Key Decisions & Constraints

### Development Approach

- Write code like a beginner with some experience
- Make it work first, don't over-engineer
- No unnecessary comments about future enhancements
- Basic tests covering main scenarios only
- Use FluentValidation for essential validations only

### Technical Stack

- **.NET 8 Web API**
- **ASP.NET Core Identity** (user management, password hashing, roles)
- **JWT Tokens** (stateless API authentication)
- **Entity Framework Core** (InMemory for dev, switchable to Postgres/SqlServer)
- **AutoMapper** (already configured)
- **FluentValidation** (for DTOs)
- **EPPlus** (Excel upload)
- **MailKit** (simple email notifications)
- **Swagger** (API documentation)

### Database Strategy

- **Development**: InMemory database (no SQL Server/Postgres needed locally)
- **Production**: Postgres (already configured)
- Switch via `appsettings.Development.json` setting
- All EF Core code remains provider-agnostic
- No code changes needed when moving to production

### Configuration Management

- **Static values**: `appsettings.Development.json`
- **Dynamic runtime config**: Static `AuctionConfig` class
- Admin endpoint `PUT /api/config` to update values at runtime
- Changes apply immediately without restart
- Values reset to defaults on app restart (not persisted to DB)

**Configurable Settings:**

- Anti-sniping threshold (default: 60 seconds)
- Auction extension duration (default: 60 seconds)
- Payment timeout (default: 60 seconds)
- Max retry attempts (default: 3)

### Auction Status Flow

```
ACTIVE → EXPIRED → (payment flow) → COMPLETED or FAILED
```

- **ACTIVE**: Auction is live and accepting bids
- **EXPIRED**: Time ran out, waiting for payment
- **COMPLETED**: Payment successful
- **FAILED**: All payment attempts failed OR auction cancelled

---

## Existing Project Structure

```
BidSphere/
├── Controllers/
│   └── ProductController.cs (basic CRUD - needs update)
├── Models/
│   ├── Domain/
│   │   └── Product.cs (basic model - needs update)
│   └── Dtos/
│       └── ProductDto.cs
├── Repository/
│   ├── Interface/
│   │   └── IProductRepository.cs
│   └── Implementation/
│       └── ProductRepository.cs
├── Service/
│   ├── Interface/
│   │   └── IProductService.cs
│   └── Implementation/
│       └── ProductService.cs
├── Mapper/
│   └── ProductMapper.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Migrations/ (existing migrations)
├── Program.cs (DI setup, auto-migration)
└── appsettings.Development.json
```

**Pattern Used:**

- Controller → Service → Repository → DbContext
- DTOs for API contracts
- AutoMapper for entity-DTO mapping
- Interface-based dependency injection

---

## Final Folder Structure

```
BidSphere/
├── Controllers/
│   ├── AuthController.cs                    [NEW]
│   ├── ProductsController.cs                [UPDATE]
│   ├── BidsController.cs                    [NEW]
│   ├── DashboardController.cs               [NEW]
│   ├── TransactionsController.cs            [NEW]
│   └── ConfigController.cs                  [NEW]
│
├── Models/
│   ├── Domain/
│   │   ├── User.cs                          [NEW - Identity user]
│   │   ├── Product.cs                       [UPDATE]
│   │   ├── Auction.cs                       [NEW]
│   │   ├── Bid.cs                           [NEW]
│   │   └── PaymentAttempt.cs                [NEW]
│   ├── Dtos/
│   │   ├── Auth/
│   │   │   ├── RegisterDto.cs               [NEW]
│   │   │   ├── LoginDto.cs                  [NEW]
│   │   │   ├── LoginResponseDto.cs          [NEW]
│   │   │   └── UserDto.cs                   [NEW]
│   │   ├── Products/
│   │   │   ├── ProductDto.cs                [UPDATE]
│   │   │   ├── CreateProductDto.cs          [NEW]
│   │   │   ├── UpdateProductDto.cs          [NEW]
│   │   │   └── AuctionDetailsDto.cs         [NEW]
│   │   ├── Bids/
│   │   │   ├── BidDto.cs                    [NEW]
│   │   │   └── PlaceBidDto.cs               [NEW]
│   │   ├── Dashboard/
│   │   │   └── DashboardStatsDto.cs         [NEW]
│   │   └── Common/
│   │       ├── PaginatedResponse.cs         [NEW]
│   │       └── ApiResponse.cs               [NEW]
│   └── Enums/
│       ├── AuctionStatus.cs                 [NEW]
│       ├── PaymentStatus.cs                 [NEW]
│       └── UserRole.cs                      [NEW]
│
├── Repository/
│   ├── Interface/
│   │   ├── IProductRepository.cs            [UPDATE]
│   │   ├── IAuctionRepository.cs            [NEW]
│   │   ├── IBidRepository.cs                [NEW]
│   │   └── IPaymentRepository.cs            [NEW]
│   └── Implementation/
│       ├── ProductRepository.cs             [UPDATE]
│       ├── AuctionRepository.cs             [NEW]
│       ├── BidRepository.cs                 [NEW]
│       └── PaymentRepository.cs             [NEW]
│
├── Service/
│   ├── Interface/
│   │   ├── IProductService.cs               [UPDATE]
│   │   ├── IBidService.cs                   [NEW]
│   │   ├── IAuthService.cs                  [NEW]
│   │   ├── IEmailService.cs                 [NEW]
│   │   ├── IExcelService.cs                 [NEW]
│   │   ├── IAsqlParserService.cs            [NEW]
│   │   └── IDashboardService.cs             [NEW]
│   └── Implementation/
│       ├── ProductService.cs                [UPDATE]
│       ├── BidService.cs                    [NEW]
│       ├── AuthService.cs                   [NEW]
│       ├── EmailService.cs                  [NEW]
│       ├── ExcelService.cs                  [NEW]
│       ├── AsqlParserService.cs             [NEW]
│       └── DashboardService.cs              [NEW]
│
├── BackgroundServices/
│   ├── AuctionExpiryMonitor.cs              [NEW]
│   ├── AuctionFinalizer.cs                  [NEW]
│   └── RetryQueueService.cs                 [NEW]
│
├── Validators/
│   ├── RegisterDtoValidator.cs              [NEW]
│   ├── LoginDtoValidator.cs                 [NEW]
│   ├── CreateProductDtoValidator.cs         [NEW]
│   ├── UpdateProductDtoValidator.cs         [NEW]
│   └── PlaceBidDtoValidator.cs              [NEW]
│
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs       [NEW]
│
├── Helpers/
│   ├── PaginationHelper.cs                  [NEW]
│   └── JwtHelper.cs                         [NEW]
│
├── Constants/
│   └── AuctionConfig.cs                     [NEW - dynamic config]
│
├── Data/
│   ├── ApplicationDbContext.cs              [UPDATE]
│   └── DbSeeder.cs                          [NEW]
│
├── Mapper/
│   ├── ProductMapper.cs                     [UPDATE]
│   ├── AuthMapper.cs                        [NEW]
│   └── BidMapper.cs                         [NEW]
│
├── Migrations/                              [NEW migrations]
├── Program.cs                               [UPDATE]
├── appsettings.json                         [NO CHANGES]
├── appsettings.Development.json             [UPDATE]
└── BidSphere.csproj                         [UPDATE]
```

---

## Phase-by-Phase Implementation Plan

### **Phase 0: Setup & Configuration** ⏱️ 30 mins

**Goal:** Update packages and configure InMemory DB

1. Update `BidSphere.csproj` with new packages:

   - Microsoft.AspNetCore.Identity.EntityFrameworkCore
   - Microsoft.AspNetCore.Authentication.JwtBearer
   - FluentValidation.AspNetCore
   - EPPlus
   - MailKit

2. Update `appsettings.Development.json`:

   - Add DatabaseProvider setting (InMemory/Postgres/SqlServer)
   - Add JWT settings (Secret, Issuer, Audience, ExpiryMinutes)
   - Add AuctionSettings (thresholds, timeouts)
   - Add SMTP settings (for email)

3. Create `Constants/AuctionConfig.cs`:

   - Static properties for runtime config
   - Load from appsettings on startup

4. Update `Program.cs`:
   - Add conditional DB provider logic
   - Configure Identity
   - Configure JWT authentication
   - Register FluentValidation
   - Register all services and repositories
   - Register background services

**Deliverables:**

- ✅ All packages installed
- ✅ InMemory DB configured and working
- ✅ appsettings.Development.json updated
- ✅ AuctionConfig class created

---

### **Phase 1: Entities & Database** ⏱️ 45 mins

**Goal:** Create all domain models and relationships

1. Create enums in `Models/Enums/`:

   - `AuctionStatus` (Active, Expired, Completed, Failed)
   - `PaymentStatus` (Pending, Success, Failed)
   - `UserRole` (Admin, User, Guest)

2. Create/Update domain models in `Models/Domain/`:

   - **User** (Identity user with Role)
   - **Product** (ProductId, Name, Description, Category, StartingPrice, OwnerId, CreatedAt)
   - **Auction** (AuctionId, ProductId, StartTime, ExpiryTime, Status, HighestBidId, ExtensionCount)
   - **Bid** (BidId, AuctionId, BidderId, Amount, Timestamp)
   - **PaymentAttempt** (PaymentId, AuctionId, BidderId, Status, AttemptNumber, AttemptTime, ConfirmedAmount)

3. Update `ApplicationDbContext`:

   - Inherit from IdentityDbContext<User>
   - Add DbSets for all entities
   - Configure relationships in OnModelCreating
   - Add indexes for performance

4. Create `Data/DbSeeder.cs`:

   - Seed Admin and User roles
   - Seed 1 admin user
   - Seed 2 regular users
   - Seed 5 sample products with auctions

5. Create new migration and update database

**Entity Relationships:**

- User 1:N Products (as Owner)
- User 1:N Bids (as Bidder)
- Product 1:1 Auction
- Auction 1:N Bids
- Auction 1:N PaymentAttempts
- Auction N:1 Bid (HighestBid)

**Deliverables:**

- ✅ All entities created with proper relationships
- ✅ DbContext updated
- ✅ Migration created and applied
- ✅ Seed data loaded

---

### **Phase 2: Authentication & Authorization** ⏱️ 1 hour

**Goal:** Complete auth system with JWT and roles

1. Create DTOs in `Models/Dtos/Auth/`:

   - RegisterDto, LoginDto, LoginResponseDto, UserDto

2. Create validators in `Validators/`:

   - RegisterDtoValidator (email format, password strength)
   - LoginDtoValidator

3. Create `Helpers/JwtHelper.cs`:

   - GenerateToken method
   - Takes User and returns JWT string

4. Create `Service/Interface/IAuthService.cs` and implementation:

   - RegisterAsync(RegisterDto)
   - LoginAsync(LoginDto) → returns JWT token
   - GetProfileAsync(userId)
   - UpdateProfileAsync(userId, UserDto)

5. Create `Controllers/AuthController.cs`:

   - POST /api/auth/register
   - POST /api/auth/login
   - GET /api/auth/profile [Authorize]
   - PUT /api/auth/profile [Authorize]

6. Create `Mapper/AuthMapper.cs` for AutoMapper profiles

7. Test authentication flow:
   - Register admin and user
   - Login and get JWT token
   - Access protected endpoint with token
   - Verify role-based access

**Deliverables:**

- ✅ User registration working
- ✅ Login returns JWT token
- ✅ Token authentication working
- ✅ Role-based authorization working
- ✅ Profile endpoints working

---

### **Phase 3: Products & Auctions (Milestone 1)** ⏱️ 1 hour

**Goal:** Complete product CRUD with auctions

1. Create DTOs in `Models/Dtos/Products/`:

   - CreateProductDto, UpdateProductDto, AuctionDetailsDto

2. Create validators:

   - CreateProductDtoValidator (price > 0, duration 2min-24hrs)
   - UpdateProductDtoValidator

3. Update `Repository/Interface/IProductRepository.cs` and implementation:

   - GetAllAsync(filters, pagination)
   - GetActiveAuctionsAsync(filters, pagination)
   - GetByIdAsync(id)
   - CreateAsync(product)
   - UpdateAsync(product)
   - DeleteAsync(id)
   - HasActiveBidsAsync(productId)

4. Create `Repository/Interface/IAuctionRepository.cs` and implementation:

   - GetByProductIdAsync(productId)
   - GetExpiredAuctionsAsync()
   - UpdateAsync(auction)

5. Update `Service/Interface/IProductService.cs` and implementation:

   - Business logic for all operations
   - Auto-create auction when product is created
   - Validate no active bids before update/delete
   - Calculate expiry time based on duration

6. Update `Controllers/ProductsController.cs`:

   - GET /api/products (with filters: status, category, minPrice, maxPrice)
   - GET /api/products/active
   - GET /api/products/{id}
   - POST /api/products [Authorize(Roles="Admin")]
   - PUT /api/products/{id} [Authorize(Roles="Admin")]
   - DELETE /api/products/{id} [Authorize(Roles="Admin")]
   - PUT /api/products/{id}/finalize [Authorize(Roles="Admin")]

7. Update AutoMapper profiles

**Deliverables:**

- ✅ Product CRUD working
- ✅ Auction auto-created with product
- ✅ Filtering working
- ✅ Role-based access enforced
- ✅ 5 sample products seeded

---

### **Phase 4: Bidding & Anti-Sniping (Milestone 2)** ⏱️ 1.5 hours

**Goal:** Bid placement and dynamic auction extension

1. Create DTOs in `Models/Dtos/Bids/`:

   - PlaceBidDto, BidDto

2. Create validator:

   - PlaceBidDtoValidator

3. Create `Repository/Interface/IBidRepository.cs` and implementation:

   - CreateAsync(bid)
   - GetByAuctionIdAsync(auctionId)
   - GetAllAsync(filters: userId, productId, dateRange, amountRange)
   - GetHighestBidAsync(auctionId)
   - GetNextHighestBidderAsync(auctionId, excludeBidderId)

4. Create `Service/Interface/IBidService.cs` and implementation:

   - PlaceBidAsync(userId, PlaceBidDto)
     - Validate auction is ACTIVE
     - Validate bid > current highest
     - Validate user != product owner
     - Check if bid within last minute → extend auction
     - Update auction's HighestBidId
   - GetBidsByAuctionAsync(auctionId)
   - GetBidsAsync(filters)

5. Create `Controllers/BidsController.cs`:

   - POST /api/bids [Authorize]
   - GET /api/bids/{auctionId}
   - GET /api/bids (with filters)

6. Create `BackgroundServices/AuctionExpiryMonitor.cs`:

   - Runs every 10 seconds
   - Finds auctions where ExpiryTime <= Now and Status = ACTIVE
   - Changes status to EXPIRED
   - Triggers payment flow

7. Update AutoMapper profiles

**Anti-Sniping Logic:**

```csharp
if (auction.ExpiryTime - DateTime.UtcNow < TimeSpan.FromSeconds(AuctionConfig.AntiSnipingThresholdSeconds))
{
    auction.ExpiryTime = auction.ExpiryTime.AddSeconds(AuctionConfig.ExtensionDurationSeconds);
    auction.ExtensionCount++;
}
```

**Deliverables:**

- ✅ Bid placement working with validations
- ✅ Anti-sniping extension working
- ✅ Background service monitoring auctions
- ✅ Bid filtering working
- ✅ Highest bid tracking working

---

### **Phase 5: Excel Upload & ASQL (Milestone 3)** ⏱️ 1 hour

**Goal:** Bulk upload and advanced filtering

1. Create `Service/Interface/IExcelService.cs` and implementation:

   - ParseProductsFromExcel(IFormFile file)
   - Validate columns: ProductId, Name, StartingPrice, Description, Category, DurationMinutes
   - Skip invalid rows and log errors
   - Return list of valid products

2. Add endpoint to ProductsController:

   - POST /api/products/upload [Authorize(Roles="Admin")]
   - Accepts .xlsx file
   - Returns summary (success count, failed rows)

3. Create `Service/Interface/IAsqlParserService.cs` and implementation:

   - ParseQuery(string asqlQuery)
   - Support operators: =, !=, <, <=, >, >=, in
   - Support AND, OR
   - Return IQueryable filter expression
   - Examples:
     - `productId=1 OR name="Vintage Watch"`
     - `category="Art" AND startingPrice>=1000`
     - `category in ["Electronics", "Art"]`

4. Create `Helpers/PaginationHelper.cs`:

   - ApplyPagination<T>(IQueryable<T> query, int page, int pageSize)
   - Return PaginatedResponse<T>

5. Update ProductsController to support ASQL:

   - GET /api/products?asql=category="Electronics" AND price>1000

6. Add FluentValidation to all endpoints

**Deliverables:**

- ✅ Excel upload working
- ✅ ASQL parser working
- ✅ Pagination working
- ✅ Validation on all endpoints

---

### **Phase 6: Payment & Notifications (Milestone 4)** ⏱️ 1.5 hours

**Goal:** Payment confirmation with retry logic

1. Create `Service/Interface/IEmailService.cs` and implementation:

   - SendPaymentNotificationAsync(userEmail, auctionDetails)
   - Use MailKit with SMTP
   - Simple HTML template

2. Create `Repository/Interface/IPaymentRepository.cs` and implementation:

   - CreateAsync(paymentAttempt)
   - GetByAuctionIdAsync(auctionId)
   - GetPendingPaymentAsync(auctionId)
   - UpdateAsync(paymentAttempt)

3. Update AuctionExpiryMonitor:

   - When auction expires, create PaymentAttempt for highest bidder
   - Send email notification
   - Start 60-second timer

4. Create `BackgroundServices/RetryQueueService.cs`:

   - Monitors PaymentAttempts with Status=Pending
   - If timeout exceeded and no confirmation → mark as Failed
   - Get next highest bidder
   - Create new PaymentAttempt (up to 3 total)
   - If all attempts fail → mark auction as FAILED

5. Add endpoint to ProductsController:

   - PUT /api/products/{id}/confirm [Authorize]
   - Body: { confirmedAmount }
   - Validate user is current eligible bidder
   - Validate amount matches highest bid
   - Support testInstantFail query param
   - On success → mark auction as COMPLETED
   - On fail → trigger retry

6. Create `Controllers/TransactionsController.cs`:

   - GET /api/transactions [Authorize]
   - Filter by userId (users see own, admins see all)
   - Return payment history

7. Create `Middleware/ExceptionHandlingMiddleware.cs`:
   - Catch all exceptions
   - Return consistent error response
   - Log errors

**Payment Flow:**

```
1. Auction expires → Status = EXPIRED
2. Create PaymentAttempt (AttemptNumber=1, Status=Pending)
3. Send email to highest bidder
4. Wait 60 seconds
5. If confirmed with correct amount → Status=Success, Auction=COMPLETED
6. If failed/timeout → Status=Failed, get next bidder, repeat (max 3 attempts)
7. If all fail → Auction=FAILED
```

**Deliverables:**

- ✅ Email notifications working
- ✅ Payment confirmation working
- ✅ Retry logic working
- ✅ Transaction history working
- ✅ Exception handling middleware working

---

### **Phase 7: Dashboard & Config (Milestone 5)** ⏱️ 1 hour

**Goal:** Analytics and runtime configuration

1. Create `Service/Interface/IDashboardService.cs` and implementation:

   - GetStatsAsync()
     - Active auctions count
     - Pending payments count
     - Completed auctions count
     - Failed auctions count
     - Top 5 bidders (by total bids or bid amount)

2. Create `Controllers/DashboardController.cs`:

   - GET /api/dashboard [Authorize(Roles="Admin")]

3. Create `Controllers/ConfigController.cs`:

   - GET /api/config [Authorize(Roles="Admin")]
   - PUT /api/config [Authorize(Roles="Admin")]
   - Updates AuctionConfig static properties
   - Returns current config values

4. Add comprehensive Swagger documentation:

   - XML comments on all endpoints
   - Example requests/responses
   - Group by functionality

5. Write basic unit tests:
   - AuthService tests (register, login)
   - BidService tests (place bid, validations)
   - ProductService tests (CRUD operations)

**Deliverables:**

- ✅ Dashboard API working
- ✅ Config endpoint working (runtime updates)
- ✅ Swagger fully documented
- ✅ Basic unit tests passing

---

## API Endpoints Summary

### Authentication

- POST /api/auth/register
- POST /api/auth/login
- GET /api/auth/profile [Auth]
- PUT /api/auth/profile [Auth]

### Products & Auctions

- GET /api/products (filters: status, category, minPrice, maxPrice, asql)
- GET /api/products/active
- GET /api/products/{id}
- POST /api/products [Admin]
- POST /api/products/upload [Admin]
- PUT /api/products/{id} [Admin]
- PUT /api/products/{id}/finalize [Admin]
- PUT /api/products/{id}/confirm [Auth]
- DELETE /api/products/{id} [Admin]

### Bids

- POST /api/bids [Auth]
- GET /api/bids/{auctionId}
- GET /api/bids (filters: userId, productId, dateRange, amountRange)

### Dashboard & Analytics

- GET /api/dashboard [Admin]

### Configuration

- GET /api/config [Admin]
- PUT /api/config [Admin]

### Transactions

- GET /api/transactions [Auth]

---

## Testing Checklist

### Phase 2: Authentication

- [ ] Register new user
- [ ] Register duplicate email (should fail)
- [ ] Login with correct credentials
- [ ] Login with wrong password (should fail)
- [ ] Access protected endpoint without token (should fail)
- [ ] Access protected endpoint with valid token
- [ ] Access admin endpoint as user (should fail)
- [ ] Access admin endpoint as admin

### Phase 3: Products

- [ ] Create product as admin
- [ ] Create product as user (should fail)
- [ ] List all products
- [ ] Filter products by category
- [ ] Filter products by price range
- [ ] Get product details
- [ ] Update product with no bids
- [ ] Update product with active bids (should fail)
- [ ] Delete product with no bids
- [ ] Delete product with active bids (should fail)

### Phase 4: Bidding

- [ ] Place bid on active auction
- [ ] Place bid lower than current highest (should fail)
- [ ] Place bid on own product (should fail)
- [ ] Place bid on expired auction (should fail)
- [ ] Place bid within last minute (should extend auction)
- [ ] Verify auction expires after time
- [ ] Verify highest bid is tracked correctly

### Phase 5: Excel & ASQL

- [ ] Upload valid Excel file
- [ ] Upload Excel with invalid rows (should skip and log)
- [ ] Upload non-Excel file (should fail)
- [ ] Query with ASQL: `category="Electronics"`
- [ ] Query with ASQL: `price>1000 AND category="Art"`
- [ ] Query with ASQL: `category in ["Electronics", "Fashion"]`
- [ ] Pagination with page=1&pageSize=10

### Phase 6: Payment

- [ ] Auction expires, highest bidder gets email
- [ ] Confirm payment with correct amount
- [ ] Confirm payment with wrong amount (should fail)
- [ ] Confirm payment as non-winner (should fail)
- [ ] Payment timeout triggers retry to next bidder
- [ ] Test instant fail mode (?testInstantFail=true)
- [ ] All 3 attempts fail, auction marked as FAILED
- [ ] View transaction history

### Phase 7: Dashboard

- [ ] Get dashboard stats as admin
- [ ] Get dashboard as user (should fail)
- [ ] Update config values
- [ ] Verify config changes apply immediately
- [ ] Verify config resets on app restart

---

## Important Notes

### Database Provider Switching

In `appsettings.Development.json`:

```json
"DatabaseProvider": "InMemory"  // Change to "Postgres" or "SqlServer" for production
```

No code changes needed - EF Core handles it.

### Email Setup (Phase 6)

Add to `appsettings.Development.json`:

```json
"SmtpSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "FromEmail": "noreply@bidsphere.com",
  "FromName": "BidSphere"
}
```

For Gmail: Enable 2FA and create App Password.

### Runtime Config

Changes via `/api/config` endpoint apply immediately but don't persist. On restart, values reload from `appsettings.Development.json`.

### Background Services

- AuctionExpiryMonitor: Runs every 10 seconds
- RetryQueueService: Runs every 5 seconds
- Both use AuctionConfig for thresholds

### Git Commits

Make meaningful commits after each phase:

- "Phase 0: Setup packages and InMemory DB"
- "Phase 1: Create entities and relationships"
- "Phase 2: Implement authentication with JWT"
- etc.

---

## Time Estimates

| Phase     | Task                           | Time         |
| --------- | ------------------------------ | ------------ |
| 0         | Setup & Configuration          | 30 mins      |
| 1         | Entities & Database            | 45 mins      |
| 2         | Authentication & Authorization | 1 hour       |
| 3         | Products & Auctions            | 1 hour       |
| 4         | Bidding & Anti-Sniping         | 1.5 hours    |
| 5         | Excel Upload & ASQL            | 1 hour       |
| 6         | Payment & Notifications        | 1.5 hours    |
| 7         | Dashboard & Config             | 1 hour       |
| **Total** |                                | **~8 hours** |

Buffer time for debugging and testing: 2-3 hours

---

## Quick Reference Commands

### Run Application

```bash
cd BidSphere
dotnet run
```

### Create Migration

```bash
dotnet ef migrations add "MigrationName"
```

### Update Database

```bash
dotnet ef database update
```

### Run Tests

```bash
dotnet test
```

### Access Swagger

```
http://localhost:8080/swagger
```

---

## Next Steps

Start with **Phase 0: Setup & Configuration**

1. Update BidSphere.csproj with packages
2. Update appsettings.Development.json
3. Create AuctionConfig.cs
4. Update Program.cs

Ready to begin!

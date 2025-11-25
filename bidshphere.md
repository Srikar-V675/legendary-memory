# **BidSphere – Auction Management System (Documentation)**

## **1. Introduction**

**BidSphere** is a smart, event-driven auction management platform built with **.NET 8**, allowing users to create, bid, and manage live auctions in real time. It supports dynamic auction extensions, retries for payment, analytics, filtering, and secure authentication.
All features can be tested through Swagger APIs.

---

## **2. Problem Statement**

Build a modular and scalable auction system where:

- Admins can upload products (manually or via Excel).
- Registered users can browse, filter, and bid on products.
- Auctions auto-extend near expiry (anti-sniping).
- Winners confirm payments within a time limit.
- Payment failures trigger retries for the next highest bidder.
- System provides analytics, filtering, and role-based access control.

---

## **3. Functional Requirements**

The application supports signup, login, and auction participation for users.
Admins manage products, auctions, and analytics.
Role-based operations are enforced throughout.

---

## **3.1 Products & Auctions**

### **Product Attributes**

- **ProductId** – Unique product identifier
- **Name** – Product name
- **Description** – Detailed description
- **Category** – e.g., Electronics, Art, Fashion
- **StartingPrice** – Must be > 0
- **AuctionDuration** – Between **2 min** to **24 hrs**
- **OwnerId** – Admin who created it
- **ExpiryTime** – Auto-calculated
- **HighestBidId** – Reference to current highest bid

### **Tasks**

a. List products with optional filters
b. List active auctions (with highest bid + remaining time)
c. Get details of a specific auction (with all bids)
d. Create a product
e. Upload multiple products via **Excel (.xlsx)**
f. Update a product (only if no active bids)
g. Delete a product (only if no active bids)
h. Force finalize an auction (Admin override)

### **Excel Upload Specifications**

- Format: **.xlsx**
- Required columns:
  `ProductId`, `Name`, `StartingPrice`, `Description`, `Category`, `DurationMinutes`
- Validations:

  - Unique product IDs
  - Valid price (> 0)
  - Correct column headers

- Invalid rows → logged + skipped

### **API Endpoints**

| Method | Endpoint                    | Role  | Description                |
| ------ | --------------------------- | ----- | -------------------------- |
| POST   | /api/products               | Admin | Create a product           |
| POST   | /api/products/upload        | Admin | Upload Excel with products |
| GET    | /api/products               | Any   | List products with filters |
| GET    | /api/products/active        | Any   | List active auctions       |
| GET    | /api/products/{id}          | Any   | Get auction details        |
| PUT    | /api/products/{id}          | Admin | Update product             |
| PUT    | /api/products/{id}/finalize | Admin | Force finalize             |
| DELETE | /api/products/{id}          | Admin | Delete product             |

### **Filtering Example**

```
GET /api/products?status=active&minPrice=200&maxPrice=500&category=electronics
```

---

## **3.2 Bid Management**

### **Bid Attributes**

- **BidId**
- **AuctionId**
- **BidderId**
- **Amount** (must be > highest bid)
- **Timestamp**

### **Tasks**

a. Place a bid
b. Get all bids for an auction
c. Filter bids by:

- userId
- productId
- date range
- min/max amount

### **Validations**

- Bid must be greater than current highest bid
- Auction must be ACTIVE
- Users cannot bid on their own products
- User may bid multiple times but must always outbid previous highest

### **API Endpoints**

| Method | Endpoint              | Role       | Description          |
| ------ | --------------------- | ---------- | -------------------- |
| POST   | /api/bids             | User       | Place bid            |
| GET    | /api/bids/{auctionId} | Any        | Get bids for auction |
| GET    | /api/bids             | Admin/User | Filter bids          |

### **Filtering Example**

```
GET /api/bids?userId=123&minAmount=500&maxAmount=1000&startDate=2025-01-01&endDate=2025-01-31
```

---

## **3.3 Dynamic Auction Extension (Anti-Sniping)**

### **Extension Rules**

- If a bid arrives **within last 1 minute**
- Auction extends automatically by **+1 minute**
- Multiple extensions allowed
- Each extension logged

### **Implementation**

- Detect bids made with <1 min remaining
- Background service finalizes expired auctions

---

## **3.4 Payment Confirmation & Retry Logic**

### **Payment Attributes**

- **PaymentId**
- **AuctionId**
- **BidderId**
- **Status** (Pending, Success, Failed)
- **AttemptNumber** (1–3)
- **AttemptTime**

### **Payment Flow**

1. Auction ends → highest bidder gets email + **1-minute window**
2. User submits payment:

   - Product identity
   - Confirmed amount

3. If amount ≠ highest bid → Fail attempt immediately
4. **Test Mode** (`?testInstantFail=true`)

   - Instantly fail attempt
   - Move to next bidder

5. If no reply within 1 minute → Fail attempt
6. Retry for next-highest bidder (up to **3 attempts**)

### **Auction Result**

- Any successful payment → **COMPLETED**
- All attempts fail → **FAILED**

### **Validations**

- Only current eligible winner can confirm payment
- Confirmed amount must match highest bid
- Instant fail mode triggers immediate retry
- Payment window strictly enforced

### **API Endpoints**

| Method | Endpoint                   | Role       | Description             |
| ------ | -------------------------- | ---------- | ----------------------- |
| PUT    | /api/products/{id}/confirm | User       | Winner confirms payment |
| GET    | /api/transactions          | Admin/User | Filter transactions     |

---

## **3.5 Dashboard & Analytics**

### **Dashboard Metrics**

- Active auctions
- Pending payments
- Completed auctions
- Failed auctions
- Top bidders

### **Tasks**

a. System-wide statistics
b. Auction performance metrics
c. Payment success rates

### **API Endpoint**

| Method | Endpoint       | Role  | Description    |
| ------ | -------------- | ----- | -------------- |
| GET    | /api/dashboard | Admin | System metrics |

### **Example Response**

```json
{
  "activeCount": 10,
  "pendingPayment": 2,
  "completedCount": 7,
  "failedCount": 1,
  "topBidders": []
}
```

---

## **3.6 Roles and Permissions**

### **Roles**

#### **Admin**

- Create/Update/Delete products
- Upload Excel
- Force finalize auctions
- View dashboard
- Manage roles
- Access all transactions
- Includes User permissions

#### **User**

- Bid on active auctions
- View products
- Confirm payment
- View own bids & transactions

#### **Guest**

- Read-only access to auctions and product details

### **Authorization Rules**

- JWT required for all modifying operations
- Role-based access enforced
- Only winning bidder can confirm payment
- Users cannot bid on own products

---

## **3.7 Search & Filter Query Language (ASQL)**

Custom query language supporting **AND**, **OR**, and operators:
`=, !=, <, <=, >, >=, in`

### **Examples**

```
/api/products/?asql=productId=1 OR name="Vintage Watch"
/api/products/?asql=productId=1 AND category="Electronics"
/api/products/?asql=category="Art" AND startingPrice>=1000
/api/products/?asql=category!="Fashion" AND status="Active"
/api/products/?asql=startingPrice<5000
/api/products/?asql=category in ["Electronics", "Art", "Fashion"]
```

---

## **3.8 Authentication**

### **User Attributes**

- UserId
- Email
- PasswordHash
- Role
- CreatedAt

### **Tasks**

a. Register new user
b. Login & receive JWT
c. View profile
d. Update profile

### **API Endpoints**

| Method | Endpoint           | Description    |
| ------ | ------------------ | -------------- |
| POST   | /api/auth/register | Register       |
| POST   | /api/auth/login    | Login          |
| GET    | /api/auth/profile  | Get profile    |
| PUT    | /api/auth/profile  | Update profile |

---

## **4. Entity Overview**

| Entity         | Key Fields                                                             | Description       |
| -------------- | ---------------------------------------------------------------------- | ----------------- |
| User           | UserId, Email, Role, PasswordHash, CreatedAt                           | Registered user   |
| Product        | ProductId, Name, StartingPrice, Description, Category, OwnerId         | Product details   |
| Auction        | AuctionId, ProductId, ExpiryTime, Status, HighestBidId, ExtensionCount | Auction lifecycle |
| Bid            | BidId, AuctionId, BidderId, Amount, Timestamp                          | Individual bid    |
| PaymentAttempt | PaymentId, AuctionId, BidderId, Status, AttemptNumber, AttemptTime     | Payment tracking  |

### **Relationships**

- One User → many Bids
- One Product → one Auction
- One Auction → many Bids
- One Auction → multiple Payment Attempts

---

## **5. Background Services**

| Service              | Responsibility           | Frequency          |
| -------------------- | ------------------------ | ------------------ |
| AuctionExpiryMonitor | Detects expired auctions | On auction expiry  |
| AuctionFinalizer     | Starts payment flow      | On completion      |
| RetryQueueService    | Handles payment retries  | On payment failure |
| DashboardService     | Aggregates analytics     | On-demand          |

---

# **6. Milestones**

The project is divided into major milestones, each producing concrete deliverables.
**Prerequisite:** Document all APIs using Swagger/Postman immediately as they are developed and classify them by functionality.

---

## **Milestone 1**

1. Clone the project and set up boilerplate with **.NET 8**
2. Run migrations to generate the initial database schema
3. Implement user authentication (Register, Login with JWT)
4. Create basic Product CRUD operations
5. Add 5 sample products manually
6. Integrate Swagger and ensure full API documentation
7. Design proper entity relationships for all entities
8. Add proper git commits for each feature

---

## **Milestone 2**

1. Build complete Product & Auction APIs (section 3.1)
2. Implement Bid placement and retrieval APIs (section 3.2)
3. Add basic filtering for Products and Bids
4. Set up background service for auction monitoring
5. Implement anti-sniping (dynamic auction extension) logic (section 3.3)

---

## **Milestone 3**

1. Build Excel upload functionality with validation (section 3.1)
2. Implement pagination for all list APIs
3. Build the ASQL (Auction Search Query Language) parser (section 3.7)
4. Implement role-based authorization on all endpoints
5. Add comprehensive logging throughout the application

---

## **Milestone 4**

1. Implement payment confirmation workflow (section 3.4)
2. Add mail notification service for eligible highest bidder
3. Build Retry Queue service for payment retries
4. Implement transaction tracking and history management
5. Add model validations and custom exception handling
6. Use LINQ expressions, enums, constants, and dependency injection across codebase

---

## **Milestone 5**

1. **Build Angular-based Dashboard UI** (section 3.5)

   - Create a user-friendly dashboard to visualize analytics
   - Integrate with Dashboard APIs

2. Build Dashboard & Analytics APIs (section 3.5)
3. Implement custom filters for reusable functionality
4. Add extension methods for common logic
5. Perform performance optimization and application testing
6. Write unit tests and complete documentation

---

# **7. Non-Functional Requirements**

---

## **Best Practices**

### **Code Quality**

- Follow C# coding standards and naming conventions
- Use meaningful git commits for every feature
- Maintain at least **80% unit test coverage**
- Implement full exception handling
- Use **async/await** for I/O operations
- Apply **SOLID principles** across the codebase

### **Documentation**

- Fully integrate Swagger for API documentation
- Add XML comments for public classes and methods
- Maintain a clean README with setup and usage instructions
- Document the database schema

### **Security**

- Enforce JWT token-based authentication
- Use secure password hashing
- Validate all inputs on API endpoints
- Prevent SQL injection using parameterized queries
- Enforce role-based authorization across routes

### **Performance**

- Optimize database queries; use proper indexing
- Cache frequently accessed data
- Use background services for long-running tasks
- Use connection pooling for efficient DB connections

### **Maintainability**

- Use enums for constant values
- Add model validations using data annotations
- Use dependency injection for loose coupling
- Implement repository pattern for data access
- Maintain a service layer for business logic

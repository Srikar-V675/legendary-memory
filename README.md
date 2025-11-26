# BidSphere - Auction Management System

## 📋 Overview

BidSphere is a smart, event-driven auction management platform built with **.NET 8**, allowing users to create, bid, and manage live auctions in real-time with dynamic auction extensions, payment retry logic, and comprehensive analytics.

**Current Status:** Milestone 4 Complete (98%)

---

## 🚀 Quick Start

### Prerequisites

- .NET 8 SDK
- Postgres/SQL Server (or use InMemory for testing)
- Mailtrap account (optional, for email testing)

### Setup

1. **Clone and restore:**

```bash
git clone <repository-url>
cd BidSphere
dotnet restore
```

2. **Update configuration:**
   Edit `BidSphere/appsettings.Development.json`:

```json
{
  "DatabaseProvider": "InMemory", // or "Postgres"
  "EmailSettings": {
    "Username": "YOUR_MAILTRAP_USERNAME",
    "Password": "YOUR_MAILTRAP_PASSWORD"
  }
}
```

3. **Create database (if using Postgres):**

```bash
dotnet ef migrations add "Initial"
dotnet ef database update
```

4. **Run application:**

```bash
dotnet run
```

5. **Open Swagger:**

```
http://localhost:8080/swagger
```

---

## 📚 Documentation

### Essential Guides

- **[bidshphere.md](bidshphere.md)** - Complete requirements and specifications
- **[IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md)** - Overall project architecture and plan
- **[MILESTONE4_FINAL_STATUS.md](MILESTONE4_FINAL_STATUS.md)** - Current implementation status
- **[QUICK_START_TESTING.md](QUICK_START_TESTING.md)** - 5-minute testing guide
- **[PAYMENT_TESTING_GUIDE.md](PAYMENT_TESTING_GUIDE.md)** - Complete payment flow testing
- **[MAILTRAP_SETUP_GUIDE.md](MAILTRAP_SETUP_GUIDE.md)** - Email notification setup
- **[BACKGROUND_SERVICES_FLOW.md](BACKGROUND_SERVICES_FLOW.md)** - Service architecture diagrams

---

## 🎯 Features Implemented

### ✅ Milestone 1-3 (Complete)

- User authentication (Register, Login with JWT)
- Product CRUD operations
- Auction management
- Bid placement with validations
- Anti-sniping (dynamic auction extension)
- Excel upload for bulk products
- ASQL (Auction Search Query Language)
- Pagination and filtering
- Role-based authorization

### ✅ Milestone 4 (Complete - 98%)

- Payment confirmation workflow
- Email notifications (Mailtrap)
- Retry queue service (3 attempts, 60s timeout)
- Transaction tracking and history
- Custom exception handling (5 exceptions)
- Global exception handler middleware
- Model validations (data annotations)
- Background services (3 independent services)

### 🔄 Milestone 5 (Pending)

- Angular Dashboard UI
- Dashboard & Analytics APIs
- Performance optimization
- Unit tests

---

## 🏗️ Architecture

### Background Services

1. **AuctionExpiryMonitor** - Monitors and marks expired auctions (every 10s)
2. **AuctionFinalizer** - Initiates payment flow for expired auctions (every 10s)
3. **RetryQueueService** - Handles payment timeouts and retries (every 10s)

### Key Components

- **Controllers** - API endpoints with Swagger documentation
- **Services** - Business logic layer
- **Repositories** - Data access layer
- **Validators** - FluentValidation for DTOs
- **Middleware** - Global exception handler
- **Exceptions** - Custom domain exceptions

---

## 📡 API Endpoints

### Authentication

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token
- `GET /api/auth/profile` - Get user profile [Auth]
- `PUT /api/auth/profile` - Update profile [Auth]

### Products & Auctions

- `GET /api/products` - List products with filters
- `GET /api/products/active` - List active auctions
- `GET /api/products/{id}` - Get auction details
- `POST /api/products` - Create product [Admin]
- `POST /api/products/upload` - Upload Excel [Admin]
- `PUT /api/products/{id}` - Update product [Admin]
- `PUT /api/products/{id}/finalize` - Force finalize [Admin]
- `PUT /api/products/{id}/confirm` - Confirm payment [Auth]
- `DELETE /api/products/{id}` - Delete product [Admin]

### Bids

- `POST /api/bids` - Place bid [Auth]
- `GET /api/bids/{auctionId}` - Get bids for auction
- `GET /api/bids` - Filter bids [Admin/User]

### Transactions

- `GET /api/transactions` - Get transaction history [Auth]

---

## 🧪 Testing

### Quick Test (5 minutes)

See [QUICK_START_TESTING.md](QUICK_START_TESTING.md)

### Complete Testing

See [PAYMENT_TESTING_GUIDE.md](PAYMENT_TESTING_GUIDE.md)

### Default Test Users

```
Admin:
- Email: admin@bidsphere.com
- Password: Admin@123

User 1:
- Email: user1@bidsphere.com
- Password: User@123

User 2:
- Email: user2@bidsphere.com
- Password: User@123
```

---

## 🔧 Configuration

### Auction Settings

Edit `appsettings.Development.json`:

```json
"AuctionSettings": {
  "AntiSnipingThresholdSeconds": 60,
  "ExtensionDurationSeconds": 60,
  "PaymentTimeoutSeconds": 60,
  "MaxPaymentAttempts": 3
}
```

### Database Providers

- **InMemory** - For development/testing (no setup required)
- **Postgres** - For production
- **SQL Server** - For production

---

## 🚨 Important Notes

### Payment Flow

- Works with or without email notifications
- Email failures don't break payment flow
- Automatic retry to next highest bidder
- Up to 3 payment attempts
- 60-second timeout per attempt

### Database

- Currently using InMemory for development
- Code is database-agnostic (EF Core)
- Migration required for Postgres/SQL Server

### Email

- Configured for Mailtrap (testing)
- Can be switched to Gmail/SendGrid for production
- Application continues if email fails

---

## 📊 Project Status

| Milestone   | Status      | Completion |
| ----------- | ----------- | ---------- |
| Milestone 1 | ✅ Complete | 100%       |
| Milestone 2 | ✅ Complete | 100%       |
| Milestone 3 | ✅ Complete | 100%       |
| Milestone 4 | ✅ Complete | 98%        |
| Milestone 5 | 🔄 Pending  | 0%         |

**Overall Progress: 78%**

---

## 🛠️ Tech Stack

- **.NET 8** - Framework
- **ASP.NET Core Identity** - Authentication
- **JWT** - Token-based auth
- **Entity Framework Core** - ORM
- **AutoMapper** - Object mapping
- **FluentValidation** - Validation
- **EPPlus** - Excel processing
- **MailKit** - Email service
- **Swagger** - API documentation

---

## 📝 Development Guidelines

### Code Quality

- Follow C# coding standards
- Use async/await for I/O operations
- Apply SOLID principles
- Maintain clean architecture

### Security

- JWT token authentication
- Role-based authorization
- Input validation
- Secure password hashing

### Performance

- Optimized database queries
- Background services for long-running tasks
- Connection pooling
- Proper indexing

---

## 🤝 Contributing

1. Follow the implementation plan
2. Write meaningful commit messages
3. Add tests for new features
4. Update documentation
5. Follow coding standards

---

## 📞 Support

For issues or questions:

1. Check documentation files
2. Review implementation plan
3. Check Swagger API documentation
4. Review test guides

---

## 📄 License

[Your License Here]

---

## 🎯 Next Steps

1. Test payment flow (see QUICK_START_TESTING.md)
2. Test with actual database
3. Implement Milestone 5 (Dashboard UI)
4. Add unit tests
5. Performance optimization

---

**Built with ❤️ using .NET 8**

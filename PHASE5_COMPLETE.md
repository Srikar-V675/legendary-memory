# Phase 5 Complete: Excel Upload & ASQL Parser

## ✅ What We Implemented

### 1. Excel Upload Service

- **ExcelService** - Parses .xlsx files and creates products in bulk
- **ExcelUploadResultDto** - Returns success/failure summary
- **POST /api/products/upload** [Admin] - Upload endpoint

**Features:**

- Validates .xlsx file format
- Validates required columns (Name, StartingPrice, Description, Category, DurationMinutes)
- Validates data (price > 0, duration 2-1440 mins)
- Skips invalid rows with error messages
- Creates products + auctions for valid rows
- Returns summary with success count and failed rows

### 2. ASQL Parser Service

- **AsqlParserService** - Parses custom query language
- Supports operators: `=`, `!=`, `<`, `<=`, `>`, `>=`, `in`
- Supports logical: `AND`, `OR`
- Applies filters to IQueryable

**Supported Fields:**

- productId
- name
- category
- startingPrice
- status

### 3. Updated Products Controller

- **GET /api/products** - Now supports ASQL and pagination
- **POST /api/products/upload** - New upload endpoint

### 4. Simple Pagination

- Added optional `page` and `pageSize` parameters
- Uses Skip() and Take()
- No complex metadata, just simple pagination

---

## 🔐 Authorization

| Endpoint                  | Auth Required | Role Required |
| ------------------------- | ------------- | ------------- |
| POST /api/products/upload | Yes           | Admin         |
| GET /api/products         | No            | Public        |

---

## 📤 Excel Upload

### Endpoint

```
POST /api/products/upload
Authorization: Bearer {admin-token}
Content-Type: multipart/form-data
```

### Required Excel Columns

- Name
- StartingPrice
- Description
- Category
- DurationMinutes

### Example Excel File

| Name          | StartingPrice | Description  | Category    | DurationMinutes |
| ------------- | ------------- | ------------ | ----------- | --------------- |
| iPhone 15     | 1000          | Brand new    | Electronics | 120             |
| Vintage Watch | 500           | 1960s watch  | Fashion     | 180             |
| Painting      | 2000          | Original art | Art         | 240             |

### Response

```json
{
  "successCount": 2,
  "failedCount": 1,
  "failedRows": [
    {
      "rowNumber": 3,
      "reason": "Invalid starting price"
    }
  ]
}
```

### Validations

- File must be .xlsx
- StartingPrice > 0
- DurationMinutes between 2 and 1440
- Name, Category, Description required
- Invalid rows skipped (not failed)

---

## 🔍 ASQL Query Language

### Endpoint

```
GET /api/products?asql={query}
```

### Supported Operators

#### Equality

```
category="Electronics"
name="Vintage Watch"
```

#### Not Equal

```
category!="Fashion"
```

#### Comparison

```
startingPrice>1000
startingPrice>=1000
startingPrice<5000
startingPrice<=5000
```

#### IN Operator

```
category in ["Electronics", "Art", "Fashion"]
```

#### AND Logic

```
category="Electronics" AND startingPrice>1000
```

#### OR Logic

```
productId=1 OR productId=2
category="Electronics" OR category="Art"
```

#### Complex Queries

```
category="Art" AND startingPrice>=1000 AND startingPrice<=10000
(category="Electronics" OR category="Art") AND startingPrice>500
```

### Example Requests

**Filter by category:**

```
GET /api/products?asql=category="Electronics"
```

**Filter by price range:**

```
GET /api/products?asql=startingPrice>=1000 AND startingPrice<=5000
```

**Multiple categories:**

```
GET /api/products?asql=category in ["Electronics", "Art"]
```

**Exclude category:**

```
GET /api/products?asql=category!="Fashion"
```

**Complex filter:**

```
GET /api/products?asql=category="Electronics" AND startingPrice>2000
```

---

## 📄 Pagination

### Simple Skip/Take Approach

**Endpoint:**

```
GET /api/products?page=1&pageSize=10
```

**With ASQL:**

```
GET /api/products?asql=category="Electronics"&page=1&pageSize=10
```

**How it works:**

```csharp
// Skip to page
products = products.Skip((page - 1) * pageSize)
                   .Take(pageSize);
```

**Examples:**

- Page 1, PageSize 10: Items 1-10
- Page 2, PageSize 10: Items 11-20
- Page 3, PageSize 5: Items 11-15

---

## 🧪 Testing Guide

### Test 1: Excel Upload

**Create sample Excel file:**

```
Name | StartingPrice | Description | Category | DurationMinutes
iPhone 15 | 1000 | New phone | Electronics | 120
Laptop | 1500 | Gaming laptop | Electronics | 180
Watch | 500 | Vintage | Fashion | 60
```

**Upload:**

```
POST /api/products/upload
Authorization: Bearer {admin-token}
[Upload file]

Expected:
{
  "successCount": 3,
  "failedCount": 0,
  "failedRows": []
}
```

### Test 2: Excel with Invalid Rows

**Create Excel with errors:**

```
Name | StartingPrice | Description | Category | DurationMinutes
Valid Product | 100 | Test | Electronics | 60
Invalid Price | 0 | Test | Electronics | 60
Invalid Duration | 100 | Test | Electronics | 5000
```

**Expected:**

```json
{
  "successCount": 1,
  "failedCount": 2,
  "failedRows": [
    { "rowNumber": 3, "reason": "Invalid starting price" },
    { "rowNumber": 4, "reason": "Duration must be between 2 and 1440 minutes" }
  ]
}
```

### Test 3: ASQL Queries

**Filter by category:**

```
GET /api/products?asql=category="Electronics"
# Should return 2 products (MacBook, Gaming PC)
```

**Filter by price:**

```
GET /api/products?asql=startingPrice>3000
# Should return 3 products (Rolex, Picasso, Rug)
```

**AND logic:**

```
GET /api/products?asql=category="Electronics" AND startingPrice>2000
# Should return 2 products (MacBook $2500, Gaming PC $3000)
```

**OR logic:**

```
GET /api/products?asql=productId=1 OR productId=2
# Should return 2 products
```

**IN operator:**

```
GET /api/products?asql=category in ["Electronics", "Art"]
# Should return 4 products
```

**NOT EQUAL:**

```
GET /api/products?asql=category!="Fashion"
# Should return 4 products (all except Rolex)
```

### Test 4: Pagination

**Page 1:**

```
GET /api/products?page=1&pageSize=2
# Should return first 2 products
```

**Page 2:**

```
GET /api/products?page=2&pageSize=2
# Should return next 2 products
```

**With ASQL:**

```
GET /api/products?asql=category="Electronics"&page=1&pageSize=1
# Should return 1 Electronics product
```

---

## 🎯 Current Status

✅ Excel upload working
✅ ASQL parser working
✅ All operators supported (=, !=, <, <=, >, >=, in)
✅ AND/OR logic working
✅ Simple pagination with Skip/Take
✅ Admin-only upload endpoint
✅ Public ASQL filtering

---

## 📋 Next Steps (Phase 6)

**Payment & Notifications (Milestone 4)**

1. Email service for notifications
2. Payment confirmation workflow
3. Retry queue for failed payments
4. Transaction tracking
5. Exception handling middleware

Ready to proceed with Phase 6!

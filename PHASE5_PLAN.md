# Phase 5 Plan: Excel Upload & ASQL (Milestone 3)

## 🎯 Goal

Implement bulk product upload via Excel and advanced filtering using ASQL (Auction Search Query Language).

---

## 📋 Actions for Phase 5

### Part A: Excel Upload (Bulk Product Creation)

#### 1. Add EPPlus Package

**Action:** Verify EPPlus package is installed

- Package: `EPPlus` (for reading .xlsx files)
- Already in csproj or need to add

#### 2. Create Excel Service (2 files)

**Location:** `Service/Interface/IExcelService.cs` and `Service/Implementation/ExcelService.cs`

**Method to implement:**

```csharp
Task<ExcelUploadResultDto> ParseProductsFromExcelAsync(IFormFile file, int ownerId)
```

**Logic:**

- Validate file is .xlsx format
- Read Excel file using EPPlus
- Validate required columns exist:
  - Name
  - StartingPrice
  - Description
  - Category
  - DurationMinutes
- For each row:
  - Validate data (price > 0, duration 2-1440 mins)
  - If valid → create product + auction
  - If invalid → log error and skip
- Return summary: success count, failed rows with reasons

#### 3. Create Excel Upload DTO

**Location:** `Models/Dtos/Products/ExcelUploadResultDto.cs`

**Properties:**

```csharp
- SuccessCount (int)
- FailedCount (int)
- FailedRows (List<FailedRowDto>)
  - RowNumber (int)
  - Reason (string)
```

#### 4. Add Upload Endpoint to ProductsController

**Endpoint:** `POST /api/products/upload` [Authorize(Roles="Admin")]

**Logic:**

- Accept IFormFile
- Validate file extension (.xlsx)
- Call ExcelService.ParseProductsFromExcelAsync
- Return ExcelUploadResultDto

#### 5. Register Excel Service

**Action:** Add to Program.cs DI container

```csharp
builder.Services.AddScoped<IExcelService, ExcelService>();
```

---

### Part B: ASQL Parser (Advanced Filtering)

#### 6. Create ASQL Parser Service (2 files)

**Location:** `Service/Interface/IAsqlParserService.cs` and `Service/Implementation/AsqlParserService.cs`

**Method to implement:**

```csharp
IQueryable<Product> ApplyAsqlFilter(IQueryable<Product> query, string asqlQuery)
```

**Supported Operators:**

- `=` (equals)
- `!=` (not equals)
- `<` (less than)
- `<=` (less than or equal)
- `>` (greater than)
- `>=` (greater than or equal)
- `in` (in array)

**Supported Logical Operators:**

- `AND`
- `OR`

**Example Queries:**

```
productId=1
name="Vintage Watch"
category="Electronics" AND startingPrice>1000
category!="Fashion"
startingPrice<5000
category in ["Electronics", "Art", "Fashion"]
productId=1 OR name="Vintage Watch"
```

**Implementation Approach:**

- Parse ASQL string into tokens
- Build expression tree
- Convert to LINQ Where clauses
- Apply to IQueryable

#### 7. Update ProductService

**Action:** Modify GetAllProductsAsync to use ASQL

**Current:**

```csharp
Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? status, string? category, decimal? minPrice, decimal? maxPrice)
```

**Update to:**

```csharp
Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? asql)
```

**Logic:**

- If asql is provided → use AsqlParserService
- If asql is null → return all products

#### 8. Update ProductsController

**Action:** Modify GET /api/products endpoint

**Current:**

```csharp
GET /api/products?status=active&category=Electronics&minPrice=100&maxPrice=1000
```

**Update to:**

```csharp
GET /api/products?asql=category="Electronics" AND startingPrice>100
```

#### 9. Register ASQL Service

**Action:** Add to Program.cs DI container

```csharp
builder.Services.AddScoped<IAsqlParserService, AsqlParserService>();
```

---

### Part C: Pagination (Optional but Recommended)

#### 10. Create Pagination Helper

**Location:** `Helpers/PaginationHelper.cs`

**Static method:**

```csharp
PaginatedResponse<T> ApplyPagination<T>(IEnumerable<T> items, int page, int pageSize)
```

#### 11. Create Pagination DTOs

**Location:** `Models/Dtos/Common/PaginatedResponse.cs`

**Properties:**

```csharp
- Items (List<T>)
- TotalCount (int)
- Page (int)
- PageSize (int)
- TotalPages (int)
- HasPrevious (bool)
- HasNext (bool)
```

#### 12. Update ProductsController for Pagination

**Action:** Add optional page and pageSize parameters

**Example:**

```csharp
GET /api/products?asql=category="Electronics"&page=1&pageSize=10
```

---

## 📝 Detailed Implementation Steps

### Step 1: Excel Upload Service (30 mins)

**Files to create:**

1. `Models/Dtos/Products/ExcelUploadResultDto.cs`
2. `Models/Dtos/Products/FailedRowDto.cs`
3. `Service/Interface/IExcelService.cs`
4. `Service/Implementation/ExcelService.cs`

**ExcelService logic:**

```csharp
1. Validate file extension
2. Open Excel file with EPPlus
3. Read first worksheet
4. Validate headers (Name, StartingPrice, Description, Category, DurationMinutes)
5. Loop through rows (skip header):
   - Read cell values
   - Validate data
   - If valid:
     - Create Product entity
     - Create Auction entity
     - Save to database
     - Increment success count
   - If invalid:
     - Add to failed rows list
     - Log reason
6. Return ExcelUploadResultDto
```

**Validations:**

- StartingPrice > 0
- DurationMinutes between 2 and 1440
- Name not empty
- Category not empty

---

### Step 2: ASQL Parser Service (30 mins)

**Files to create:**

1. `Service/Interface/IAsqlParserService.cs`
2. `Service/Implementation/AsqlParserService.cs`

**Parser logic:**

```csharp
1. Tokenize ASQL string
   - Split by AND/OR (preserve logical operators)
   - Parse each condition

2. Parse condition:
   - Extract: field, operator, value
   - Example: "category="Electronics""
     - field: category
     - operator: =
     - value: Electronics

3. Build LINQ expression:
   - For each condition, create Where clause
   - Combine with AND/OR logic

4. Apply to IQueryable
```

**Simple Implementation:**

- Use string parsing (Split, Contains, etc.)
- Support basic operators first
- Handle quoted strings
- Handle numeric values
- Handle arrays for "in" operator

---

### Step 3: Update ProductsController (10 mins)

**Changes:**

1. Add upload endpoint
2. Update GET /api/products to accept asql parameter
3. Remove old filter parameters (status, category, minPrice, maxPrice)

---

### Step 4: Pagination (Optional - 15 mins)

**Files to create:**

1. `Models/Dtos/Common/PaginatedResponse.cs`
2. `Helpers/PaginationHelper.cs`

**Logic:**

```csharp
1. Calculate total pages
2. Skip to correct page
3. Take pageSize items
4. Return with metadata
```

---

## 🚨 Important Decisions

### Excel Upload

**Keep it simple:**

- No complex validation
- Skip invalid rows (don't fail entire upload)
- Log errors clearly
- Return summary

**Column Names (Case-insensitive):**

- Name
- StartingPrice
- Description
- Category
- DurationMinutes

### ASQL Parser

**Keep it simple:**

- Basic string parsing (no complex parser library)
- Support quoted strings: `"value"`
- Support numbers: `1000`
- Support arrays: `["value1", "value2"]`
- Case-insensitive field names
- Case-sensitive values

**Supported Fields:**

- productId
- name
- category
- startingPrice
- status (from auction)

**Error Handling:**

- Invalid ASQL → return all products (or return error)
- Unknown field → ignore condition
- Invalid operator → ignore condition

---

## 📊 Testing Scenarios

### Excel Upload Tests

**Test 1: Valid Excel File**

```
Create Excel with 3 products:
- Row 1: Valid product
- Row 2: Valid product
- Row 3: Valid product

Expected: SuccessCount=3, FailedCount=0
```

**Test 2: Mixed Valid/Invalid**

```
Create Excel with 5 products:
- Row 1: Valid
- Row 2: Invalid (price = 0)
- Row 3: Valid
- Row 4: Invalid (duration = 2000)
- Row 5: Valid

Expected: SuccessCount=3, FailedCount=2
```

**Test 3: Invalid File Format**

```
Upload .csv or .txt file

Expected: 400 Bad Request
```

**Test 4: Missing Columns**

```
Excel missing "StartingPrice" column

Expected: 400 Bad Request
```

### ASQL Tests

**Test 1: Simple Equality**

```
GET /api/products?asql=category="Electronics"

Expected: Only Electronics products
```

**Test 2: Numeric Comparison**

```
GET /api/products?asql=startingPrice>1000

Expected: Products with price > 1000
```

**Test 3: AND Logic**

```
GET /api/products?asql=category="Art" AND startingPrice>=1000

Expected: Art products with price >= 1000
```

**Test 4: OR Logic**

```
GET /api/products?asql=productId=1 OR productId=2

Expected: Products with ID 1 or 2
```

**Test 5: IN Operator**

```
GET /api/products?asql=category in ["Electronics", "Art"]

Expected: Electronics or Art products
```

**Test 6: NOT EQUAL**

```
GET /api/products?asql=category!="Fashion"

Expected: All products except Fashion
```

---

## ✅ Deliverables Checklist

After Phase 5, we should have:

### Excel Upload:

- [ ] EPPlus package installed
- [ ] ExcelUploadResultDto created
- [ ] IExcelService and ExcelService created
- [ ] POST /api/products/upload endpoint working
- [ ] Can upload valid Excel file
- [ ] Invalid rows are skipped with reasons
- [ ] Returns summary of success/failures
- [ ] Only Admin can upload

### ASQL Parser:

- [ ] IAsqlParserService and AsqlParserService created
- [ ] Supports =, !=, <, <=, >, >= operators
- [ ] Supports AND, OR logical operators
- [ ] Supports "in" operator with arrays
- [ ] GET /api/products?asql=... working
- [ ] Can filter by category, price, name, etc.
- [ ] Handles invalid ASQL gracefully

### Pagination (Optional):

- [ ] PaginatedResponse DTO created
- [ ] PaginationHelper created
- [ ] GET /api/products supports page and pageSize
- [ ] Returns pagination metadata

---

## 🔄 What's NOT in Phase 5

These will be in later phases:

- ❌ Payment flow (Phase 6)
- ❌ Email notifications (Phase 6)
- ❌ Dashboard (Phase 7)
- ❌ Config endpoint (Phase 7)
- ❌ Unit tests (Phase 7)

---

## 📝 Implementation Order

1. **Excel Upload** (30 mins)

   - Create DTOs
   - Create ExcelService
   - Add upload endpoint
   - Test with sample Excel file

2. **ASQL Parser** (30 mins)

   - Create AsqlParserService
   - Implement basic parsing
   - Support all operators
   - Test with various queries

3. **Update ProductsController** (10 mins)

   - Add upload endpoint
   - Update GET endpoint for ASQL
   - Remove old filter parameters

4. **Pagination** (15 mins - Optional)

   - Create DTOs
   - Create helper
   - Update controller

5. **Testing** (15 mins)
   - Test Excel upload
   - Test ASQL queries
   - Test pagination

**Total Time: ~1.5 hours**

---

## 🎯 Success Criteria

Phase 5 is complete when:

1. Admin can upload Excel file with multiple products
2. Invalid rows are skipped and reported
3. Can filter products using ASQL syntax
4. All operators work correctly
5. AND/OR logic works
6. Pagination works (if implemented)

---

Ready to implement Phase 5!

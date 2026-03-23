# 🎯 Netpower.CustomerOrders — Senior Backend Engineer Interview Preparation

> **Role:** Senior Fullstack Engineer (Backend-focused)  
> **Stack:** C# / .NET 8 / ASP.NET Core / EF Core / SQL Server  
> **Prepared:** March 2026

---

## Table of Contents

1. [Solution Overview — What You Built & Why](#1-solution-overview)
2. [Clean Architecture & Project Structure](#2-clean-architecture--project-structure)
3. [Dependency Injection (DI) & Service Lifetimes](#3-dependency-injection--service-lifetimes)
4. [Repository Pattern & Unit of Work](#4-repository-pattern--unit-of-work)
5. [CQRS with MediatR](#5-cqrs-with-mediatr)
6. [Entity Framework Core — Advanced Usage](#6-entity-framework-core--advanced-usage)
7. [Async / Await & CancellationToken](#7-async--await--cancellationtoken)
8. [JWT Authentication & Authorization](#8-jwt-authentication--authorization)
9. [Middleware Pipeline](#9-middleware-pipeline)
10. [Input Validation — DataAnnotations + FluentValidation + Pipeline Behavior](#10-input-validation)
11. [GDPR Compliance — Soft Delete, Data Export, Email Masking](#11-gdpr-compliance)
12. [API Design — REST Best Practices](#12-api-design--rest-best-practices)
13. [Pagination & Filtering](#13-pagination--filtering)
14. [SQL Server Indexing & Query Optimization](#14-sql-server-indexing--query-optimization)
15. [Concurrency Control — RowVersion / ETag](#15-concurrency-control)
16. [Unit Testing with xUnit & Moq](#16-unit-testing-with-xunit--moq)
17. [Integration Testing with WebApplicationFactory](#17-integration-testing-with-webapplicationfactory)
18. [Structured Logging with Serilog](#18-structured-logging-with-serilog)
19. [Security Headers & OWASP Best Practices](#19-security-headers--owasp)
20. [Docker & Containerization](#20-docker--containerization)
21. [C# Language Features Used](#21-c-language-features)
22. [Design Decisions — Trade-offs You Should Be Ready to Defend](#22-design-decisions--trade-offs)

---

## 1. Solution Overview

### Q: Walk me through what you built and why.

**Answer:**

I built **Netpower.CustomerOrders**, an enterprise-grade SaaS backend API for managing customers and orders. It targets **.NET 8** (latest LTS) and follows **Clean Architecture** with four source projects:

| Project | Layer | Responsibility |
|---------|-------|----------------|
| `Netpower.CustomerOrders.Api` | Presentation | Controllers, middleware, authentication, HTTP pipeline |
| `Netpower.CustomerOrders.Application` | Application | Business logic, services, CQRS handlers, DTOs, validation |
| `Netpower.CustomerOrders.Domain` | Domain | Entities, enums, business rules — zero external dependencies |
| `Netpower.CustomerOrders.Infrastructure` | Infrastructure | EF Core DbContext, repositories, data access |

Plus two test projects (`UnitTests`, `IntegrationTests`).

**Why this structure:** It enforces a strict dependency rule — outer layers depend on inner layers, never the reverse. The Domain layer has no knowledge of databases or HTTP. This makes the business logic testable and portable.

**Key capabilities:**
- Full CRUD for Customers, order retrieval with server-side filtering & pagination
- JWT-based authentication with role-based authorization
- GDPR-compliant soft deletes, data export, email masking in logs
- Comprehensive validation (DataAnnotations + FluentValidation + MediatR pipeline)
- 37 unit tests, 4 integration tests
- Dockerized with multi-stage Dockerfile and Docker Compose (API + SQL Server)

---

## 2. Clean Architecture & Project Structure

### Q: Why did you choose Clean Architecture? What problem does it solve?

**Answer:**

Clean Architecture separates concerns into concentric layers with a strict dependency rule: **dependencies point inward**. In my solution:

- **Domain** is the center — pure C# classes (`Customers`, `Orders`, `OrderStatus` enum), no NuGet references.
- **Application** defines interfaces (`ICustomerRepository`, `ICustomerService`) and contains all business logic. It *depends on Domain* but never on Infrastructure.
- **Infrastructure** *implements* the Application interfaces using EF Core. The Application layer does not know EF Core exists.
- **API** wires everything together via DI and handles HTTP concerns only.

**Concrete benefit in my code:** `CustomerService` depends on `ICustomerRepository` (defined in Application). The actual `CustomerRepository` using `AppDbContext` lives in Infrastructure. I could swap EF Core for Dapper or a NoSQL driver without touching a single line of business logic.

### Q: How does the dependency flow work? Show me an example from your code.

**Answer:**

When a `GET /api/customers/{id}` request arrives:

1. **`CustomersController`** (API layer) receives it and calls `_customerService.GetByIdAsync(id, ct)`
2. **`CustomerService`** (Application layer) calls `_repo.GetByIdAsync(id, ct)` — where `_repo` is `ICustomerRepository`
3. **`CustomerRepository`** (Infrastructure layer) calls `_db.Customers.FirstOrDefaultAsync(x => x.Id == id, ct)` against EF Core
4. The service maps the entity to a `CustomerDto` using a private `Map()` method and returns it
5. The controller returns `Ok(customer)` or `NotFound()`

Each layer only knows about the layer directly below it, and only through interfaces.

---

## 3. Dependency Injection & Service Lifetimes

### Q: How did you register your services? Why `Scoped` vs `Transient` vs `Singleton`?

**Answer:**

In `Program.cs`:

```csharp
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
```

- `AddScoped` - creates a **scoped** instance of the service within the lifespan of a request. Ideal for repositories and services that use EF Core, as it matches the DbContext lifespan.
- `AddTransient` - creates a **new** instance each time it is requested. Typically used for lightweight, stateless services.
- `AddSingleton` - creates a single instance for the lifetime of the application. Use with caution; not suitable for services with scoped dependencies.

**Why Scoped for repositories and services:** EF Core's `DbContext` is scoped by default — one instance per HTTP request. Repositories and services that depend on it must also be scoped to share the same `DbContext` instance. This ensures the Unit of Work pattern works correctly (one `SaveChangesAsync()` commits all changes in the request).

**Why Transient for middleware:** `ExceptionHandlingMiddleware` implements `IMiddleware` (factory-activated). It has no per-request state, so transient is appropriate.

### Q: What is the "captive dependency" problem and how does your code avoid it?

**Answer:**

A captive dependency occurs when a **Singleton** service holds a reference to a **Scoped** service. The scoped service is never disposed, causing memory leaks and stale data. My code avoids this because all database-touching components are registered as **Scoped**, and no Singleton in the pipeline depends on them.

---

## 4. Repository Pattern & Unit of Work

### Q: Why use the Repository pattern when you already have EF Core (which is itself a repository + UoW)?

**Answer:**

Three reasons specific to my project:

1. **Testability:** `CustomerService` depends on `ICustomerRepository` — I mock it with Moq in my 26 unit tests. Without the interface, I'd need an in-memory DbContext or a real database for every test.

2. **Abstraction over query complexity:** `OrderRepository.GetByCustomerAsync()` encapsulates a complex dynamic query (filter by status, date range, pagination) and projects directly to `OrderDto`. The service layer never deals with IQueryable.

3. **Enforced consistency:** Soft-delete logic (checking `IsDeleted`) is centralized. The controller never accidentally queries deleted records directly.

**Example from my code:**

````````csharp
// In Application layer
public interface ICustomerRepository {
    Task<CustomerDto> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(Customer customer, CancellationToken ct);
    // ... other members ...
}

// In Infrastructure layer
public class CustomerRepository : ICustomerRepository {
    private readonly AppDbContext _db;
    public CustomerRepository(AppDbContext db) { _db = db; }

    public async Task<CustomerDto> GetByIdAsync(int id, CancellationToken ct) {
        var customer = await _db.Customers.FindAsync(new object[] { id }, ct);
        return customer == null ? null : new CustomerDto { Id = customer.Id, Name = customer.Name };
    }

    public async Task AddAsync(Customer customer, CancellationToken ct) {
        await _db.Customers.AddAsync(customer, ct);
        // No SaveChangesAsync here
    }
}
````````

The repository exposes `SaveChangesAsync()` which acts as the Unit of Work commit. The service calls `AddAsync()` then `SaveChangesAsync()` — both happen within a single DbContext scope, ensuring atomicity.

---

## 5. CQRS with MediatR

### Q: You use MediatR for orders but not for customers — why the inconsistency? What is CQRS?

**Answer:**

**CQRS (Command Query Responsibility Segregation)** separates read and write models. I used MediatR for the orders **read path** because it demonstrates a more advanced pattern where the query definition, validation, and handling are all separated:

```csharp
// Query
public class GetOrdersByCustomerQuery : IRequest<List<OrderDto>> {
    public int CustomerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

// Handler
public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, List<OrderDto>> {
    private readonly IOrderRepository _repo;
    public GetOrdersByCustomerQueryHandler(IOrderRepository repo) { _repo = repo; }

    public async Task<List<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken) {
        var query = _repo.GetAll() // IQueryable
            .Where(o => o.CustomerId == request.CustomerId);

        if (request.StartDate.HasValue) {
            query = query.Where(o => o.OrderDate >= request.StartDate.Value);
        }
        if (request.EndDate.HasValue) {
            query = query.Where(o => o.OrderDate <= request.EndDate.Value);
        }

        var orders = await query
            .Select(o => new OrderDto { Id = o.Id, OrderDate = o.OrderDate, Status = o.Status })
            .ToListAsync(cancellationToken);

        return orders;
    }
}
```

The **write path** for orders uses standard commands (Create, Update, Delete) without MediatR, as they are simple and do not require the same level of complexity.

---

## 6. Entity Framework Core — Advanced Usage

### Q: What EF Core optimization techniques did you implement?

**Answer:**

1. **`AsNoTracking()` for read-only queries:** Disabled change tracking for queries that don't require it, improving performance for read-only scenarios.

    ```csharp
    var customers = await _db.Customers
        .AsNoTracking()
        .ToListAsync();
    ```

2. **Batching and transactions:** Combined multiple inserts or updates into a single batch to reduce round-trips to the database.

    ```csharp
    using var transaction = await _db.Database.BeginTransactionAsync();
    await _db.Customers.AddRangeAsync(customers);
    await transaction.CommitAsync();
    ```

3. **Compiled queries:** Used compiled queries for frequently executed queries with different parameters, reducing query planning time.

    ```csharp
    private static readonly Func<AppDbContext, int, CancellationToken, Task<Customer>> _compiledGetCustomerById =
        EF.CompileAsyncQuery((AppDbContext db, int id, CancellationToken ct) =>
            db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct));
    ```

4. **NoTracking entities** in `DbContext`: Globally configured entities as `NoTracking` by default to optimize read performance.

    ```csharp
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Customer>().Metadata.SetIsInstrumented(false);
    }
    ```

5. **Query splitting:** Forcing query splitting to avoid Cartesian explosion when including related data.

    ```csharp
    var customers = await _db.Customers
        .Include(c => c.Orders)
        .AsSplitQuery() // Split into multiple queries
        .ToListAsync();
    ```

6. **Filtered indexes:** Used filtered indexes to load related data conditionally.

    ```csharp
    var customers = await _db.Customers
        .Include(c => c.Orders.Where(o => o.Status == OrderStatus.Shipped))
        .ToListAsync();
    ```

7. **Custom repository methods for complex queries:** Moved complex or performance-critical queries to custom repository methods to optimize execution.

    ```csharp
    public async Task<List<CustomerDto>> GetActiveCustomersWithLargeOrders() {
        return await _db.Customers
            .Where(c => c.IsActive && c.Orders.Count() > 10)
            .Select(c => new CustomerDto { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }
    ```

    - Challenge: Continuously monitoring and profiling the database performance to identify and optimize slow queries.

### Q: How did you handle database seeding? Any complex logic?

**Answer:**

Database seeding is handled in `AppDbContextSeed` class:

- Checks for existing data before seeding to avoid duplicates
- Seeds reference data (e.g., `OrderStatus` values) that rarely changes
- **Performs one-time dataset migrations** — applies only on development or staging databases

**Example - seeding logic:**
```csharp
public static async Task SeedAsync(AppDbContext context) {
    if (context.OrderStatuses.Any()) return; // Already seeded

    var statuses = new List<OrderStatus> {
        new OrderStatus { Name = "Pending" },
        new OrderStatus { Name = "Shipped" }
    };

    context.OrderStatuses.AddRange(statuses);
    await context.SaveChangesAsync();
}
```

---

## 7. Async / Await & CancellationToken

### Q: Why is everything async in your codebase? When would you NOT use async?

**Answer:**

Every I/O-bound operation (database queries, token generation) is async because it frees the calling thread back to the thread pool while waiting for the database response. Under load, this means an ASP.NET Core server can handle thousands of concurrent requests without thread exhaustion.

In my code, `CancellationToken` is threaded through every layer:

````````csharp
public class CustomersController : ControllerBase {
    private readonly ICustomerService _service;

    public CustomersController(ICustomerService service) {
        _service = service;
    }

    // GET: api/customers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id, CancellationToken ct) {
        var customer = await _service.GetByIdAsync(id, ct);
        if (customer == null) return NotFound();
        return customer;
    }
}
````````

If a client disconnects mid-request, the token is cancelled, and EF Core aborts the SQL command — preventing wasted database resources.

**When NOT to use async:** CPU-bound work (e.g., sorting an in-memory list of 50 items, hashing a password). Making those async adds overhead (state machine allocation) without benefit.

### Q: What is the danger of using `.Result` or `.Wait()`?

**Answer:**

In ASP.NET Core, calling `.Result` or `.Wait()` on an async method can cause a **deadlock** in certain synchronization contexts. Even in ASP.NET Core (which uses a threadpool-based `SynchronizationContext`), it blocks a thread pool thread, reducing throughput. My code consistently uses `await` throughout — no `.Result` or `.Wait()` anywhere.

---

## 8. JWT Authentication & Authorization

### Q: Walk me through your JWT implementation.

**Answer:**

**Token Generation (`JwtTokenService.GenerateToken`):**

1. Read the secret key from `IOptions<JwtSettings>` (Options pattern, bound to `appsettings.json`)
2. Create a `SymmetricSecurityKey` from the key bytes
3. Build claims: `NameIdentifier` (userId), `Email`, `Role` (multiple), `aud`
4. Create a `JwtSecurityToken` with issuer, audience, claims, expiration (`DateTime.UtcNow.AddMinutes(60)`), and HMAC-SHA256 signing credentials
5. Serialize to a compact JWT string

**Token Validation (ASP.NET Core middleware):**

Configured in `Program.cs` via `AddJwtBearer()`:

```csharp
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        // ... other parameters ...
    };
});
```

**`ClockSkew = TimeSpan.Zero`** is a deliberate choice. The default is 5 minutes, which means a token is valid for 5 minutes *after* expiry. I set it to zero for tighter security.

**Authorization:** The `[Authorize]` attribute on `CustomersController` blocks unauthenticated requests. The middleware pipeline order matters:

---

## 9. Middleware Pipeline

### Q: Explain your middleware architecture and why the order matters.

**Answer:**

My pipeline (in execution order):

1. **ExceptionHandlingMiddleware** — global exception handler, logs errors, returns standardized error responses
2. **RequestLoggingMiddleware** — logs incoming requests and responses
3. **ResponseTimeMiddleware** — measures and logs how long requests take
4. **AuthenticationMiddleware** — populates `User` from the JWT token
5. **AuthorizationMiddleware** — checks permissions based on policies

**Why order matters:** Each middleware component does its job and hands off to the next. Some middleware need to run before others (e.g., authentication before authorization). The order also affects performance and resource usage.

**Custom middleware example - RequestLoggingMiddleware:**
```csharp
public class RequestLoggingMiddleware {
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger) {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context) {
        var path = context.Request.Path;
        var method = context.Request.Method;
        _logger.LogInformation("Received {Method} request for {Path}", method, path);

        // Call the next middleware in the pipeline
        await _next(context);

        _logger.LogInformation("Response {StatusCode} for {Method} request to {Path}", context.Response.StatusCode, method, path);
    }
}
```

---

## 10. Input Validation

### Q: You have both DataAnnotations and FluentValidation — isn't that redundant?

**Answer:**

They serve different layers:

**DataAnnotations** (`CreateCustomerRequest`) — edge validation at the API boundary:
```csharp
public class CreateCustomerRequest {
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
```

ASP.NET Core model binding automatically validates these before the controller action executes. Fast-fails with 400 for malformed input.

**FluentValidation** (`GetCustomerOrdersQueryValidator`) — business rule validation in the Application layer:
```csharp
public class GetCustomerOrdersQueryValidator : AbstractValidator<GetCustomerOrdersQuery> {
    public GetCustomerOrdersQueryValidator() {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.StartDate).LessThanOrEqualTo(x => x.EndDate);
    }
}
```

These run inside the MediatR `ValidationBehavior` pipeline — validating the CQRS query *before* it reaches the handler.

**Why both:** DataAnnotations catch syntactic issues (missing fields, bad formats). FluentValidation catches semantic issues (date range logic, cross-property rules, business constraints). Defense in depth.

---

## 11. GDPR Compliance

### Q: What GDPR features did you implement and why?

**Answer:**

| Feature | Implementation | GDPR Article |
|---------|---------------|--------------|
| **Soft Delete** | `IsDeleted` flag + `DeletedAtUtc` timestamp | Art. 17 (Right to Erasure) — reversible |
| **Data Export** | `GET /api/customers/{id}/export` returns JSON | Art. 20 (Right to Data Portability) |
| **Email Masking in Logs** | `MaskEmail()` method in controller | Art. 25 (Data Protection by Design) |
| **Audit Timestamps** | `CreatedAtUtc`, `UpdatedAtUtc`, `CreatedBy`, `UpdatedBy` on every entity | Art. 30 (Records of Processing) |
| **Data Retention Config** | `GdprSettings.DataRetentionDays = 2555` (≈7 years) | Art. 5(1)(e) (Storage Limitation) |

### Q: Why soft delete instead of hard delete?

**Answer:**

Soft delete allows for data recovery and auditing (compliance with GDPR Art. 17). It avoids the complexity and performance cost of maintaining a separate audit table or entity history.

Hard delete would simplify the model and potentially improve performance (no deleted records to filter), but at the cost of violating GDPR requirements and losing historical data.

**My choice:** Soft delete, as implemented by overriding `SaveChanges()` in `AppDbContext`. When `SaveChanges()` detects a delete operation, it instead sets the `IsDeleted` flag and updates the `DeletedAtUtc` timestamp.

1. **Audit trail preserved** — regulators can verify what data existed and when it was "deleted"
2. **Recoverability** — accidental deletions can be undone
3. **Referential integrity** — orders referencing the customer don't become orphans
4. **Data retention** — the record stays for the legal retention period, then a scheduled job can hard-delete

Filtered indexes (`HasFilter("([IsDeleted]=(0))")`) ensure soft-deleted rows don't degrade query performance for active records.

### Q: Show me how email masking works.

**Answer:**

```csharp
// In Controllers
[HttpPost]
public async Task<ActionResult<CustomerDto>> Create(CustomerDto dto, CancellationToken ct) {
    var customer = new Customer { Name = dto.Name, Email = dto.Email };
    await _service.AddAsync(customer, ct);
    _logger.LogInformation("Customer created {@Customer}", customer.MaskEmail());
    return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
}

// Customer.cs (Domain)
public class Customer {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public CustomerDto MaskEmail() {
        var parts = Email.Split('@');
        if (parts.Length != 2) return null;
        return new CustomerDto { Id = Id, Name = Name, Email = $"{parts[0].First()}*****@{parts[1]}" };
    }
}
```

---

## 12. API Design — REST Best Practices

### Q: How does your API follow REST conventions?

**Answer:**

| Convention | Example in My Code |
|---|---|
| Resource-based URLs | `/api/customers`, `/api/customers/{id}` |
| HTTP verbs for actions | `GET` (read), `POST` (create), `PUT` (update), `DELETE` (soft delete) |
| Proper status codes | `200 OK`, `201 Created`, `204 No Content`, `400 Bad Request`, `401 Unauthorized`, `404 Not Found` |
| Location header on POST | `CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer)` |
| Sub-resource routing | `/api/customers/{customerId}/orders` — orders scoped under their customer |
| Route constraints | `{id:guid}` — rejects non-GUID values at the routing level |
| Content negotiation | `[Produces("application/json")]` |
| `[ProducesResponseType]` | Documents every possible response for Swagger/OpenAPI |

### Q: Why `CreatedAtAction` instead of just `Ok`?

**Answer:**

`CreatedAtAction` returns HTTP `201 Created` with a `Location` header pointing to the new resource:

````````markdown
Location: /api/customers/123
```

This follows RFC 7231 and lets clients discover the new resource URI without hardcoding paths.

---

## 13. Pagination & Filtering

### Q: Explain your pagination strategy.

**Answer:**

I use **offset-based pagination** with `PageNumber` and `PageSize`:

````````markdown
GET /api/customers?pageNumber=2&pageSize=10

Response:
{
  "pageNumber": 2,
  "pageSize": 10,
  "totalRecords": 50,
  "data": [ /* array of customer records */ ]
}
```

- `pageNumber` (1-based) and `pageSize` specify the window of records
- Enforced by middleware: filters out records where `RowVersion` ≤ client's `LastRowVersion`
- Clients send their `LastRowVersion` in the request; the server responds with the next set of records
- Lightweight: no need for `COUNT` queries or complex state management
- Leverages indexed columns for efficient lookups

**Key design decisions:**
- **Max page size enforced server-side:** `if (pageSize > 200) pageSize = 200;` — protects against clients requesting 1M rows
- **Stable sort order:** `ThenByDescending(o => o.Id)` prevents items appearing on two pages when `OrderDateUtc` has ties
- **Separate count query:** `CountByCustomerAsync()` runs first. If `total == 0`, the items query is skipped entirely
- **Date range normalization:** If `fromUtc > toUtc`, the controller swaps them — graceful handling of user mistakes

### Q: What's the downside of offset-based pagination? What alternative exists?

**Answer:**

**Offset pagination** (`SKIP N`) becomes slow on large datasets because the database still scans `N` rows to skip them.

**Keyset (cursor) pagination** is the alternative: `WHERE OrderDateUtc < @lastSeenDate ORDER BY OrderDateUtc DESC TAKE 20`. It uses an index seek instead of a scan. I chose offset for this project because the order count per customer is manageable (thousands, not millions), and offset gives a familiar page-number UX.

---

## 14. SQL Server Indexing & Query Optimization

### Q: Walk me through your indexing strategy.

**Answer:**

| Index | Type | Purpose |
|---|---|---|
| `UQ_Customers_Email` | Unique | Prevent duplicate emails |
| `UQ_Customers_CustomerNumber` | Unique | Business key uniqueness |
| `IX_Customers_Email_IsDeleted` | Filtered (`IsDeleted=0`) | Fast email lookup for active customers only |
| `IX_Orders_CustomerId_OrderDateUtc` | Composite, filtered, descending | Covers the primary query: "get orders for customer X, newest first" |
| `IX_Orders_Status_OrderDateUtc` | Composite, filtered, descending | Covers status-based filtering |
| `UQ_Orders_OrderNumber` | Unique | Business key uniqueness |

**Why filtered indexes:** Only active records are indexed. If 30% of customers are soft-deleted, the index is 30% smaller and faster. SQL Server uses filtered indexes when the query's `WHERE` clause matches the filter predicate.

**Why descending on `OrderDateUtc`:** The most common sort is newest-first (`ORDER BY OrderDateUtc DESC`). A descending index avoids a sort operation in the execution plan.

---

## 15. Concurrency Control

### Q: What is `RowVersion` and why did you add it?

**Answer:**

`RowVersion` is a SQL Server `ROWVERSION` / `TIMESTAMP` column — an 8-byte value that auto-increments every time the row is modified.

**How it works:**
1. User A reads Customer with `RowVersion = 0x000001`
2. User B reads the same Customer with `RowVersion = 0x000001`
3. User A saves changes — SQL Server increments to `0x000002`
4. User B tries to save — EF Core sends `UPDATE ... WHERE Id = @Id AND RowVersion = 0x000001`. The row doesn't match (it's now `0x000002`), so `DbUpdateConcurrencyException` is thrown

This is **optimistic concurrency control** — no locks are held between reads and writes. It's ideal for web APIs where requests are short-lived and conflicts are rare.

---

## 16. Unit Testing with xUnit & Moq

### Q: Describe your testing strategy. Show me an example.

**Answer:**

**26 unit tests** cover `CustomerService` with the **Arrange-Act-Assert** pattern:

````````csharp
public class CustomerServiceTests {
    private readonly Mock<ICustomerRepository> _repo = new Mock<ICustomerRepository>();
    private readonly CustomerService _service;

    public CustomerServiceTests() {
        _service = new CustomerService(_repo.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCustomer() {
        // Arrange
        var customer = new Customer { Id = 1, Name = "John" };
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        // Act
        var result = await _service.GetByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.Equal("John", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull() {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Customer)null);

        // Act
        var result = await _service.GetByIdAsync(99, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
````````

- **Easy setup of mocks and dependencies** using constructors
- **Isolation** from infrastructure concerns (e.g., database)
- **Fast feedback** — tests run in < 500ms

**Testing categories:**
- **Happy path:** Create, Get, Update, Delete all succeed
- **Not found:** Returns `null` or `false` for non-existent IDs
- **Soft-deleted records:** Treated as if they don't exist (returns `null` / `false`)
- **Edge cases:** Whitespace trimming, already-deleted records
- **Interaction verification:** `_mockRepository.Verify()` ensures `SaveChangesAsync` is called (or NOT called) exactly when expected

### Q: Why use `_sut` as a variable name?

**Answer:**

`_sut` stands for **System Under Test** — a testing convention that makes it immediately clear which object is being tested vs. which are mocked dependencies. It's a widely recognized pattern in the .NET testing community.

### Q: What is FluentAssertions and why use it over `Assert.Equal()`?

**Answer:**

FluentAssertions provides expressive, readable assertions:
````````csharp
result.Should().BeEquivalentTo(expected);
````````

Better failure messages too: `"Expected result to be true, but found false"` vs `"Assert.True() Failure"`.

---

## 17. Integration Testing with WebApplicationFactory

### Q: How do your integration tests work?

**Answer:**

I use `WebApplicationFactory<Program>` to spin up the entire ASP.NET Core pipeline in-process:

````````
public class CustomersControllerTests : IClassFixture<WebApplicationFactory<Program>> {
    private readonly HttpClient _client;
    private readonly string _baseUrl = "/api/customers";

    public CustomersControllerTests(WebApplicationFactory<Program> factory) {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetById_ReturnsCustomer() {
        // Arrange
        var customer = new CustomerDto { Name = "Jane", Email = "jane@example.com" };
        var postResponse = await _client.PostAsJsonAsync(_baseUrl, customer);
        postResponse.EnsureSuccessStatusCode();
        var customerId = (await postResponse.ReadAsJsonAsync<CustomerDto>()).Id;

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/{customerId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var retrievedCustomer = await response.ReadAsJsonAsync<CustomerDto>();
        Assert.Equal("Jane", retrievedCustomer.Name);
    }

    [Fact]
    public async Task Create_Customer_SavesToDatabase() {
        // Arrange
        var initialCount = await GetCustomersCount();

        var customer = new CustomerDto { Name = "New Customer", Email = "new@example.com" };

        // Act
        var response = await _client.PostAsJsonAsync(_baseUrl, customer);

        // Assert
        response.EnsureSuccessStatusCode();
        var savedCustomer = await response.ReadAsJsonAsync<CustomerDto>();
        Assert.NotEqual(0, savedCustomer.Id);

        var finalCount = await GetCustomersCount();
        Assert.Equal(initialCount + 1, finalCount);
    }

    private async Task<int> GetCustomersCount() {
        var response = await _client.GetAsync(_baseUrl + "/count");
        response.EnsureSuccessStatusCode();
        return await response.ReadAsJsonAsync<int>();
    }
}
````````

- **Real HTTP requests**: Tests the entire stack (routing, model binding, filters, controller)
- **Database hooked up**: Uses the real database context, ensuring schema and configuration are tested
- **Custom factory `NetpowerFactory`**: Overrides `CreateHost` to use the test database

**Key design decisions:**

1. **Real SQL Server (`Netpower_IT`)** — not in-memory. This catches SQL-specific issues (data types, indexes, constraints) that in-memory providers miss.

2. **Safety guard:** `CreateHost()` verifies the database name is `Netpower_IT` — prevents accidental test runs against production:

    ```csharp
    protected override IHostBuilder CreateHostBuilder(string[] args) {
        return base.CreateHostBuilder(args)
            .ConfigureServices(services => {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) {
                    services.Remove(descriptor);
                }

                // Register the test DbContext with the correct database name
                services.AddDbContext<AppDbContext>(options => {
                    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Netpower_IT;Trusted_Connection=True;");
                });
            });
    }
    ```

    - Throws an exception on startup if the database name is not `Netpower_IT`
    - Ensures all tests run against the local development database

---

## 18. Structured Logging with Serilog

### Q: Why Serilog over the built-in `ILogger`?

**Answer:**

Serilog supports **structured logging** — log parameters are preserved as named properties, not just interpolated into strings:

```csharp
_log.LogInformation("Customer created {@Customer} by {User}", customer, _httpContextAccessor.HttpContext.User.Identity.Name);
```

This produces a log entry where `UserId` and `CustomerId` are queryable fields. In a log aggregator (Seq, Elastic, Application Insights), you can filter by `UserId = "admin@test.com"` without regex.

**Serilog is configured at the top of `Program.cs`** — before the host builder, so even startup errors are captured:
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs\\Netpower.CustomerOrders.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

- `MinimumLevel.Debug()` - logs all events from Debug level and above
- `Enrich.FromLogContext()` - adds contextual information from the `LogContext`
- `WriteTo.Console()` - logs to the console
- `WriteTo.File()` - logs to a file, rolling over daily

`appsettings.json` configures additional settings:

```json
"Serilog": {
  "MinimumLevel": "Information",
  "WriteTo": [
    {
      "Name": "Console",
      "Args": {
        "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] ({RequestId}) {Message}{NewLine}{Exception}"
      }
    },
    {
      "Name": "File",
      "Args": {
        "path": "logs/log-.txt",
        "rollingInterval": "Day"
      }
    }
  ]
}
```

---

## 19. Security Headers & OWASP Best Practices

### Q: What security headers did you implement and why?

**Answer:**

| Header | Value | Attack Prevented |
|--------|-------|-----------------|
| `X-Frame-Options` | `DENY` | Clickjacking |
| `X-Content-Type-Options` | `nosniff` | MIME-type sniffing |
| `X-XSS-Protection` | `1; mode=block` | Reflected XSS (legacy browsers) |
| `Content-Security-Policy` | `default-src 'self'; ...` | XSS, data injection |
| `Referrer-Policy` | `strict-origin-when-cross-origin` | Information leakage via Referer header |
| `Permissions-Policy` | `geolocation=(), microphone=(), camera=()` | Unauthorized feature access |
| `Strict-Transport-Security` | `max-age=31536000; includeSubDomains` | Downgrade attacks (HTTPS → HTTP) |

These are added via inline middleware (`app.Use(...)`) so they apply to **every response**, including error responses and static files.

### Q: How did you prevent SQL injection?

**Answer:**

EF Core uses **parameterized queries** by default. My code never constructs raw SQL strings. All LINQ expressions:

- Are safe by design, as they generate SQL parameters for values
- Are automatically escaped, preventing injection attacks

Are translated to parameterized SQL: `SELECT ... FROM Customers WHERE Id = @p0`.

---

## 20. Docker & Containerization

### Q: Explain your Dockerfile. Why multi-stage?

**Answer:**

My Dockerfile has 4 stages:

| Stage | Base Image | Purpose |
|---|---|---|
| `base` | `aspnet:8.0` (~220 MB) | Runtime only — no SDK |
| `build` | `sdk:8.0` (~900 MB) | Restore + build |
| `publish` | (extends build) | `dotnet publish` — trimmed output |
| `final` | (extends base) | Copy published files, run as non-root |

**Why multi-stage:** The SDK image is 4x larger than the runtime image. Multi-stage keeps the final image small (~220 MB vs ~900 MB) because the SDK is discarded.

**Security:** The container runs as `appuser` (UID 1001), not root — follows the principle of least privilege.

### Q: How does Docker Compose orchestrate the API and SQL Server?

**Answer:**

- SQL Server starts first with a **health check** (`sqlcmd SELECT 1`)
- API container has `depends_on: sqlserver: condition: service_healthy`
- Secrets are injected via `.env` file (not baked into the image)
- SQL Server data is persisted in a named Docker volume (`sqlserver_data`)

---

## 21. C# Language Features

### Q: What modern C# features does your code use?

**Answer:**

| Feature | Example | C# Version |
|---------|---------|------------|
| **Records** | `public sealed record GetCustomerOrdersQuery(...)` | C# 9 |
| **Records** | `public sealed record PagedResult<T>(...)` | C# 9 |
| **Records** | `public sealed record OrderDto(...)` | C# 9 |
| **`init` accessors** | `public string FirstName { get; init; }` (DTOs) | C# 9 |
| **Pattern matching** | `if (customer is null or { IsDeleted: true })` | C# 9 |
| **`is not null`** | `.Where(f => f is not null)` | C# 9 |
| **File-scoped namespaces** | `namespace Netpower.CustomerOrders.Domain.Entities;` | C# 10 |
| **`sealed` classes** | Every service, handler, validator, controller, test class | Best practice |
| **Tuple deconstruction** | `(customerId, _) = await TestDataSeeder.SeedAsync(db)` | C# 7 |
| **Tuple assignment in ctor** | `(_service, _logger) = (service, logger)` | C# 7 |
| **Expression-bodied members** | `public CustomerService(IRepo repo) => _repo = repo;` | C# 6 |
| **`null!` forgiveness** | `public string Email { get; set; } = null!;` | C# 8 |
| **Nullable reference types** | `public string? Phone { get; set; }` | C# 8 |
| **Target-typed new** | `= new List<Orders>()` on navigation properties | C# 9 |
| **Raw string interpolation** | Date range swap: `(fromUtc, toUtc) = (toUtc, fromUtc)` | C# 7 |

### Q: Why `sealed` on almost every class?

**Answer:**

1. **Performance:** The JIT compiler can devirtualize method calls on sealed classes, enabling inlining
2. **Design intent:** These classes are not designed for inheritance. Making them sealed communicates that clearly
3. **Security:** Prevents subclasses from overriding behavior in unexpected ways (e.g., a mock service in production)

---

## 22. Design Decisions — Trade-offs You Should Be Ready to Defend

### Q: What would you do differently if you started over?

**Suggested answers (honest, shows maturity):**

1. **Standardize on MediatR everywhere** — mixing service classes (customers) and MediatR handlers (orders) is inconsistent. I'd use MediatR for all commands/queries for uniform cross-cutting concerns.

2. **Use `record` for DTOs consistently** — `OrderDto` is a record, but `CustomerDto` is a class with `init` properties. Records give you value-based equality and `ToString()` for free.

3. **Global query filters for soft deletes** — instead of checking `IsDeleted` in every service method, configure EF Core:

````````
modelBuilder.Entity<Customer>()
    .HasQueryFilter(c => !c.IsDeleted);
```

This automatically excludes deleted records from all queries.

4. **Move JWT settings to Azure Key Vault** — the secret key is in `appsettings.json`, which is fine for development but not for production.

5. **Add API versioning** — when the API evolves, versioning (`/api/v1/customers`) prevents breaking existing clients.

### Q: Why GUIDs for primary keys instead of auto-increment integers?

**Answer:**

- **Mergeability:** GUIDs are globally unique — multiple services or databases can generate IDs without coordination
- **Security:** Sequential integers are predictable (`/api/customers/1`, `/api/customers/2`). GUIDs are not enumerable
- **Client-generated:** The service creates the GUID before `SaveChangesAsync()`, so it can return the ID immediately without a database round-trip
- **Downside:** GUIDs are 16 bytes vs 4 bytes for int. They cause page fragmentation on clustered indexes. Mitigation: `ValueGeneratedNever()` prevents sequential GUID generation issues, and consider `NEWSEQUENTIALID()` for clustered index performance

---

## 📋 Quick-Fire Questions You May Get

| Question | One-liner Answer |
|---|---|
| What's the difference between `AddScoped` and `AddTransient`? | Scoped = one instance per HTTP request. Transient = new instance every time it's injected. |
| What's `IMiddleware` vs convention-based middleware? | `IMiddleware` is factory-activated from DI (can inject scoped services). Convention-based is singleton by default. |
| What's `AsNoTracking()` do? | Tells EF Core not to track entities in the Change Tracker — read-only queries are faster and use less memory. |
| What is `CancellationToken`? | A token that signals an operation should be cancelled (e.g., client disconnects). Prevents wasted server resources. |
| What's a filtered index? | An index with a `WHERE` clause — only rows matching the predicate are indexed. Smaller and faster. |
| What's the Options pattern? | `IOptions<T>` / `IOptionsSnapshot<T>` — strongly-typed access to configuration sections, registered via `Configure<T>()`. |
| What does `sealed` do? | Prevents inheritance. Enables JIT devirtualization. Communicates design intent. |
| What's `CreatedAtAction` return? | HTTP 201 with a `Location` header pointing to the newly created resource. |
| What's a concurrency token? | A column (like `RowVersion`) used to detect conflicting updates. EF Core throws `DbUpdateConcurrencyException` on conflict. |
| What's `IPipelineBehavior`? | MediatR's middleware — intercepts requests before/after the handler (used for validation, logging, etc.). |

---

## 📂 File Location

This document is located at: `Documentations\INTERVIEW_PREP.md`

---

> **Tip:** Before the interview, be ready to **open Visual Studio, navigate to specific files, and explain your code live**. Interviewers for senior roles want to see you reason about code, not recite definitions.

---

**Last Updated:** March 06, 2026

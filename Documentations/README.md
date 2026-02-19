# Netpower.CustomerOrders - Enterprise SaaS Backend API

A modern, production-ready backend API for managing customers and orders with enterprise-grade security, clean architecture, and GDPR compliance.

## 📋 Table of Contents

- [Project Overview](#project-overview)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [API Endpoints](#api-endpoints)
- [Authentication & Security](#authentication--security)
- [GDPR Compliance](#gdpr-compliance)
- [Testing](#testing)
- [Database](#database)
- [Development Guidelines](#development-guidelines)
- [Implementation Status](#implementation-status)
- [Contributing](#contributing)

---

## 🎯 Project Overview

**Netpower.CustomerOrders** is an enterprise-grade SaaS backend platform designed to manage customer relationships and order fulfillment with a focus on:

- ✅ **Clean Architecture** - Separation of concerns across layers
- ✅ **Security First** - JWT authentication, authorization, input validation
- ✅ **GDPR Compliance** - Soft deletes, audit logging, data export, consent management
- ✅ **Performance** - Query optimization, indexing, async operations
- ✅ **Testability** - Comprehensive unit and integration tests
- ✅ **Scalability** - Stateless design, containerization-ready

### Key Features

- **Customer Management** - Create, read, update, soft-delete customers
- **Order Management** - Create orders, filter by status/date, paginated results
- **JWT Authentication** - Secure token-based authentication
- **Role-Based Authorization** - Control access to resources
- **Data Protection** - GDPR-compliant soft deletes and data export
- **Audit Logging** - Track all data modifications
- **Input Validation** - Comprehensive validation with meaningful error messages
- **API Documentation** - Interactive Swagger/OpenAPI interface

---

## 🛠️ Technology Stack

### Backend
- **.NET 8.0** - Latest LTS framework
- **C# 12.0** - Modern language features
- **ASP.NET Core** - Web framework
- **Entity Framework Core 8** - ORM for data access
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Validation library
- **Serilog** - Structured logging

### Database
- **SQL Server 2019+** - Relational database
- **Entity Framework Core Migrations** - Schema versioning

### Testing
- **xUnit** - Test framework
- **Moq** - Mocking library
- **FluentAssertions** - Assertion library
- **WebApplicationFactory** - Integration testing

### Security
- **JWT (JSON Web Tokens)** - Authentication
- **BCrypt** - Password hashing (if applicable)
- **HTTPS** - Transport security
- **CORS** - Cross-origin policies

### DevOps (Planned)
- **Docker** - Containerization
- **GitHub Actions** - CI/CD pipeline
- **SQL Server Docker** - Database containerization

---

## 🏛️ Architecture

### Clean Architecture Pattern

### Dependency Flow
- **Controllers** → Services
- **Services** → Repositories
- **Repositories** → DbContext (EF Core)
- **DbContext** → Database

### Design Patterns Used
- **Repository Pattern** - Abstract data access
- **Dependency Injection** - Loose coupling
- **CQRS** - Separate read/write models (MediatR)
- **DTO Pattern** - Don't expose entities
- **Middleware Pattern** - Cross-cutting concerns

---

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server 2019+ or SQL Server Express
- Visual Studio 2022 or VS Code
- Git

### Installation & Setup

#### 1. Clone the Repository

````````

#### 3. Update JWT Secret Key

⚠️ **SECURITY WARNING**: The default secret key is for development only. Update it immediately:

````````

**Generate a secure key:**

````````

#### 4. Apply Database Migrations

````````
Or using Package Manager Console in Visual Studio:

````````

#### 5. Run the Application

````````

The API will be available at: `https://localhost:7180` (or as configured)

#### 6. Access Swagger Documentation

Navigate to: `https://localhost:7180/swagger/index.html`

## 📁 Project Structure

````````

# API Endpoints

### Customers

All customer endpoints require a valid JWT token in the `Authorization` header:

- **GET** `/api/customers` - Get all customers
- **GET** `/api/customers/{id}` - Get customer by ID
- **POST** `/api/customers` - Create a new customer
- **PUT** `/api/customers/{id}` - Update an existing customer
- **DELETE** `/api/customers/{id}` - Soft-delete a customer

### Orders

- **GET** `/api/orders` - Get all orders
- **GET** `/api/orders/{id}` - Get order by ID
- **POST** `/api/orders` - Create a new order
- **PUT** `/api/orders/{id}` - Update an existing order
- **DELETE** `/api/orders/{id}` - Soft-delete an order

---

## 🔒 Authentication & Security

### Authentication

- **JWT Bearer Tokens** are used for authenticating users.
- The token must be included in the `Authorization` header as a Bearer token.

### Role-Based Access Control (RBAC)

- Users are assigned roles which define their permissions.
- Roles are checked on each request to authorize access to resources.

### HTTPS

- All communications must be done over HTTPS to ensure data in transit is encrypted.

### CORS

- Cross-Origin Resource Sharing (CORS) policies are configured to restrict access to the API.

---

## 📋 GDPR Compliance

Your system is designed with GDPR compliance as a core principle:

### Data Protection Features

#### 1. Soft Delete
All customer records are "soft deleted" - data remains in the database but marked as deleted:

```sql
UPDATE Customers SET IsDeleted = 1 WHERE Id = @Id
```
No data is ever physically deleted from the database.

**Benefit:** Data can be recovered if needed, audit trail preserved.

#### 2. Data Export (Right to Data Portability)
Users can request their data in machine-readable format:

```csharp
public IActionResult ExportData(int customerId) {
    var customer = _context.Customers.Find(customerId);
    var json = JsonSerializer.Serialize(customer);
    return File(Encoding.UTF8.GetBytes(json), "application/json", "data.json");
}
```

Returns all customer data with export timestamp.

#### 3. Data Retention Policy
Configure automatic cleanup of deleted data:

**Planned Implementation:** Scheduled job to:
- Hard-delete records after retention period
- Anonymize sensitive data before deletion

#### 4. Audit Logging
**Planned Implementation:** Every data modification logged with:
- User who made the change
- Timestamp of change
- Type of change (Create, Update, Delete)
- Old and new values
- IP address and user agent

#### 5. Email Masking in Logs
Sensitive data is masked in log output:

````````

#### 6. Consent Management
**Planned Implementation:**
- User consent tracking
- Cookie consent banner
- Consent withdrawal mechanism

### GDPR Compliance Checklist

- ✅ Soft deletes (data recovery possible)
- ✅ Data export functionality
- ⚠️ Audit logging (planned)
- ⚠️ Data retention policies (planned)
- ⚠️ Anonymization mechanism (planned)
- ⚠️ Consent management (planned)
- ✅ Email masking in logs
- ✅ Secure authentication

---

## 🧪 Testing

### Test Coverage

**Unit Tests:** 37 total
- CustomerService: 26 tests
- JwtTokenService: 11 tests

**Integration Tests:** 4 total
- Customer endpoints: 2 tests
- Order endpoints: 2 tests

### Running Tests

#### All Tests

````````

---

## 🗄️ Database

### Schema Overview

#### Customers Table

| Column Name | Data Type | Constraint |
|-------------|-----------|------------|
| Id          | int       | PK, AUTO_INCREMENT |
| Name        | varchar(100) | NOT NULL   |
| Email       | varchar(255) | NOT NULL, UNIQUE |
| Phone       | varchar(50)  | NULL       |
| IsDeleted   | bit       | DEFAULT(0)  |
| CreatedAt   | datetime  | DEFAULT(GETDATE()) |
| UpdatedAt   | datetime  | DEFAULT(GETDATE()) |

#### Orders Table

| Column Name | Data Type | Constraint |
|-------------|-----------|------------|
| Id          | int       | PK, AUTO_INCREMENT |
| CustomerId  | int       | FK(Customers.Id) |
| OrderDate   | datetime  | DEFAULT(GETDATE()) |
| Status      | varchar(50)  | NOT NULL   |
| TotalAmount | decimal(18,2) | NOT NULL   |
| IsDeleted   | bit       | DEFAULT(0)  |
| CreatedAt   | datetime  | DEFAULT(GETDATE()) |
| UpdatedAt   | datetime  | DEFAULT(GETDATE()) |

### Relationships

- **Customers** to **Orders**: One-to-Many (FK: CustomerId)

### Indexes

- **Customers**: Id, Email (unique)
- **Orders**: Id, CustomerId, OrderDate

### Query Optimization

#### N+1 Prevention
Uses `AsNoTracking()` for read-only queries and projection to DTOs:

````````

---

## 👨‍💻 Development Guidelines

### Code Style & Standards

#### Naming Conventions
- **Classes**: PascalCase (e.g., `CustomerService`)
- **Methods**: PascalCase (e.g., `GetCustomerAsync`)
- **Fields**: camelCase with leading underscore (e.g., `_customerRepository`)
- **Properties**: PascalCase (e.g., `FirstName`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `MAX_PAGE_SIZE`)

#### Async/Await Guidelines
- Always use `async/await` for I/O operations
- Use `CancellationToken` parameters
- Don't use `.Result` or `.Wait()`

---

## 📊 Implementation Status

### Completed (100%) ✅
- [x] Customer CRUD operations
- [x] Order retrieval with filtering and pagination
- [x] JWT authentication
- [x] Input validation
- [x] Security headers
- [x] GDPR soft delete
- [x] Data export endpoint
- [x] Error handling middleware
- [x] Unit tests (37 tests)
- [x] Integration tests (4 tests)
- [x] Swagger documentation

### In Progress (75%) ⚠️
- [ ] Order creation service
- [ ] Audit logging mechanism
- [ ] Data retention policies
- [ ] Additional integration tests

### Planned (0%) 🔄
- [ ] Docker support
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Rate limiting
- [ ] Consent management
- [ ] Data anonymization
- [ ] Cache layer (Redis)

---

## 🐳 Docker (Planned)

### Dockerfile
````````

### Docker Compose

````````

---

## 🔄 CI/CD Pipeline (Planned)

GitHub Actions workflow for automatic testing and deployment.

---

## 📚 Additional Resources

- [Microsoft .NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [JWT.io](https://jwt.io/) - JWT explanation and tools
- [GDPR Compliance Guide](https://gdpr-info.eu/)
- [OWASP Security Best Practices](https://owasp.org/)

---

## 🤝 Contributing

### How to Contribute

1. **Create a feature branch**

````````

### Code Review Checklist

- [ ] Tests pass
- [ ] Code follows style guidelines
- [ ] No hardcoded secrets or sensitive data
- [ ] Documentation updated
- [ ] Security best practices followed

---

## 📝 License

This project is proprietary and confidential.

---

## 📞 Support

For issues, questions, or suggestions, please open a GitHub Issue or contact the development team.

---

## 🎯 Roadmap

### Q1 2026
- [x] Core CRUD operations
- [x] JWT authentication
- [ ] Order creation service
- [ ] Audit logging

### Q2 2026
- [ ] Docker containerization
- [ ] CI/CD pipeline
- [ ] API rate limiting
- [ ] Data anonymization

### Q3 2026
- [ ] Cache layer (Redis)
- [ ] Event sourcing
- [ ] Advanced reporting

---

## 📋 Quick Reference

### Common Commands
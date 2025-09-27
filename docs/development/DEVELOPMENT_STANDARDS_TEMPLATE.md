# Development Standards Template
**Document ID**: DST-004
**Title**: Enterprise Development Standards and Best Practices Template
**Date**: September 27, 2025
**Author**: Tim Yuan (Software Architect)
**Version**: 1.0
**Status**: 📋 Template
**Type**: Reference Document

## 📋 Purpose

This template serves as a comprehensive guide for maintaining enterprise-level development standards across all future projects. It consolidates lessons learned from the COMP9034 Farm Time Management System redesign and establishes reusable patterns for consistent, high-quality development.

## 🎯 Template Usage

### When to Use This Template
- ✅ Starting new projects
- ✅ Major architecture refactoring
- ✅ Code review preparation
- ✅ Developer onboarding
- ✅ Quality assurance audits

### How to Customize
1. Copy this template to your project documentation
2. Replace bracketed placeholders with project-specific values
3. Remove sections not applicable to your project
4. Add project-specific requirements
5. Update version and author information

## 🏗️ Architecture Standards

### 1. Layered Architecture Pattern

#### Mandatory Layers
```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                   │
│  Controllers, Views, API Endpoints, Frontend Components │
└─────────────────────┬───────────────────────────────────┘
                      │ DTOs, ViewModels
┌─────────────────────▼───────────────────────────────────┐
│                     Service Layer                       │
│         Business Logic, Validation, Orchestration      │
└─────────────────────┬───────────────────────────────────┘
                      │ Domain Models
┌─────────────────────▼───────────────────────────────────┐
│                  Repository Layer                       │
│        Data Access, Query Logic, Entity Mapping        │
└─────────────────────┬───────────────────────────────────┘
                      │ Database Entities
┌─────────────────────▼───────────────────────────────────┐
│                     Data Layer                          │
│          Database, External APIs, File Systems         │
└─────────────────────────────────────────────────────────┘
```

#### Layer Responsibilities
| Layer | Responsibilities | Dependencies |
|-------|------------------|--------------|
| **Presentation** | HTTP handling, Input validation, Response formatting | Service Layer only |
| **Service** | Business rules, Coordination, Transaction management | Repository Layer only |
| **Repository** | Data access, Query optimization, Entity mapping | Data Layer only |
| **Data** | Storage, External integrations | None |

### 2. Design Patterns (Mandatory)

#### Repository Pattern
```csharp
// Template: Generic Repository Interface
public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}

// Template: Specific Repository Interface
public interface I[EntityName]Repository : IGenericRepository<[EntityName]>
{
    // Add entity-specific methods here
    Task<[EntityName]?> GetBy[UniqueProperty]Async([PropertyType] [propertyName]);
    Task<IEnumerable<[EntityName]>> GetBy[Category]Async([CategoryType] [categoryValue]);
}
```

#### Service Pattern
```csharp
// Template: Service Interface
public interface I[EntityName]Service
{
    Task<ApiResult<IEnumerable<[EntityName]Dto>>> GetAll[EntityName]sAsync();
    Task<ApiResult<[EntityName]Dto>> Get[EntityName]ByIdAsync(int id);
    Task<ApiResult<[EntityName]Dto>> Create[EntityName]Async(Create[EntityName]Request request);
    Task<ApiResult<[EntityName]Dto>> Update[EntityName]Async(int id, Update[EntityName]Request request);
    Task<ApiResult> Delete[EntityName]Async(int id);
}

// Template: Service Implementation
public class [EntityName]Service : I[EntityName]Service
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<[EntityName]Service> _logger;

    public [EntityName]Service(IUnitOfWork unitOfWork, IMapper mapper, ILogger<[EntityName]Service> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // Implement interface methods with proper error handling and logging
}
```

#### Unit of Work Pattern
```csharp
// Template: Unit of Work Interface
public interface IUnitOfWork : IDisposable
{
    I[Entity1]Repository [Entity1]Repository { get; }
    I[Entity2]Repository [Entity2]Repository { get; }
    // Add other repositories

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

## 📝 Code Standards

### 1. Naming Conventions

#### C# Backend
```csharp
// ✅ Correct Examples

// Classes and Interfaces
public class StaffService { }
public interface IStaffService { }

// Methods and Properties
public async Task<ApiResult> GetStaffByIdAsync(int id) { }
public string FirstName { get; set; }

// Private fields
private readonly ILogger<StaffService> _logger;

// Constants
public const string DEFAULT_ROLE = "Staff";

// Local variables and parameters
public void ProcessUser(User user)
{
    var processedData = ProcessUserData(user);
    // ...
}
```

#### TypeScript Frontend
```typescript
// ✅ Correct Examples

// Interfaces and Types
interface User {
  id: number;
  firstName: string;
  isActive: boolean;
}

type ApiResponse<T> = {
  success: boolean;
  data: T;
  message: string;
};

// Functions and Variables
const getUserById = async (id: number): Promise<User | null> => {
  // implementation
};

// Constants
const API_BASE_URL = 'http://localhost:4000/api';

// React Components
const UserListComponent: React.FC<UserListProps> = ({ users }) => {
  // implementation
};
```

### 2. Error Handling Standards

#### Backend Error Handling
```csharp
// Template: Service Method Error Handling
public async Task<ApiResult<[EntityName]Dto>> Get[EntityName]ByIdAsync(int id)
{
    try
    {
        _logger.LogInformation("Retrieving [EntityName] with ID: {Id}", id);

        if (id <= 0)
        {
            return ApiResult<[EntityName]Dto>.ErrorResult(
                "Invalid ID provided",
                "INVALID_ID"
            );
        }

        var entity = await _unitOfWork.[EntityName]Repository.GetByIdAsync(id);

        if (entity == null)
        {
            return ApiResult<[EntityName]Dto>.ErrorResult(
                "[EntityName] not found",
                "NOT_FOUND"
            );
        }

        var dto = _mapper.Map<[EntityName]Dto>(entity);
        return ApiResult<[EntityName]Dto>.SuccessResult(dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving [EntityName] with ID: {Id}", id);
        return ApiResult<[EntityName]Dto>.ErrorResult(
            "An error occurred while retrieving [EntityName]",
            "INTERNAL_ERROR"
        );
    }
}
```

#### Frontend Error Handling
```typescript
// Template: API Call Error Handling
const useApiCall = <T>(apiFunction: () => Promise<ApiResponse<T>>) => {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const execute = async () => {
    try {
      setLoading(true);
      setError(null);

      const response = await apiFunction();

      if (response.success) {
        setData(response.data);
      } else {
        setError(response.message);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An unexpected error occurred');
    } finally {
      setLoading(false);
    }
  };

  return { data, loading, error, execute };
};
```

### 3. Testing Standards

#### Unit Test Template
```csharp
// Template: Service Unit Test
[TestFixture]
public class [EntityName]ServiceTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IMapper> _mockMapper;
    private Mock<ILogger<[EntityName]Service>> _mockLogger;
    private [EntityName]Service _service;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<[EntityName]Service>>();

        _service = new [EntityName]Service(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Get[EntityName]ByIdAsync_ValidId_ReturnsSuccess()
    {
        // Arrange
        var entityId = 1;
        var entity = new [EntityName] { Id = entityId, /* other properties */ };
        var dto = new [EntityName]Dto { Id = entityId, /* other properties */ };

        _mockUnitOfWork.Setup(x => x.[EntityName]Repository.GetByIdAsync(entityId))
            .ReturnsAsync(entity);
        _mockMapper.Setup(x => x.Map<[EntityName]Dto>(entity))
            .Returns(dto);

        // Act
        var result = await _service.Get[EntityName]ByIdAsync(entityId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.EqualTo(dto));
    }

    [Test]
    public async Task Get[EntityName]ByIdAsync_InvalidId_ReturnsError()
    {
        // Arrange
        var invalidId = -1;

        // Act
        var result = await _service.Get[EntityName]ByIdAsync(invalidId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorCode, Is.EqualTo("INVALID_ID"));
    }
}
```

## 📦 Project Structure Standards

### Backend Structure
```
backend/
├── Controllers/                    # Thin API controllers
│   ├── [EntityName]Controller.cs
│   └── BaseApiController.cs
├── Services/                       # Business logic layer
│   ├── Interfaces/
│   │   └── I[EntityName]Service.cs
│   └── Implementation/
│       └── [EntityName]Service.cs
├── Repositories/                   # Data access layer
│   ├── Interfaces/
│   │   ├── IGenericRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   └── I[EntityName]Repository.cs
│   └── Implementation/
│       ├── GenericRepository.cs
│       ├── UnitOfWork.cs
│       └── [EntityName]Repository.cs
├── Models/                         # Domain entities
│   ├── [EntityName].cs
│   └── BaseEntity.cs
├── DTOs/                          # Data transfer objects
│   ├── [EntityName]Dto.cs
│   ├── Create[EntityName]Request.cs
│   └── Update[EntityName]Request.cs
├── Common/                        # Shared utilities
│   ├── Exceptions/
│   ├── Results/
│   └── Extensions/
├── Middlewares/                   # Custom middleware
├── Data/                          # Database context
│   └── ApplicationDbContext.cs
└── Migrations/                    # EF Core migrations
```

### Frontend Structure
```
frontend/src/
├── components/                    # Reusable UI components
│   ├── common/
│   └── [feature]/
├── pages/                         # Page components
│   └── [FeatureName]/
├── stores/                        # State management
│   ├── authStore.ts
│   └── [feature]Store.ts
├── api/                          # API integration
│   ├── client.ts
│   └── [feature]Api.ts
├── types/                        # TypeScript definitions
│   ├── api.ts
│   └── [feature].ts
├── hooks/                        # Custom React hooks
├── utils/                        # Utility functions
└── assets/                       # Static assets
```

## 🔧 Configuration Standards

### Environment Configuration
```json
// Template: appsettings.json structure
{
  "ConnectionStrings": {
    "DefaultConnection": "[DatabaseConnectionString]"
  },
  "Jwt": {
    "SecretKey": "[JwtSecretKey]",
    "Issuer": "[ProjectName]",
    "Audience": "[ProjectName]-Users",
    "TokenExpiryMinutes": "480"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ApiSettings": {
    "Version": "1.0.0",
    "Title": "[ProjectName] API",
    "Description": "[ProjectDescription]"
  },
  "AllowedOrigins": [
    "[FrontendUrl]"
  ]
}
```

### Dependency Injection Template
```csharp
// Template: Program.cs service registration
// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<I[EntityName]Repository, [EntityName]Repository>();

// Services
builder.Services.AddScoped<I[EntityName]Service, [EntityName]Service>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Logging
builder.Services.AddLogging();
```

## 📋 Quality Checklist

### Code Review Checklist
- [ ] **Architecture Compliance**
  - [ ] Follows layered architecture pattern
  - [ ] Proper separation of concerns
  - [ ] No layer violations (e.g., Controller directly accessing Repository)

- [ ] **Design Patterns**
  - [ ] Repository pattern implemented correctly
  - [ ] Service layer contains business logic only
  - [ ] Unit of Work manages transactions

- [ ] **Error Handling**
  - [ ] All exceptions properly caught and logged
  - [ ] Structured error responses returned
  - [ ] Input validation performed

- [ ] **Testing**
  - [ ] Unit tests written for new functionality
  - [ ] Integration tests for complex workflows
  - [ ] Test coverage meets minimum requirements (80%)

- [ ] **Documentation**
  - [ ] XML comments on public methods
  - [ ] README updated if necessary
  - [ ] API documentation current

- [ ] **Security**
  - [ ] Input sanitization implemented
  - [ ] Authentication/authorization properly configured
  - [ ] No sensitive data in logs

### Performance Checklist
- [ ] **Database Optimization**
  - [ ] Queries use proper indexing
  - [ ] N+1 query problems avoided
  - [ ] Pagination implemented for large datasets

- [ ] **Caching Strategy**
  - [ ] Appropriate caching mechanisms in place
  - [ ] Cache invalidation logic implemented

- [ ] **Frontend Performance**
  - [ ] State management optimized
  - [ ] Unnecessary re-renders avoided
  - [ ] Code splitting implemented

## 📚 Documentation Requirements

### Required Documentation
1. **Architecture Decision Records (ADRs)**
   - Document significant architectural decisions
   - Include context, options considered, and rationale

2. **API Documentation**
   - Swagger/OpenAPI specifications
   - Request/response examples
   - Error code documentation

3. **Development Setup Guide**
   - Environment setup instructions
   - Database setup and seeding
   - Testing procedures

4. **Deployment Guide**
   - Environment-specific configurations
   - Deployment procedures
   - Rollback procedures

## 📝 Template Customization Guide

### Project-Specific Adaptations

1. **Replace Placeholders**:
   - `[ProjectName]` → Your project name
   - `[EntityName]` → Your domain entities
   - `[DatabaseConnectionString]` → Your database connection

2. **Add Project-Specific Patterns**:
   - Domain-specific validation rules
   - Custom middleware requirements
   - Integration patterns

3. **Update Technology Choices**:
   - State management library (Redux vs Zustand)
   - Database technology (PostgreSQL vs SQL Server)
   - Authentication method (JWT vs OAuth)

### Maintenance
- Review and update standards quarterly
- Incorporate lessons learned from new projects
- Align with industry best practice evolution

## 📝 Document Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-09-27 | Tim Yuan | Initial template creation based on COMP9034 redesign |

---
**Usage**: Copy this template for new projects and customize as needed. This template embodies industry best practices and lessons learned from enterprise-level development.
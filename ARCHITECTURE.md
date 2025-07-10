# CharityPay .NET - Architecture Documentation

## Overview

CharityPay is built using Clean Architecture principles with Domain-Driven Design (DDD) tactical patterns. This document describes the system architecture, key design decisions, and implementation patterns.

## System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         Frontend                             │
│                   React + TypeScript + Vite                  │
└─────────────────────┬───────────────────────────────────────┘
                      │ HTTPS/REST API
┌─────────────────────┴───────────────────────────────────────┐
│                      API Gateway                             │
│                  ASP.NET Core 8.0 API                       │
├─────────────────────────────────────────────────────────────┤
│                   Application Layer                          │
│              Use Cases / Application Services                │
├─────────────────────────────────────────────────────────────┤
│                     Domain Layer                             │
│           Entities / Value Objects / Domain Services         │
├─────────────────────────────────────────────────────────────┤
│                  Infrastructure Layer                        │
│        EF Core / External Services / File Storage          │
└─────────────────────┬───────────────────┬───────────────────┘
                      │                   │
              ┌───────┴────────┐   ┌─────┴──────┐
              │   PostgreSQL    │   │   Fiserv   │
              │    Database     │   │    API     │
              └────────────────┘   └────────────┘
```

## Layer Responsibilities

### 1. Domain Layer (CharityPay.Domain)

The heart of the application containing business logic and rules.

**Components:**
- **Entities**: Core business objects with identity (User, Organization, Payment)
- **Value Objects**: Immutable objects without identity (Money, EmailAddress)
- **Domain Events**: Significant business occurrences
- **Domain Services**: Business logic spanning multiple entities
- **Repository Interfaces**: Contracts for data persistence
- **Specifications**: Encapsulated query logic

**Key Principles:**
- No dependencies on other layers
- Rich domain model (not anemic)
- Invariants enforced within aggregates
- Domain events for loose coupling

**Example Structure:**
```
CharityPay.Domain/
├── Entities/
│   ├── User.cs
│   ├── Organization.cs
│   └── Payment.cs
├── ValueObjects/
│   ├── Money.cs
│   ├── EmailAddress.cs
│   └── PaymentMethod.cs
├── Events/
│   ├── PaymentCompletedEvent.cs
│   └── OrganizationApprovedEvent.cs
├── Services/
│   └── IPaymentService.cs
├── Repositories/
│   ├── IUserRepository.cs
│   ├── IOrganizationRepository.cs
│   └── IPaymentRepository.cs
└── Specifications/
    └── ActiveOrganizationSpecification.cs
```

### 2. Application Layer (CharityPay.Application)

Orchestrates the flow of data and coordinates domain objects.

**Components:**
- **Use Cases**: Application-specific business rules
- **DTOs**: Data Transfer Objects for API communication
- **Mappers**: Object-to-object mapping configurations
- **Validators**: Input validation rules
- **Application Services**: Orchestration logic
- **CQRS**: Commands and Queries (with MediatR)

**Key Principles:**
- Thin layer orchestrating domain objects
- No business logic (delegates to domain)
- Transaction management
- Cross-cutting concerns (logging, validation)

**Example Structure:**
```
CharityPay.Application/
├── UseCases/
│   ├── Organizations/
│   │   ├── Commands/
│   │   │   ├── CreateOrganizationCommand.cs
│   │   │   └── ApproveOrganizationCommand.cs
│   │   └── Queries/
│   │       ├── GetOrganizationsQuery.cs
│   │       └── GetOrganizationByIdQuery.cs
│   └── Payments/
│       ├── Commands/
│       │   └── InitiatePaymentCommand.cs
│       └── Queries/
│           └── GetPaymentStatusQuery.cs
├── DTOs/
│   ├── OrganizationDto.cs
│   ├── PaymentDto.cs
│   └── UserDto.cs
├── Mappings/
│   ├── OrganizationProfile.cs
│   └── PaymentProfile.cs
├── Validators/
│   ├── CreateOrganizationValidator.cs
│   └── InitiatePaymentValidator.cs
└── Services/
    ├── IAuthenticationService.cs
    └── IQrCodeService.cs
```

### 3. Infrastructure Layer (CharityPay.Infrastructure)

Implements all external concerns and provides concrete implementations.

**Components:**
- **Data Access**: EF Core DbContext and configurations
- **Repositories**: Concrete implementations
- **External Services**: Payment gateway, file storage
- **Identity**: ASP.NET Core Identity configuration
- **Caching**: Redis or in-memory caching
- **Logging**: Serilog sinks and enrichers

**Key Principles:**
- Depends on Domain and Application layers
- Implements interfaces defined in Domain
- Handles all I/O operations
- Technology-specific implementations

**Example Structure:**
```
CharityPay.Infrastructure/
├── Data/
│   ├── CharityPayDbContext.cs
│   ├── Configurations/
│   │   ├── UserConfiguration.cs
│   │   ├── OrganizationConfiguration.cs
│   │   └── PaymentConfiguration.cs
│   └── Repositories/
│       ├── UserRepository.cs
│       ├── OrganizationRepository.cs
│       └── PaymentRepository.cs
├── Identity/
│   ├── IdentityService.cs
│   └── JwtTokenService.cs
├── Services/
│   ├── FiservPaymentService.cs
│   ├── LocalFileStorageService.cs
│   └── QrCodeGenerationService.cs
├── Caching/
│   └── RedisCacheService.cs
└── Logging/
    └── SerilogEnricher.cs
```

### 4. API Layer (CharityPay.API)

The entry point for all client requests.

**Components:**
- **Endpoints**: Minimal API endpoints or controllers
- **Middleware**: Cross-cutting concerns
- **Filters**: Action filters for common behaviors
- **Models**: API-specific request/response models
- **Configuration**: Dependency injection setup

**Key Principles:**
- Thin controllers/endpoints
- No business logic
- Request/response transformation
- API versioning
- Authentication/authorization

**Example Structure:**
```
CharityPay.API/
├── Endpoints/
│   ├── AuthEndpoints.cs
│   ├── OrganizationEndpoints.cs
│   ├── PaymentEndpoints.cs
│   └── AdminEndpoints.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   ├── RequestLoggingMiddleware.cs
│   └── SecurityHeadersMiddleware.cs
├── Filters/
│   ├── ValidationFilter.cs
│   └── ApiKeyAuthFilter.cs
├── Models/
│   ├── Requests/
│   │   ├── LoginRequest.cs
│   │   └── InitiatePaymentRequest.cs
│   └── Responses/
│       ├── ApiResponse.cs
│       └── PaginatedResponse.cs
├── Configuration/
│   ├── DependencyInjection.cs
│   ├── SwaggerConfiguration.cs
│   └── AuthenticationConfiguration.cs
└── Program.cs
```

## Key Design Patterns

### 1. Repository Pattern with Unit of Work

```csharp
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IOrganizationRepository Organizations { get; }
    IPaymentRepository Payments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
```

### 2. CQRS with MediatR

```csharp
// Command
public record CreateOrganizationCommand(
    string Name,
    string Description,
    OrganizationCategory Category) : IRequest<OrganizationDto>;

// Handler
public class CreateOrganizationCommandHandler 
    : IRequestHandler<CreateOrganizationCommand, OrganizationDto>
{
    public async Task<OrganizationDto> Handle(
        CreateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### 3. Options Pattern for Configuration

```csharp
public class FiservSettings
{
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public string StoreId { get; set; }
}

// Usage
public class FiservPaymentService
{
    private readonly FiservSettings _settings;
    
    public FiservPaymentService(IOptions<FiservSettings> options)
    {
        _settings = options.Value;
    }
}
```

### 4. Specification Pattern

```csharp
public class ActiveOrganizationSpecification : Specification<Organization>
{
    public override Expression<Func<Organization, bool>> ToExpression()
    {
        return org => org.Status == OrganizationStatus.Approved 
            && !org.IsDeleted;
    }
}
```

## Data Flow

### 1. Request Flow (Command)

```
Client Request
    ↓
API Endpoint
    ↓
Validation (FluentValidation)
    ↓
MediatR Command
    ↓
Command Handler (Application Layer)
    ↓
Domain Service/Entity (Domain Layer)
    ↓
Repository (Infrastructure Layer)
    ↓
Database
```

### 2. Query Flow

```
Client Request
    ↓
API Endpoint
    ↓
MediatR Query
    ↓
Query Handler (Application Layer)
    ↓
Repository (Infrastructure Layer)
    ↓
Database
    ↓
DTO Mapping (AutoMapper)
    ↓
Response
```

## Security Architecture

### Authentication Flow

```
┌─────────┐      ┌─────────┐      ┌──────────────┐
│ Client  │─────►│   API   │─────►│ Identity     │
│         │◄─────│         │◄─────│ Service      │
└─────────┘      └─────────┘      └──────────────┘
    │                                      │
    │            ┌──────────────┐         │
    └───────────►│ JWT Token    │◄────────┘
                 │ Validation   │
                 └──────────────┘
```

### Authorization Policies

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    
    options.AddPolicy("OrganizationOwner", policy =>
        policy.AddRequirements(new OrganizationOwnerRequirement()));
});
```

## Error Handling Strategy

### Global Exception Handling

```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationException(context, ex);
        }
        catch (NotFoundException ex)
        {
            await HandleNotFoundException(context, ex);
        }
        catch (Exception ex)
        {
            await HandleGenericException(context, ex);
        }
    }
}
```

### Problem Details Response

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "detail": "See errors for details",
    "instance": "/api/v1/organizations",
    "errors": {
        "Name": ["Name is required", "Name must be at least 3 characters"]
    }
}
```

## Caching Strategy

### Multi-Level Caching

1. **Response Caching**: HTTP response caching for GET requests
2. **Distributed Caching**: Redis for shared data across instances
3. **In-Memory Caching**: For frequently accessed reference data

```csharp
[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
public async Task<IActionResult> GetOrganizations()
{
    var cacheKey = "organizations:all";
    var cached = await _cache.GetAsync<List<OrganizationDto>>(cacheKey);
    
    if (cached != null)
        return Ok(cached);
    
    var organizations = await _mediator.Send(new GetOrganizationsQuery());
    await _cache.SetAsync(cacheKey, organizations, TimeSpan.FromMinutes(5));
    
    return Ok(organizations);
}
```

## Testing Strategy

### Test Pyramid

```
         ┌─────┐
        /│ E2E │\        5%
       / └─────┘ \
      /┌─────────┐\
     / │  Integ  │ \     20%
    /  └─────────┘  \
   /┌───────────────┐\
  / │     Unit      │ \  75%
 /  └───────────────┘  \
└───────────────────────┘
```

### Testing Patterns

1. **Unit Tests**: Domain logic, services, validators
2. **Integration Tests**: API endpoints, database operations
3. **End-to-End Tests**: Critical user journeys

## Performance Considerations

### Database Optimization

1. **Indexes**: On foreign keys and frequently queried columns
2. **Eager Loading**: Include related data to avoid N+1 queries
3. **Pagination**: Limit result sets with cursor-based pagination
4. **Connection Pooling**: Optimized pool size based on load

### API Performance

1. **Response Compression**: Gzip/Brotli compression
2. **HTTP/2**: Multiplexing and server push
3. **Output Caching**: For relatively static data
4. **Async/Await**: Throughout the stack

## Monitoring and Observability

### Logging Structure

```csharp
Log.Information("Payment initiated {@Payment}", new
{
    PaymentId = payment.Id,
    OrganizationId = payment.OrganizationId,
    Amount = payment.Amount,
    Method = payment.Method
});
```

### Metrics Collection

- Request duration
- Error rates
- Business metrics (payments, registrations)
- Infrastructure metrics (CPU, memory, connections)

### Distributed Tracing

Using OpenTelemetry for end-to-end request tracing across services.

## Deployment Architecture

### Container Strategy

```dockerfile
# Multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "CharityPay.API.dll"]
```

### Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: charitypay-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: charitypay-api
  template:
    metadata:
      labels:
        app: charitypay-api
    spec:
      containers:
      - name: api
        image: charitypay/api:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
```

## Future Considerations

1. **Event Sourcing**: For payment audit trail
2. **Microservices**: Split into smaller services as needed
3. **GraphQL**: Alternative API for complex queries
4. **gRPC**: For internal service communication
5. **Message Queue**: For async processing (Azure Service Bus/RabbitMQ)
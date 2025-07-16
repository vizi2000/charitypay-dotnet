# CharityPay .NET - Architecture Documentation

## Overview

CharityPay is built using Clean Architecture principles with Domain-Driven Design (DDD) tactical patterns. This document describes the system architecture, key design decisions, and implementation patterns.

**Current Status**: The migration from Python/FastAPI to .NET 8 is approximately 60% complete with core infrastructure and basic functionality implemented.

## System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         Frontend                             │
│                   React + JavaScript + Vite                  │
│                    (Tailwind CSS, Axios)                     │
└─────────────────────┬───────────────────────────────────────┘
                      │ HTTPS/REST API
┌─────────────────────┴───────────────────────────────────────┐
│                      API Gateway                             │
│                  ASP.NET Core 8.0 API                       │
│                 (JWT Auth, CORS, Swagger)                   │
├─────────────────────────────────────────────────────────────┤
│                   Application Layer                          │
│              Application Services / DTOs                     │
│            (AutoMapper, FluentValidation)                   │
├─────────────────────────────────────────────────────────────┤
│                     Domain Layer                             │
│           Entities / Value Objects / Enums                  │
│              (User, Organization, Payment)                  │
├─────────────────────────────────────────────────────────────┤
│                  Infrastructure Layer                        │
│        EF Core / Repositories / External Services           │
│         (Polcard Client, JWT Service, Seeding)              │
└─────────────────────┬───────────────────┬───────────────────┘
                      │                   │
              ┌───────┴────────┐   ┌─────┴──────────┐
              │   PostgreSQL    │   │ Polcard/Fiserv │
              │    Database     │   │   CoPilot API  │
              └────────────────┘   └────────────────┘
```

## Layer Responsibilities

### 1. Domain Layer (CharityPay.Domain) ✅ IMPLEMENTED

The heart of the application containing business logic and rules.

**Components:**
- **Entities**: Core business objects with identity (User, Organization, Payment, Document, IoTDevice, DeviceHeartbeat)
- **Value Objects**: Immutable objects without identity (Nip, BankAccount)
- **Enums**: Business constants (UserRole, OrganizationStatus, PaymentStatus, PaymentMethod, OrganizationCategory)
- **Base Classes**: Entity base class with audit fields, Error class
- **Repository Interfaces**: Contracts for data persistence (planned)

**Current Implementation Status:**
- ✅ All core entities implemented
- ✅ All enums properly defined in separate files
- ✅ Value objects for NIP and BankAccount
- ✅ Base entity classes with audit tracking
- ❌ Domain events not yet implemented
- ❌ Specifications pattern not implemented

**Actual Structure:**
```
CharityPay.Domain/
├── Entities/
│   ├── User.cs
│   ├── Organization.cs
│   ├── Payment.cs
│   ├── Document.cs
│   ├── IoTDevice.cs
│   └── DeviceHeartbeat.cs
├── Enums/
│   ├── UserRole.cs
│   ├── OrganizationStatus.cs
│   ├── PaymentStatus.cs
│   ├── PaymentMethod.cs
│   └── OrganizationCategory.cs
├── ValueObjects/
│   ├── Nip.cs
│   └── BankAccount.cs
└── Shared/
    ├── Entity.cs
    └── Error.cs
```

### 2. Application Layer (CharityPay.Application) 🚧 PARTIALLY IMPLEMENTED

Orchestrates the flow of data and coordinates domain objects.

**Components:**
- **Application Services**: Business logic orchestration
- **DTOs**: Data Transfer Objects for API communication
- **Abstractions**: Service interfaces and contracts
- **Mappers**: AutoMapper profiles for object mapping
- **Validators**: FluentValidation rules

**Current Implementation Status:**
- ✅ Core services implemented (Authentication, Organization, Payment, MerchantOnboarding)
- ✅ DTOs for all major entities
- ✅ Service interfaces properly abstracted
- ✅ AutoMapper profiles configured
- ✅ FluentValidation validators
- ❌ CQRS with MediatR not implemented
- ❌ Use cases pattern not implemented

**Actual Structure:**
```
CharityPay.Application/
├── Abstractions/
│   ├── IAuthenticationService.cs
│   ├── IMerchantOnboardingService.cs
│   ├── IOrganizationService.cs
│   ├── IPasswordService.cs
│   ├── IPaymentService.cs
│   └── IQRCodeService.cs
├── DTOs/
│   ├── Auth/
│   │   ├── LoginDto.cs
│   │   ├── RegisterDto.cs
│   │   └── TokenDto.cs
│   ├── Organization/
│   │   ├── OrganizationDto.cs
│   │   └── CreateOrganizationDto.cs
│   └── Payment/
│       ├── PaymentDto.cs
│       └── PaymentLinkDto.cs
├── Mappings/
│   └── MappingProfile.cs
├── Services/
│   ├── AuthenticationService.cs
│   ├── MerchantOnboardingService.cs
│   ├── OrganizationService.cs
│   └── PaymentService.cs
└── Validators/
    ├── LoginDtoValidator.cs
    ├── RegisterDtoValidator.cs
    └── CreateOrganizationDtoValidator.cs
```

### 3. Infrastructure Layer (CharityPay.Infrastructure) ✅ MOSTLY IMPLEMENTED

Implements all external concerns and provides concrete implementations.

**Components:**
- **Data Access**: EF Core DbContext with comprehensive configurations
- **Repositories**: Generic repository pattern with Unit of Work
- **External Services**: Polcard/Fiserv integration, JWT, QR code generation
- **Database Seeding**: Comprehensive test data generation
- **Background Services**: Merchant status synchronization
- **Security**: JWT token generation, password hashing

**Current Implementation Status:**
- ✅ Complete EF Core setup with all entity configurations
- ✅ Repository pattern with Unit of Work
- ✅ Polcard/Fiserv CoPilot client fully implemented
- ✅ JWT service for token generation
- ✅ Password service with secure hashing
- ✅ QR code service implementation
- ✅ Database seeding for development
- ✅ Background service for merchant sync
- ❌ Redis caching not implemented
- ❌ Email service not implemented
- ❌ File storage service not implemented

**Actual Structure:**
```
CharityPay.Infrastructure/
├── Data/
│   ├── CharityPayDbContext.cs
│   ├── Configurations/
│   │   └── (Entity configurations in OnModelCreating)
│   ├── Repositories/
│   │   ├── Repository.cs
│   │   └── UnitOfWork.cs
│   └── Seeding/
│       └── DatabaseSeeder.cs
├── ExternalServices/
│   └── Polcard/
│       ├── PolcardCoPilotClient.cs
│       ├── Models/
│       │   ├── Requests/
│       │   └── Responses/
│       └── Mappings/
├── Services/
│   ├── JwtService.cs
│   ├── PasswordService.cs
│   └── QrCodeService.cs
├── BackgroundServices/
│   └── MerchantStatusSyncService.cs
└── Logging/
    └── LoggerManager.cs
```

### 4. API Layer (CharityPay.API) 🚧 PARTIALLY IMPLEMENTED

The entry point for all client requests.

**Components:**
- **Controllers**: MVC controllers (not Minimal APIs yet)
- **Middleware**: Security headers, CORS, authentication
- **Configuration**: JWT, Swagger, dependency injection
- **Background Services**: Hosted services registration

**Current Implementation Status:**
- ✅ JWT authentication configured
- ✅ CORS properly configured
- ✅ Swagger/OpenAPI documentation
- ✅ Security headers middleware
- ✅ Rate limiting middleware
- ✅ Database initialization on startup
- ✅ Basic controllers implemented
- ❌ Minimal APIs not implemented (using controllers)
- ❌ Global exception handling incomplete
- ❌ API versioning not implemented

**Actual Structure:**
```
CharityPay.API/
├── Controllers/
│   ├── AdminController.cs
│   ├── AuthController.cs
│   ├── DemoController.cs
│   ├── HealthController.cs
│   ├── MerchantOnboardingController.cs
│   ├── OrganizationsController.cs
│   └── PolcardWebhookController.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── Middleware/
│   └── (Configured in Program.cs)
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

**Key Endpoints Implemented:**
- `POST /api/v1/auth/login` - User authentication
- `POST /api/v1/auth/register-organization` - Organization registration
- `GET /api/v1/organizations` - List organizations
- `GET /api/v1/organizations/{id}` - Organization details
- `POST /api/v1/merchant-onboarding/create` - Polcard merchant creation
- `POST /api/v1/webhooks/polcard` - Webhook receiver
- `GET /health` - Health check

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
public class PolcardSettings
{
    public string BaseUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string WebhookSecret { get; set; }
    public int TokenExpiryBufferMinutes { get; set; } = 5;
}

// Usage
public class PolcardCoPilotClient
{
    private readonly PolcardSettings _settings;
    
    public PolcardCoPilotClient(IOptions<PolcardSettings> options)
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

## External Integrations

### Polcard/Fiserv CoPilot Integration

The system integrates with Polcard's CoPilot API for merchant onboarding and payment processing.

**Implementation Details:**
- OAuth2 authentication with automatic token refresh
- Comprehensive error handling and retry logic
- Request/response logging for debugging
- Webhook signature verification
- Background service for status synchronization

**Key Features:**
```csharp
public interface IPolcardCoPilotClient
{
    Task<AuthTokenResponse> GetAuthTokenAsync();
    Task<CreateMerchantResponse> CreateMerchantAsync(CreateMerchantRequest request);
    Task<UploadDocumentResponse> UploadDocumentAsync(Guid merchantId, IFormFile file);
    Task<MerchantStatusResponse> GetMerchantStatusAsync(Guid merchantId);
    bool VerifyWebhookSignature(string payload, string signature);
}
```

**Integration Flow:**
```
┌──────────────┐     ┌─────────────┐     ┌─────────────────┐
│ Organization │────►│ CharityPay  │────►│ Polcard CoPilot │
│ Registration │     │    API      │     │      API        │
└──────────────┘     └─────────────┘     └─────────────────┘
                            │                      │
                            │◄─────────────────────┘
                            │    Webhook Events
                            ▼
                     ┌─────────────┐
                     │   Database  │
                     │   Updates   │
                     └─────────────┘
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

## Frontend Architecture

### React Application Structure

The frontend is built with React 19 and JavaScript (not TypeScript as originally planned).

**Technology Stack:**
- React 19 with functional components and hooks
- Vite for fast development and building
- React Router v6 for client-side routing
- Axios for API communication
- Tailwind CSS for styling
- Context API for state management

**Current Structure:**
```
frontend/
├── src/
│   ├── components/
│   │   ├── AdminDashboard.jsx
│   │   ├── DonationForm.jsx
│   │   ├── LanguageSwitcher.jsx
│   │   ├── LoginForm.jsx
│   │   ├── OrganizationDashboard.jsx
│   │   └── RegisterForm.jsx
│   ├── contexts/
│   │   └── AuthContext.jsx
│   ├── pages/
│   │   ├── HomePage.jsx
│   │   ├── LoginPage.jsx
│   │   ├── OrganizationDetailsPage.jsx
│   │   └── PaymentPage.jsx
│   ├── services/
│   │   └── api.js (Axios client)
│   ├── utils/
│   │   └── translations.js
│   └── App.jsx
├── public/
├── index.html
└── vite.config.js
```

**API Integration:**
- Base URL configured for local development (http://localhost:8081)
- JWT token storage in localStorage
- Automatic token injection via Axios interceptors
- Field name mapping between backend (camelCase) and frontend (snake_case)

## Current Development Status

### Implementation Progress

| Layer | Status | Description |
|-------|--------|-------------|
| Domain | ✅ 100% | All entities, enums, and value objects implemented |
| Application | 🚧 70% | Core services done, CQRS/MediatR pending |
| Infrastructure | 🚧 80% | EF Core, Polcard integration complete; caching/email pending |
| API | 🚧 60% | Basic endpoints working; versioning, global error handling pending |
| Frontend | 🚧 60% | Core functionality working; TypeScript migration pending |

### Key Achievements
- Complete Polcard/Fiserv merchant onboarding integration
- Working authentication with JWT tokens
- Database seeding with comprehensive test data
- Frontend successfully integrated with backend API
- Repository pattern with Unit of Work implemented

### Critical Gaps
- Refresh token implementation (throws NotImplementedException)
- Real payment processing (mock implementation only)
- Email notifications
- File storage service
- Production-ready error handling
- Comprehensive test coverage

## Migration Considerations

### From Development to Production

1. **Database Strategy**:
   - Currently using `EnsureCreatedAsync()` for development
   - Need to implement proper EF Core migrations
   - Add database indexes for performance

2. **Security Hardening**:
   - Implement refresh token storage and rotation
   - Add API rate limiting per user
   - Enhance webhook signature verification
   - Implement proper secrets management

3. **Performance Optimization**:
   - Add Redis caching layer
   - Implement response compression
   - Add database query optimization
   - Consider CDN for static assets

4. **Monitoring & Observability**:
   - Integrate Application Insights or similar
   - Add structured logging with correlation IDs
   - Implement health checks for all dependencies
   - Add performance metrics collection

## Future Considerations

1. **Event Sourcing**: For payment audit trail
2. **Microservices**: Split into smaller services as needed
3. **GraphQL**: Alternative API for complex queries
4. **gRPC**: For internal service communication
5. **Message Queue**: For async processing (Azure Service Bus/RabbitMQ)
6. **TypeScript Migration**: Convert frontend from JavaScript to TypeScript
7. **Minimal APIs**: Migrate from controllers to .NET Minimal APIs
8. **CQRS Pattern**: Implement with MediatR for better separation of concerns
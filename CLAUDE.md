# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with the CharityPay .NET codebase.

## Development Commands

### Prerequisites
```bash
# Install .NET 8 SDK (macOS with Homebrew)
brew install --cask dotnet-sdk

# Or download from https://dotnet.microsoft.com/download
# Verify installation
dotnet --version  # Should show 8.x.x
```

### Backend (.NET 8/ASP.NET Core)
```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run API (development)
cd src/CharityPay.API
dotnet run                        # Runs on https://localhost:5001, http://localhost:5000

# Run with hot reload
dotnet watch run                  # Auto-restart on file changes

# Run tests
dotnet test                       # All tests
dotnet test --logger "console;verbosity=detailed"  # Verbose output
dotnet test /p:CollectCoverage=true  # With coverage

# Database operations (when EF Core is implemented)
dotnet ef migrations add MigrationName -p src/CharityPay.Infrastructure -s src/CharityPay.API
dotnet ef database update -p src/CharityPay.Infrastructure -s src/CharityPay.API
dotnet ef migrations remove -p src/CharityPay.Infrastructure -s src/CharityPay.API
```

### Frontend (React/TypeScript/Vite)
```bash
cd frontend
npm install
npm run dev                       # Development server on port 5173
npm run build                     # Production build
npm run preview                   # Preview production build
npm run lint                      # ESLint
# Note: Test framework not yet configured
```

### Docker Development
```bash
# Build and run with Docker Compose
docker-compose up -d              # Run all services
docker-compose down               # Stop all services
docker-compose build              # Rebuild images
docker-compose logs -f api        # View API logs
```

### Quality & Testing
```bash
# Code formatting and linting
dotnet format                     # Format code
dotnet build --verbosity normal   # Check for warnings

# Security scanning
dotnet list package --vulnerable  # Check for vulnerable packages

# Performance profiling
dotnet run -c Release --project src/CharityPay.API
```

## Project Architecture

**CharityPay .NET** - Enterprise-grade charitable payment platform built with .NET 8 and React TypeScript.

### Current System Overview
- **Status**: Migration from Python/FastAPI to C#/.NET 8
- **Architecture**: Clean Architecture with Domain-Driven Design
- **Users**: Organizations (parishes), donors, administrators
- **Payment Flow**: QR → Organization page → Donation form → Fiserv gateway → Settlement
- **Data**: PostgreSQL with Entity Framework Core

### Backend (ASP.NET Core 8)
- **Framework**: ASP.NET Core 8.0 with Minimal APIs
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, API)
- **Database**: PostgreSQL with Entity Framework Core 8
- **Authentication**: ASP.NET Core Identity + JWT Bearer tokens
- **Authorization**: Policy-based with role claims (admin, organization)
- **Validation**: FluentValidation for input validation
- **Mapping**: AutoMapper for DTO transformations
- **Logging**: Serilog with structured logging
- **API Documentation**: Swashbuckle (OpenAPI/Swagger)
- **Testing**: xUnit, Moq, FluentAssertions

#### Project Structure
```
src/
├── CharityPay.Domain/           # Core business entities and interfaces
│   ├── Entities/                # Domain entities (User, Organization, Payment, IoT)
│   ├── Enums/                   # Domain enums (UserRole, PaymentStatus, etc.)
│   └── Shared/                  # Base classes (Entity, Error)
├── CharityPay.Application/      # Business logic and use cases
│   └── (Currently empty - structure to be implemented)
├── CharityPay.Infrastructure/   # Data access and external services
│   ├── Data/                    # EF Core DbContext and configurations
│   │   ├── Configurations/      # Entity configurations
│   │   ├── Migrations/          # EF Core migrations
│   │   └── Repositories/        # Repository implementations
│   └── Logging/                 # Logging configuration
└── CharityPay.API/              # Web API layer
    ├── Controllers/             # MVC controllers (demo endpoints)
    └── (Minimal APIs structure to be implemented)

tests/                           # Test projects (xUnit structure ready)
frontend/                        # React TypeScript frontend
```

#### API Structure
- **Base URL**: `/api/v1/`
- **Authentication**: JWT Bearer tokens
- **Response Format**: Consistent envelope pattern with ApiResponse<T>
- **Error Handling**: RFC 7807 Problem Details format
- **Versioning**: URL path versioning

### Frontend (React + TypeScript)
- **Framework**: React 19 with TypeScript
- **Build Tool**: Vite for fast development and building
- **Styling**: Tailwind CSS v3 with custom design system
- **Routing**: React Router v7 with protected routes
- **State Management**: React Context API with useReducer
- **Icons**: Heroicons React
- **HTTP Client**: Axios with TypeScript interfaces
- **QR Code**: qrcode.js for QR code generation
- **Testing**: Framework to be configured

### Key Data Models

#### Domain Entities
- **User**: Authentication, roles (admin/organization), profile data
- **Organization**: Name, description, branding, contact info, financial data
- **Payment**: Amount, status, donor info, Fiserv transaction data

#### Enums (One File Per Enum)
- **UserRole**: `Admin`, `Organization`
- **OrganizationStatus**: `Pending`, `Approved`, `Rejected`, `Suspended`
- **PaymentStatus**: `Pending`, `Completed`, `Failed`, `Cancelled`
- **PaymentMethod**: `Card`, `Blik`, `ApplePay`, `GooglePay`
- **OrganizationCategory**: `Religia`, `Dzieci`, `Zwierzeta`, `Edukacja`, `Zdrowie`, `Inne`

### Database Schema (PostgreSQL)

```sql
-- Managed by Entity Framework Core migrations
-- Identity tables (AspNetUsers, AspNetRoles, etc.)
-- Organizations table with branding and financial data
-- Payments table with transaction details and status
-- Audit logs and system configuration tables
```

## Important Implementation Details

### Environment Configuration
- **Development**: `appsettings.Development.json` and user secrets
- **Production**: Environment variables and Azure Key Vault
- **Required Settings**:
  - `ConnectionStrings:DefaultConnection` - PostgreSQL connection
  - `JwtSettings:SecretKey` - JWT signing key
  - `FiservSettings:ApiKey` - Fiserv API credentials
  - `FiservSettings:ApiSecret` - Fiserv API secret
  - `FiservSettings:BaseUrl` - Fiserv API base URL
  - `FiservSettings:StoreId` - Fiserv store identifier

### Complete API Endpoints

**Authentication (JWT-based):**
- `POST /api/v1/auth/login` - User login with email/password
- `POST /api/v1/auth/register-organization` - Organization registration
- `POST /api/v1/auth/refresh` - Refresh JWT token
- `GET /api/v1/auth/me` - Current user information
- `POST /api/v1/auth/logout` - User logout

**Public Organization APIs:**
- `GET /api/v1/organizations` - List approved organizations (paginated)
- `GET /api/v1/organizations/{id}` - Get organization details
- `GET /api/v1/organizations/{id}/qr` - Generate QR code image
- `GET /api/v1/organizations/{id}/stats` - Public organization statistics

**Protected Organization Management:**
- `GET /api/v1/organization/profile` - Get own organization profile
- `PUT /api/v1/organization/profile` - Update organization profile
- `POST /api/v1/organization/logo` - Upload organization logo
- `GET /api/v1/organization/dashboard-stats` - Dashboard analytics
- `GET /api/v1/organization/payments` - Organization payment history

**Payment Processing:**
- `POST /api/v1/payments/initiate` - Initialize payment with Fiserv
- `GET /api/v1/payments/{id}/status` - Check payment status
- `POST /api/v1/payments/{id}/cancel` - Cancel pending payment

**Webhooks:**
- `POST /api/v1/webhooks/fiserv` - Fiserv webhook for payment updates

**Admin Panel (Role-based):**
- `GET /api/v1/admin/users` - List all users (paginated)
- `GET /api/v1/admin/organizations` - List organizations for approval
- `PUT /api/v1/admin/organizations/{id}/approve` - Approve/reject organization
- `GET /api/v1/admin/stats` - Global platform statistics
- `GET /api/v1/admin/payments` - All payments (admin view)

### Security Implementation
- **Authentication**: JWT Bearer tokens with claims-based identity
- **Authorization**: Policy-based authorization with custom requirements
- **Password Security**: ASP.NET Core Identity with configurable requirements
- **API Validation**: FluentValidation with comprehensive rules
- **HTTPS**: Enforced in production with HSTS headers
- **CORS**: Configured for frontend domain with credentials support
- **Rate Limiting**: IP-based and user-based rate limiting
- **Security Headers**: Comprehensive security headers middleware
- **Input Sanitization**: Automatic encoding and validation
- **SQL Injection**: Prevented by EF Core parameterized queries

### Current Development Status
- **Backend Foundation**: ✅ Clean Architecture structure established
- **Domain Models**: ✅ Entity design completed with base entities and enums
- **Database Layer**: ✅ EF Core DbContext and basic repositories implemented
- **Basic API**: ✅ Simple controllers and demo endpoints working
- **Frontend**: ✅ React TypeScript frontend with routing and auth context
- **Authentication**: ⏳ JWT and Identity integration pending
- **Full API Endpoints**: ⏳ Complete CRUD operations in progress
- **Payment Integration**: ⏳ Fiserv client development
- **Testing**: ⏳ Comprehensive test suite development

## Code Conventions

### C# Backend Standards
- **Target Framework**: .NET 8.0
- **Language Features**: C# 12 with nullable reference types
- **Async/Await**: Consistent use throughout with CancellationToken support
- **Naming**: PascalCase for public members, camelCase with underscore for private fields
- **File Organization**: One class per file, file-scoped namespaces
- **Documentation**: XML documentation for public APIs
- **Error Handling**: Custom exceptions with meaningful messages

### Dependency Injection
```csharp
// Service registration by lifetime
builder.Services.AddScoped<IPaymentService, PaymentService>();        // Per request
builder.Services.AddSingleton<IQrCodeService, QrCodeService>();      // Application lifetime
builder.Services.AddTransient<IEmailService, EmailService>();        // Per resolution
```

### Entity Framework Patterns
```csharp
// Repository pattern with Unit of Work
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IOrganizationRepository Organizations { get; }
    IPaymentRepository Payments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// Async operations with proper cancellation
var organizations = await _context.Organizations
    .AsNoTracking()
    .Where(o => o.Status == OrganizationStatus.Approved)
    .ToListAsync(cancellationToken);
```

### API Response Patterns
```csharp
// Consistent response envelope
public record ApiResponse<T>(
    bool Success,
    T Data,
    string Message = null,
    Dictionary<string, string[]> Errors = null);

// Proper HTTP status codes
return Results.Ok(new ApiResponse<OrganizationDto>(true, organizationDto));
return Results.BadRequest(new ApiResponse<object>(false, null, "Validation failed", errors));
```

### React Frontend (TypeScript)
- Use functional components with hooks
- TypeScript for all new code with strict mode
- React Hook Form for form handling
- Tailwind CSS for styling
- Axios with interceptors for API calls

### Git Workflow
- Use Conventional Commits format with task IDs
- Feature branches: `feature/CS-XXX-description`
- Reference task IDs from `TASK.md` in commit messages
- Example: `feat(api): Add JWT authentication (CS-051)`

## Development Process

1. **Always read `PLANNING.md`** to understand project goals and architecture
2. **Check `TASK.md`** before starting work to see current priorities
3. **Update `TASK.md`** when adding new tasks or completing existing ones
4. **Follow `rules.md`** for detailed coding standards and conventions
5. **Use user secrets for development** instead of appsettings files
6. **Run migrations after model changes** to keep database in sync
7. **Write tests** for new features following TDD principles
8. **Update documentation** when making significant changes

## Common Tasks

### Adding New Entity
1. Create entity in `CharityPay.Domain/Entities/`
2. Add EF configuration in `CharityPay.Infrastructure/Data/Configurations/`
3. Create migration: `dotnet ef migrations add AddNewEntity`
4. Update DbContext and repositories
5. Create DTOs and mapping profiles

### Implementing New API Endpoint
1. Create request/response DTOs in `CharityPay.API/Models/`
2. Add validation with FluentValidation
3. Create endpoint in appropriate file in `CharityPay.API/Endpoints/`
4. Add authorization policies if needed
5. Write unit and integration tests
6. Update Swagger documentation

### Setting Up External Service
1. Define interface in `CharityPay.Application/Services/`
2. Create implementation in `CharityPay.Infrastructure/Services/`
3. Add configuration settings
4. Register in DI container
5. Create mock for testing

### Database Migration
1. Make entity changes
2. Add migration: `dotnet ef migrations add MigrationName -p src/CharityPay.Infrastructure -s src/CharityPay.API`
3. Review generated migration
4. Apply to database: `dotnet ef database update`
5. Test with sample data

### Debugging Common Issues
1. **JWT Token Issues**: Check token expiration and signing key
2. **EF Core Issues**: Review migrations and connection string
3. **CORS Issues**: Verify origin configuration in CORS policy
4. **Validation Issues**: Check FluentValidation rules and model binding
5. **DI Issues**: Verify service registration and lifetime scopes

## Success Criteria

### Technical Goals
- All Python/FastAPI features migrated to .NET
- >80% test coverage across all layers
- API response times <200ms (p95)
- Zero data loss during migration
- Improved type safety and developer experience

### Quality Standards
- No compiler warnings in Release build
- All tests passing in CI/CD pipeline
- Security scan with no high/critical vulnerabilities
- Performance benchmarks meeting targets
- Complete API documentation

### Migration Completeness
- Feature parity with Python version maintained
- All existing API endpoints functional
- Payment integration working end-to-end
- Admin panel fully operational
- QR code generation and branding features complete

## Support Resources

- **.NET Documentation**: https://docs.microsoft.com/en-us/dotnet/
- **ASP.NET Core Guide**: https://docs.microsoft.com/en-us/aspnet/core/
- **Entity Framework Core**: https://docs.microsoft.com/en-us/ef/core/
- **Clean Architecture**: https://github.com/jasontaylordev/CleanArchitecture
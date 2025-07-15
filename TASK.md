# CharityPay .NET - Project Tasks

This file tracks the development tasks for the CharityPay .NET migration project.

## Current Status: **MIGRATION IN PROGRESS**
Migrating from Python/FastAPI to C#/.NET 8 while maintaining React/Vite frontend.

## Phase 1: Project Setup & Infrastructure 🚧 IN PROGRESS

### Initial Setup
- [x] **CS-001:** Create new repository 'charitypay-dotnet'
- [x] **CS-002:** Setup solution structure with Clean Architecture
- [ ] **CS-003:** Configure development environment files (appsettings.json, Docker)
- [ ] **CS-004:** Setup Entity Framework Core with migrations
- [ ] **CS-005:** Create base repository and unit of work patterns
- [ ] **CS-006:** Setup structured logging with Serilog
- [ ] **CS-007:** Configure global exception handling middleware
- [ ] **CS-008:** Setup API versioning strategy

### Development Environment
- [ ] **CS-009:** Create solution file and project references
- [ ] **CS-010:** Configure .editorconfig and code style rules
- [ ] **CS-011:** Setup pre-commit hooks for code quality
- [ ] **CS-012:** Create development Docker Compose configuration

## Phase 2: Domain Models & Data Layer

### Domain Entities
- [ ] **CS-013:** Create User entity with identity integration
- [ ] **CS-014:** Create Organization entity with DDD principles
- [ ] **CS-015:** Create Payment entity and value objects
- [ ] **CS-016:** Define domain events (PaymentCompleted, etc.)
- [ ] **CS-017:** Create domain service interfaces

### Enums (One File Per Enum)
- [x] **CS-018:** PaymentStatus.cs enum
- [x] **CS-019:** PaymentMethod.cs enum
- [x] **CS-020:** OrganizationCategory.cs enum
- [x] **CS-021:** UserRole.cs enum
- [x] **CS-022:** OrganizationStatus.cs enum

### Data Access
- [ ] **CS-023:** Create CharityPayDbContext with configurations
- [ ] **CS-024:** Entity configurations (one file per entity)
- [ ] **CS-025:** Repository interfaces in Domain layer
- [ ] **CS-026:** Repository implementations in Infrastructure
- [ ] **CS-027:** Unit of Work pattern implementation
- [ ] **CS-028:** Database seeding for development
- [ ] **CS-029:** JSON to database migration tool

## Phase 3: Application Layer & Business Logic

### DTOs and Mapping
- [ ] **CS-030:** Create OrganizationDto and related DTOs
- [ ] **CS-031:** Create PaymentDto and related DTOs
- [ ] **CS-032:** Create UserDto and AuthDto
- [ ] **CS-033:** AutoMapper profiles configuration
- [ ] **CS-034:** Response envelope models (ApiResponse, etc.)

### Validation
- [ ] **CS-035:** CreateOrganizationValidator.cs
- [ ] **CS-036:** InitiatePaymentValidator.cs
- [ ] **CS-037:** UserRegistrationValidator.cs
- [ ] **CS-038:** Custom validation rules and extensions

### Application Services
- [ ] **CS-039:** IAuthenticationService interface
- [ ] **CS-040:** AuthenticationService implementation
- [ ] **CS-041:** IOrganizationService interface
- [ ] **CS-042:** OrganizationService implementation
- [ ] **CS-043:** IPaymentService interface
- [ ] **CS-044:** PaymentService implementation

### CQRS Implementation
- [ ] **CS-045:** Setup MediatR pipeline
- [ ] **CS-046:** Organization commands (Create, Update, Approve)
- [ ] **CS-047:** Organization queries (GetAll, GetById, GetStats)
- [ ] **CS-048:** Payment commands (Initiate, UpdateStatus)
- [ ] **CS-049:** Payment queries (GetStatus, GetHistory)
- [ ] **CS-050:** Pipeline behaviors (validation, logging)

## Phase 4: Authentication & Security

### JWT Implementation
- [ ] **CS-051:** JwtSettings configuration class
- [ ] **CS-052:** JwtTokenService.cs implementation
- [ ] **CS-053:** Token validation middleware
- [ ] **CS-054:** Refresh token support

### Identity & Authorization
- [ ] **CS-055:** ASP.NET Core Identity configuration
- [ ] **CS-056:** Custom user claims and roles
- [ ] **CS-057:** Authorization policies (AdminOnly, OrganizationOwner)
- [ ] **CS-058:** Custom authorization handlers
- [ ] **CS-059:** Password requirements and validation

### Security Features
- [ ] **CS-060:** Security headers middleware
- [ ] **CS-061:** Rate limiting implementation
- [ ] **CS-062:** API key authentication (for webhooks)
- [ ] **CS-063:** CORS configuration
- [ ] **CS-064:** Data protection configuration

## Phase 5: API Endpoints Implementation

### Authentication Endpoints
- [ ] **CS-065:** POST /api/v1/auth/login
- [ ] **CS-066:** POST /api/v1/auth/register-organization
- [ ] **CS-067:** POST /api/v1/auth/refresh
- [ ] **CS-068:** GET /api/v1/auth/me
- [ ] **CS-069:** POST /api/v1/auth/logout

### Public Organization Endpoints
- [ ] **CS-070:** GET /api/v1/organizations
- [ ] **CS-071:** GET /api/v1/organizations/{id}
- [ ] **CS-072:** GET /api/v1/organizations/{id}/stats
- [ ] **CS-073:** GET /api/v1/organizations/{id}/qr

### Protected Organization Endpoints
- [ ] **CS-074:** GET /api/v1/organization/profile
- [ ] **CS-075:** PUT /api/v1/organization/profile
- [ ] **CS-076:** POST /api/v1/organization/logo
- [ ] **CS-077:** GET /api/v1/organization/dashboard-stats
- [ ] **CS-078:** GET /api/v1/organization/payments

### Payment Endpoints
- [ ] **CS-079:** POST /api/v1/payments/initiate
- [ ] **CS-080:** GET /api/v1/payments/{id}/status
- [ ] **CS-081:** POST /api/v1/payments/{id}/cancel

### Webhook Endpoints
- [ ] **CS-082:** POST /api/v1/webhooks/fiserv
- [ ] **CS-083:** Webhook signature verification

### Admin Endpoints
- [ ] **CS-084:** GET /api/v1/admin/users
- [ ] **CS-085:** GET /api/v1/admin/organizations
- [ ] **CS-086:** PUT /api/v1/admin/organizations/{id}/approve
- [ ] **CS-087:** GET /api/v1/admin/stats
- [ ] **CS-088:** GET /api/v1/admin/payments

## Phase 6: External Integrations

### Fiserv Payment Gateway
- [ ] **CS-089:** FiservSettings configuration
- [ ] **CS-090:** IFiservClient interface
- [ ] **CS-091:** FiservClient implementation
- [ ] **CS-092:** Payment link generation
- [ ] **CS-093:** Webhook payload models
- [ ] **CS-094:** HMAC signature verification
- [ ] **CS-095:** Mock Fiserv client for testing

### Other Services
- [ ] **CS-096:** IQrCodeService interface
- [ ] **CS-097:** QrCodeService implementation
- [ ] **CS-098:** IFileStorageService interface
- [ ] **CS-099:** LocalFileStorageService implementation
- [ ] **CS-100:** Email service preparation (interface only)

## Phase 7: Testing & Quality Assurance

### Unit Tests
- [ ] **CS-101:** Domain entity tests
- [ ] **CS-102:** Value object tests
- [ ] **CS-103:** Domain service tests
- [ ] **CS-104:** Application service tests
- [ ] **CS-105:** Validator tests
- [ ] **CS-106:** Mapping profile tests

### Integration Tests
- [ ] **CS-107:** API endpoint integration tests
- [ ] **CS-108:** Database repository tests
- [ ] **CS-109:** Authentication flow tests
- [ ] **CS-110:** Payment flow tests
- [ ] **CS-111:** File upload tests

### Testing Infrastructure
- [x] **CS-112:** Test data builders
- [ ] **CS-113:** In-memory database for tests
- [ ] **CS-114:** Mock service implementations
- [ ] **CS-115:** Test fixtures and helpers
- [ ] **CS-116:** Code coverage configuration

## Phase 8: Frontend Enhancement

### TypeScript Migration
- [ ] **CS-117:** Configure TypeScript in frontend
- [ ] **CS-118:** Create type definitions for API
- [ ] **CS-119:** Migrate components to TypeScript
- [ ] **CS-120:** Type-safe API client generation

### Testing
- [ ] **CS-121:** Setup Vitest configuration
- [ ] **CS-122:** Component unit tests
- [ ] **CS-123:** Integration tests
- [ ] **CS-124:** E2E tests with Playwright

## Phase 9: DevOps & Deployment

### CI/CD Pipeline
- [ ] **CS-125:** GitHub Actions workflow for .NET
- [ ] **CS-126:** Automated testing in pipeline
- [ ] **CS-127:** Code quality checks
- [ ] **CS-128:** Container image building
- [ ] **CS-129:** Deployment automation

### Infrastructure
- [ ] **CS-130:** Production Dockerfile
- [ ] **CS-131:** Docker Compose for local dev
- [ ] **CS-132:** Kubernetes manifests
- [ ] **CS-133:** Health check endpoints
- [ ] **CS-134:** Monitoring and logging setup

## Phase 10: Documentation & Finalization

### Documentation
- [ ] **CS-135:** API documentation with examples
- [ ] **CS-136:** Architecture diagrams
- [ ] **CS-137:** Deployment guide
- [ ] **CS-138:** Developer onboarding guide
- [ ] **CS-139:** Security documentation

### Final Tasks
- [ ] **CS-140:** Performance optimization
- [ ] **CS-141:** Security audit
- [ ] **CS-142:** Load testing
- [ ] **CS-143:** Migration guide from Python
- [ ] **CS-144:** Production readiness checklist

## Notes

### Migration Strategy
- Maintain feature parity with Python version
- Improve performance and type safety
- Enhanced error handling and logging
- Better scalability and maintainability

### Key Improvements
- Compile-time type checking
- Superior IDE support
- Better performance (10-50x faster)
- Enterprise-grade patterns
- Robust testing framework

### Technical Decisions
- Clean Architecture for maintainability
- PostgreSQL for production database
- JWT for stateless authentication
- MediatR for CQRS pattern
- Serilog for structured logging

### Success Metrics
- All features migrated successfully
- >80% test coverage
- <200ms API response time (p95)
- Zero data loss during migration
- Improved developer experience

## Remaining TODOs from Code Review

### High Priority - Blocking Issues
- [x] **FIX-001:** Fix entity configuration default values (OrganizationStatus.Pending and PaymentStatus.Pending)
- [x] **FIX-002:** Create missing OrganizationCategory enum
- [x] **FIX-003:** Fix test method names (GetOrganizationByUserIdAsync to GetMyOrganizationAsync)
- [x] **FIX-004:** Create TestDataBuilder infrastructure for tests

### Medium Priority - Feature Implementation
- [ ] **TODO-001:** Implement refresh token functionality in AuthenticationService
  - Location: `src/CharityPay.Application/Services/AuthenticationService.cs:103`
  - Current: Throws NotImplementedException
  - Required: Token storage, validation, and user lookup

- [ ] **TODO-002:** Implement logout token invalidation
  - Location: `src/CharityPay.Application/Services/AuthenticationService.cs:126`
  - Current: Empty implementation
  - Required: Refresh token invalidation logic

- [ ] **TODO-003:** Integrate Fiserv payment gateway
  - Location: `src/CharityPay.Application/Services/PaymentService.cs:55`
  - Current: Mock implementation returning test URL
  - Required: Full Fiserv API integration

- [ ] **TODO-004:** Implement Fiserv webhook processing
  - Location: `src/CharityPay.Application/Services/PaymentService.cs:128`
  - Current: Empty implementation
  - Required: Signature verification, payload parsing, status updates

- [ ] **TODO-005:** Implement admin statistics endpoint
  - Location: `src/CharityPay.API/Controllers/AdminController.cs:82`
  - Current: Returns placeholder data
  - Required: Aggregate statistics from database

### Low Priority - Documentation
- [ ] **DOC-001:** Update all references from .NET 8 to match project files (currently all target .NET 8)
- [ ] **DOC-002:** Document test data builder usage patterns
- [ ] **DOC-003:** Add integration test examples with new builders
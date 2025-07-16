# CharityPay .NET - Project Planning

**Last Updated**: 2025-07-16  
**Status**: Migration 60% Complete

## 1. Project Goal

The primary goal is to migrate the existing Python/FastAPI backend to a production-ready .NET 8 implementation while maintaining feature parity and improving performance, scalability, and maintainability. The system provides charitable organizations with brandable donation sites accessible via payment links or QR codes.

### Current Progress
- ✅ Core infrastructure established with Clean Architecture
- ✅ Polcard/Fiserv merchant onboarding fully integrated
- ✅ Database layer with EF Core and seeding implemented
- 🚧 Authentication partially complete (refresh tokens pending)
- 🚧 Payment processing using mock implementation
- ❌ Production features (caching, email, monitoring) not started

## 2. Architecture Overview

### 2.1 Clean Architecture Principles

The project follows Clean Architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────┐
│                   Presentation                   │
│              (API Controllers/Endpoints)         │
├─────────────────────────────────────────────────┤
│                  Application                     │
│           (Use Cases, DTOs, Services)           │
├─────────────────────────────────────────────────┤
│                    Domain                        │
│         (Entities, Value Objects, Rules)        │
├─────────────────────────────────────────────────┤
│                Infrastructure                    │
│      (Data Access, External Services, IoC)      │
└─────────────────────────────────────────────────┘
```

### 2.2 Domain-Driven Design

- **Aggregates**: User, Organization, Payment
- **Value Objects**: Money, EmailAddress, PaymentMethod
- **Domain Events**: PaymentCompleted, OrganizationApproved
- **Domain Services**: PaymentProcessingService, QrCodeGenerationService

## 3. Technical Architecture

### 3.1 Backend (.NET 8)

#### Core Technologies
- **Framework**: ASP.NET Core 8.0 with Minimal APIs
- **ORM**: Entity Framework Core 8 with PostgreSQL
- **Authentication**: ASP.NET Core Identity + JWT Bearer
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Logging**: Serilog with structured logging
- **Caching**: IMemoryCache (in-memory) / Redis (production)
- **Background Jobs**: IHostedService / Hangfire (complex scenarios)

#### Design Patterns
- **Repository Pattern**: Generic repository with Unit of Work
- **CQRS**: Using MediatR for complex operations
- **Options Pattern**: For configuration management
- **Factory Pattern**: For payment provider instantiation
- **Strategy Pattern**: For payment processing methods

#### API Design
- **RESTful**: Following REST principles
- **Versioning**: URL path versioning (/api/v1/)
- **Documentation**: OpenAPI 3.0 with Swashbuckle
- **Response Format**: Consistent envelope pattern
- **Error Handling**: RFC 7807 Problem Details

### 3.2 Frontend (React + JavaScript)

**Note**: Currently implemented in JavaScript, TypeScript migration planned

- **Framework**: React 19 with JavaScript
- **Build Tool**: Vite for fast development
- **Styling**: Tailwind CSS v3
- **State Management**: React Context API
- **Data Fetching**: Axios with interceptors
- **Form Handling**: Controlled components
- **Routing**: React Router v6
- **Testing**: Not yet configured

### 3.3 Data Architecture

#### PostgreSQL Schema

```sql
-- Users table (managed by ASP.NET Core Identity)
-- Organizations table
CREATE TABLE Organizations (
    Id UUID PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description TEXT NOT NULL,
    Category VARCHAR(50) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    UserId UUID REFERENCES AspNetUsers(Id),
    CollectedAmount DECIMAL(10,2) DEFAULT 0,
    TargetAmount DECIMAL(10,2) NOT NULL,
    -- Additional fields...
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP
);

-- Payments table
CREATE TABLE Payments (
    Id UUID PRIMARY KEY,
    OrganizationId UUID REFERENCES Organizations(Id),
    Amount DECIMAL(10,2) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    Method VARCHAR(20) NOT NULL,
    -- Additional fields...
    CreatedAt TIMESTAMP NOT NULL,
    CompletedAt TIMESTAMP
);
```

#### Migration Strategy
1. Initial migration creates Identity tables
2. Domain entities migration
3. Seed data migration
4. Data import from JSON files

### 3.4 Security Architecture

- **Authentication**: JWT with refresh tokens
- **Authorization**: Policy-based with custom requirements
- **Data Protection**: ASP.NET Core Data Protection API
- **HTTPS**: Enforced in production
- **CORS**: Configured for frontend origin
- **Rate Limiting**: IP-based and user-based limits
- **Input Validation**: Multi-layer validation
- **SQL Injection**: Prevented by EF Core parameterization
- **XSS Prevention**: Automatic in Razor, manual in API responses

## 4. Core Features (Maintained from MVP)

### 4.1 Payment Processing
- **Fiserv Integration**: Payment link generation and webhook handling
- **Payment Methods**: Card, BLIK, Apple Pay, Google Pay
- **Transaction Management**: Status tracking and updates
- **Security**: HMAC-SHA256 webhook verification

### 4.2 Organization Management
- **Registration**: Self-service with admin approval workflow
- **Branding**: Logo upload, color customization, custom messages
- **Analytics**: Dashboard with donation statistics
- **QR Codes**: Dynamic generation for print materials

### 4.3 User Management
- **Roles**: Admin, Organization
- **Authentication**: JWT-based with role claims
- **Profile Management**: Organization users can manage their profiles
- **Admin Panel**: Complete user and organization oversight

### 4.4 Public Features
- **Organization Directory**: Searchable list of approved organizations
- **Donation Pages**: Mobile-optimized with OS-specific payment buttons
- **Multi-language**: Polish and English support

## 5. Development Strategy - UPDATED STATUS

### 5.1 Phase 1: Foundation ✅ COMPLETE
- ✅ Project setup and structure
- ✅ Database design and EF Core configuration
- ✅ Base repositories and unit of work
- ✅ Authentication infrastructure (JWT)
- ✅ Logging with Serilog

### 5.2 Phase 2: Core Domain ✅ COMPLETE
- ✅ Domain entities and value objects
- ✅ Business rule implementation
- ✅ Service layer development
- ✅ DTO mapping with AutoMapper
- ✅ Validation rules with FluentValidation

### 5.3 Phase 3: API Development 🚧 70% COMPLETE
- ✅ Basic endpoint implementation
- ✅ Request/response models
- ❌ API versioning setup
- ✅ Swagger documentation
- ✅ Integration with frontend

### 5.4 Phase 4: External Integrations 🚧 60% COMPLETE
- ✅ Polcard/Fiserv merchant onboarding
- 🚧 Payment gateway (mock implementation)
- ✅ QR code generation
- ❌ File storage service
- ❌ Email notifications

### 5.5 Phase 5: Testing & Polish ❌ 20% COMPLETE
- 🚧 Unit test infrastructure ready
- ❌ Integration tests
- ❌ Performance optimization
- ❌ Security audit
- 🚧 Documentation in progress

## 5.6 Critical Next Steps (July-August 2025)

### Immediate Priorities
1. **Refresh Token Implementation** - Required for production auth
2. **Real Payment Processing** - Replace mock with actual Fiserv integration
3. **Database Migrations** - Move from EnsureCreated to proper migrations
4. **Error Handling** - Implement global exception middleware

### Production Readiness (August-September 2025)
1. **Caching Layer** - Redis integration for performance
2. **Email Service** - Transactional email implementation
3. **Monitoring** - Application Insights or equivalent
4. **CI/CD Pipeline** - Automated testing and deployment

### 5.6 Phase 6: Deployment (Week 6)
- CI/CD pipeline setup
- Container configuration
- Production deployment
- Monitoring setup
- Performance tuning

## 6. Infrastructure Requirements

### Development
- .NET 8 SDK
- PostgreSQL 14+
- Redis (optional for caching)
- Docker Desktop

### Production
- Azure App Service / AWS ECS / Kubernetes
- PostgreSQL (Azure Database / AWS RDS)
- Redis Cache
- Application Insights / CloudWatch
- SSL Certificate

## 7. Performance Targets

- API Response Time: < 200ms (p95)
- Database Query Time: < 50ms (p95)
- Page Load Time: < 2s (mobile 3G)
- Concurrent Users: 1000+
- Uptime: 99.9%

## 8. Monitoring & Observability

- **Metrics**: Response times, error rates, throughput
- **Logging**: Structured logs with correlation IDs
- **Tracing**: Distributed tracing for payment flows
- **Alerts**: Error rate thresholds, performance degradation
- **Dashboards**: Real-time system health visualization

## 9. Future Enhancements

- SMS notifications for donations
- Recurring donation support
- Multiple payment provider support
- Advanced analytics and reporting
- Mobile applications (iOS/Android)
- Blockchain donation tracking
- AI-powered fraud detection

## 10. Success Criteria

- All existing features migrated successfully
- Performance improvement of 5-10x
- Zero data loss during migration
- Improved developer experience
- Enhanced security posture
- Scalable architecture for future growth
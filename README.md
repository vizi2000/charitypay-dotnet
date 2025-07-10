# CharityPay .NET

A modern, enterprise-grade charitable payment platform built with .NET 8 and React.

## Overview

CharityPay is a complete rewrite of the original Python/FastAPI backend to C#/.NET 8, maintaining the React/Vite frontend. The platform enables charitable organizations (parishes) to accept contactless payments via QR codes and custom donation pages.

## Architecture

This project follows Clean Architecture principles with Domain-Driven Design:

```
src/
├── CharityPay.Domain/        # Core business entities and interfaces
├── CharityPay.Application/   # Business logic, DTOs, and use cases
├── CharityPay.Infrastructure/ # Data access, external services
└── CharityPay.API/           # Web API endpoints and middleware

tests/                        # Unit and integration tests
frontend/                     # React + Vite + TypeScript
docs/                        # Documentation
```

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0 with Minimal APIs
- **Database**: PostgreSQL with Entity Framework Core 8
- **Authentication**: ASP.NET Core Identity + JWT
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Logging**: Serilog
- **API Documentation**: Swashbuckle (OpenAPI/Swagger)
- **Testing**: xUnit, Moq, FluentAssertions

### Frontend
- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite
- **Styling**: Tailwind CSS v4
- **State Management**: React Context API
- **HTTP Client**: Axios with TypeScript interfaces
- **Testing**: Vitest, React Testing Library

### External Integrations
- **Payment Gateway**: Fiserv/PolCard API
- **QR Code Generation**: QRCoder
- **File Storage**: Local (development), Azure Blob Storage (production)

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- PostgreSQL 14+
- Docker (optional)

## Quick Start

### Backend Setup

1. Install .NET 8 SDK:
   ```bash
   # macOS with Homebrew
   brew install --cask dotnet-sdk
   
   # Or download from https://dotnet.microsoft.com/download
   ```

2. Clone and navigate to the repository:
   ```bash
   cd ~/etaca/charitypay-dotnet
   ```

3. Restore dependencies and build:
   ```bash
   dotnet restore
   dotnet build
   ```

4. Set up environment variables:
   ```bash
   cp src/CharityPay.API/.env.example src/CharityPay.API/.env
   # Edit .env with your configuration
   ```

5. Run database migrations:
   ```bash
   cd src/CharityPay.API
   dotnet ef database update
   ```

6. Run the API:
   ```bash
   dotnet run --project src/CharityPay.API
   ```

The API will be available at `https://localhost:7001` (HTTPS) and `http://localhost:5000` (HTTP).

### Frontend Setup

1. Navigate to frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Run development server:
   ```bash
   npm run dev
   ```

The frontend will be available at `http://localhost:5173`.

## Development

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Run specific project tests
dotnet test tests/CharityPay.Domain.Tests
```

### Database Migrations

```bash
# Add a new migration
dotnet ef migrations add MigrationName -p src/CharityPay.Infrastructure -s src/CharityPay.API

# Update database
dotnet ef database update -p src/CharityPay.Infrastructure -s src/CharityPay.API

# Remove last migration
dotnet ef migrations remove -p src/CharityPay.Infrastructure -s src/CharityPay.API
```

### API Documentation

When running in development, Swagger UI is available at:
- https://localhost:7001/swagger

## Project Structure

### Domain Layer (CharityPay.Domain)
- Contains enterprise business rules
- Entities, Value Objects, Domain Events
- Repository interfaces
- Domain services interfaces

### Application Layer (CharityPay.Application)
- Contains application business rules
- Use cases/Application services
- DTOs and mapping profiles
- Validation rules
- CQRS commands and queries

### Infrastructure Layer (CharityPay.Infrastructure)
- Data access implementations
- External service integrations
- File system access
- Email/SMS services

### API Layer (CharityPay.API)
- RESTful endpoints
- Middleware components
- Dependency injection configuration
- API-specific models

## Configuration

### Application Settings

Configuration is managed through:
1. `appsettings.json` - Base configuration
2. `appsettings.{Environment}.json` - Environment-specific overrides
3. Environment variables
4. User secrets (development only)

### Key Configuration Sections

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=charitypay;Username=postgres;Password=password"
  },
  "JwtSettings": {
    "Secret": "your-secret-key",
    "Issuer": "CharityPay",
    "Audience": "CharityPayUsers",
    "ExpirationDays": 7
  },
  "FiservSettings": {
    "BaseUrl": "https://sandbox.api.fiservapps.com",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret",
    "StoreId": "your-store-id"
  }
}
```

## Security

- JWT authentication with refresh tokens
- Role-based authorization (Admin, Organization)
- HTTPS enforcement in production
- Input validation and sanitization
- SQL injection prevention via parameterized queries
- XSS protection through proper encoding
- CORS configuration
- Rate limiting
- Security headers middleware

## Deployment

### Docker

```bash
# Build images
docker-compose build

# Run services
docker-compose up -d

# View logs
docker-compose logs -f
```

### Production Deployment

1. Set production environment variables
2. Run database migrations
3. Build and publish:
   ```bash
   dotnet publish -c Release -o ./publish
   ```
4. Deploy to Azure App Service, AWS, or Kubernetes

## Monitoring

- Health checks: `/health`
- Metrics: `/metrics` (Prometheus format)
- Structured logging with Serilog
- Application Insights integration (Azure)

## Contributing

1. Follow the coding standards in `docs/CODING_STANDARDS.md`
2. Write unit tests for new features
3. Update documentation
4. Create pull request with clear description

## License

[License information]

## Support

For issues and questions:
- GitHub Issues: [repository-url]/issues
- Documentation: See `/docs` directory
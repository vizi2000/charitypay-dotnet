# CharityPay .NET

A modern, enterprise-grade charitable payment platform built with .NET 8 and React.

**Status**: Migration 60% Complete | [Architecture](./architecture.md) | [Tasks](./TASK.md) | [Planning](./PLANNING.md)

## Overview

CharityPay is a complete rewrite of the original Python/FastAPI backend to C#/.NET 8, maintaining the React/Vite frontend. The platform enables charitable organizations to accept contactless payments via QR codes and custom donation pages.

### Current Features
- ✅ Organization registration and management
- ✅ QR code generation for contactless payments
- ✅ JWT-based authentication (refresh tokens pending)
- ✅ Polcard/Fiserv merchant onboarding integration
- 🚧 Payment processing (mock implementation)
- ❌ Email notifications (not implemented)
- ❌ Production deployment (development only)

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
- **Framework**: React 19 with JavaScript (TypeScript migration planned)
- **Build Tool**: Vite
- **Styling**: Tailwind CSS v3
- **State Management**: React Context API
- **HTTP Client**: Axios
- **Testing**: Not yet configured

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

### Using Docker (Recommended)

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

Services will be available at:
- API: http://localhost:8081
- Frontend: http://localhost:5174
- PostgreSQL: localhost:5433
- Redis: localhost:6380

### Manual Setup

#### Backend

1. Install .NET 8 SDK:
   ```bash
   # macOS with Homebrew
   brew install --cask dotnet-sdk
   
   # Windows/Linux: Download from https://dotnet.microsoft.com/download
   ```

2. Configure PostgreSQL connection in `appsettings.Development.json`

3. Run the API:
   ```bash
   cd src/CharityPay.API
   dotnet run
   ```

The API will be available at `https://localhost:5001` with Swagger at `/swagger`.

#### Frontend

1. Navigate to frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Configure API endpoint in `.env`:
   ```bash
   VITE_API_URL=http://localhost:8081
   ```

4. Run development server:
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

### Database Management

The application now applies EF Core migrations automatically on startup. During
development the database is recreated and seeded with sample data (six test organizations).

### API Documentation

Swagger UI is available at:
- Docker: http://localhost:8081/swagger
- Manual setup: https://localhost:5001/swagger

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
  "PolcardSettings": {
    "BaseUrl": "https://copilot-uat.polcard.pl",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "WebhookSecret": "your-webhook-secret"
  }
}
```

## Security

- JWT authentication (refresh tokens pending implementation)
- Role-based authorization (Admin, Organization)
- HTTPS enforcement in production
- Input validation with FluentValidation
- SQL injection prevention via Entity Framework Core
- XSS protection through proper encoding
- CORS properly configured
- Rate limiting implemented
- Security headers middleware active

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

The project ships with a helper script `start-production.sh` which provisions the
containers defined in `docker-compose.production.yml` and runs database migrations automatically.
To deploy to a host such as **194.181.240.37**:

1. Copy the provided template and edit your settings:
   ```bash
   cp .env.production.example .env.production
   nano .env.production   # set EXTERNAL_IP=194.181.240.37 and strong secrets
   ```
2. Start the stack:
   ```bash
   ./start-production.sh
   ```
   The API will run on port `8081` and the frontend will be available at
   `http://194.181.240.37:5174`.

For advanced scenarios you may still publish the application manually and deploy
to a cloud provider:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

## Monitoring

- Health checks: `/health`
- Metrics: `/metrics` (Prometheus format)
- Structured logging with Serilog
- Application Insights integration (Azure)

## Known Issues

1. **Refresh Tokens**: Not implemented, throws `NotImplementedException`
2. **Payment Processing**: Using mock implementation, returns test URLs
3. **Email Service**: Not implemented
4. **File Storage**: Local storage only, cloud storage pending
5. **Database Migrations**: Automatically applied on startup

## Contributing

1. Check [TASK.md](./TASK.md) for current priorities
2. Follow Clean Architecture principles
3. Write unit tests for new features
4. Update documentation
5. Create pull request with clear description

## License

This project is proprietary software. All rights reserved.

## Support

For issues and questions:
- GitHub Issues: https://github.com/vizi2000/charitypay-dotnet/issues
- Documentation: See architecture.md, PLANNING.md, and TASK.md
- Development guide: See CLAUDE.md for AI-assisted development
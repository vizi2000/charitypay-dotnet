# CharityPay .NET Deployment Guide

## Prerequisites

- Docker Desktop installed and running
- Git (for cloning the repository)
- .NET 9 SDK (optional, only if running without Docker)

## Quick Start with Docker

### 1. Clone the Repository

```bash
git clone <repository-url>
cd charitypay-dotnet
```

### 2. Configure Environment

```bash
# Copy the example environment file
cp .env.example .env

# Edit .env with your settings
# IMPORTANT: Change all passwords and secret keys!
```

### 3. Choose Deployment Type

#### Option A: Simple Deployment (Recommended for Testing)
Includes: Frontend, API, PostgreSQL, and Adminer

```bash
docker-compose -f docker-compose.simple.yml up -d
```

Access:
- Frontend: http://localhost:5173
- API: http://localhost:5000
- API Docs (Swagger): http://localhost:5000/swagger
- Database Admin: http://localhost:8085

#### Option B: Development Deployment
Includes hot reload and development tools

```bash
docker-compose -f docker-compose.dev.yml up -d
```

#### Option C: Production Deployment
Full stack with monitoring (Prometheus & Grafana)

```bash
docker-compose up -d
```

### 4. Initialize Database

The database will be automatically initialized with migrations when the API container starts.

To manually run migrations:
```bash
docker-compose exec api dotnet ef database update -p src/CharityPay.Infrastructure -s src/CharityPay.API
```

## Service URLs

### Simple/Dev Deployment:
- **Frontend**: http://localhost:5173
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **Adminer** (DB UI): http://localhost:8085
  - Server: `postgres`
  - Username: from .env `POSTGRES_USER`
  - Password: from .env `POSTGRES_PASSWORD`
  - Database: from .env `POSTGRES_DB`

### Production Deployment adds:
- **Nginx** (Reverse Proxy): http://localhost
- **Prometheus**: http://localhost:9090
- **Grafana**: http://localhost:3001

## Verification Steps

1. Check all containers are running:
```bash
docker-compose ps
```

2. Check API health:
```bash
curl http://localhost:5000/health
```

3. Access Swagger UI to test endpoints:
http://localhost:5000/swagger

## Common Issues

### Port Conflicts
If ports are already in use, modify the port mappings in the docker-compose file:
```yaml
ports:
  - "8080:5000"  # Change 8080 to available port
```

### Database Connection Issues
- Ensure PostgreSQL container is healthy: `docker-compose ps`
- Check logs: `docker-compose logs postgres`
- Verify connection string in .env file

### Permission Issues
On Linux/Mac, you might need to set proper permissions:
```bash
chmod +x scripts/*.sh
```

## Stopping Services

```bash
# Stop all services
docker-compose down

# Stop and remove volumes (WARNING: Deletes data)
docker-compose down -v
```

## Updating

1. Pull latest changes:
```bash
git pull origin main
```

2. Rebuild containers:
```bash
docker-compose build
docker-compose up -d
```

3. Run any new migrations:
```bash
docker-compose exec api dotnet ef database update -p src/CharityPay.Infrastructure -s src/CharityPay.API
```

## Security Notes

Before deploying to production:
1. Change ALL default passwords in .env
2. Generate strong JWT secret key (min 32 characters)
3. Use HTTPS with valid SSL certificates
4. Configure firewall rules
5. Enable rate limiting
6. Review and update CORS settings
7. Disable Swagger in production

## Support

For issues or questions, please check:
- Application logs: `docker-compose logs api`
- Database logs: `docker-compose logs postgres`
- Frontend logs: `docker-compose logs frontend`
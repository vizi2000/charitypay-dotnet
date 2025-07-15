# Docker Deployment Guide

## Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/charitypay-dotnet.git
   cd charitypay-dotnet
   ```

2. **Configure environment variables**
   - Copy `.env.example` to `.env` (already done)
   - Update any necessary values in `.env`

3. **Build and run containers**
   ```bash
   docker-compose build
   docker-compose up -d
   ```

4. **Access the application**
   - Frontend: http://localhost:5174
   - API: http://localhost:8081
   - Swagger UI: http://localhost:8081/swagger
   - Health Check: http://localhost:8081/health
   - PostgreSQL: localhost:5433
   - Redis: localhost:6380
   - Grafana: http://localhost:3001 (when enabled)

## Container Management

### View container status
```bash
docker-compose ps
```

### View logs
```bash
# All containers
docker-compose logs -f

# Specific container
docker-compose logs -f charitypay-api
```

### Stop containers
```bash
docker-compose down
```

### Stop and remove volumes
```bash
docker-compose down -v
```

## Development Mode

The default configuration runs in development mode with:
- Hot reload enabled (code changes auto-restart)
- Source code mounted as volumes
- Development environment variables

## Production Mode

To run in production mode:
```bash
docker-compose -f docker-compose.yml up -d
```

This will:
- Skip the override file
- Include nginx reverse proxy
- Include monitoring stack (Prometheus & Grafana)
- Use production build target

## Monitoring Stack

Enable monitoring services:
```bash
docker-compose --profile monitoring up -d
```

Access:
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3001
  - Default login: admin/admin123

## Troubleshooting

### Port conflicts
If you get port binding errors, update the ports in `docker-compose.yml`:
- PostgreSQL: Change `5433:5432` to another port
- Redis: Change `6380:6379` to another port
- API: Change `8081:80` to another port

### Database connection issues
Check the connection string in the API logs:
```bash
docker-compose logs charitypay-api | grep -i connection
```

### Container won't start
1. Check logs: `docker-compose logs [container-name]`
2. Verify environment variables in `.env`
3. Ensure all required directories exist
4. Check Docker daemon is running

## Database Migrations

To run database migrations:
```bash
# Enter the API container
docker-compose exec charitypay-api bash

# Run migrations
cd /app
dotnet ef database update
```

## Backup and Restore

### Backup PostgreSQL
```bash
docker-compose exec charitypay-db pg_dump -U charitypay_user charitypay > backup.sql
```

### Restore PostgreSQL
```bash
docker-compose exec -T charitypay-db psql -U charitypay_user charitypay < backup.sql
```
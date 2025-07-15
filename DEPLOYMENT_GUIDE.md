# CharityPay .NET - Deployment Guide

## Quick Start

### 1. First-time deployment:
```bash
chmod +x *.sh
./clean-deploy.sh
```

### 2. Redeploy existing:
```bash
./redeploy.sh
```

### 3. Check status:
```bash
./check-status.sh
```

### 4. Run tests:
```bash
./quick-test.sh
```

## Deployment Scripts

### `clean-deploy.sh`
- Removes all containers and volumes
- Builds fresh images from scratch
- Starts services in proper order
- Attempts to run database migrations
- Runs quick test automatically

**Use when:**
- First deployment
- Major configuration changes
- Database schema changes
- Troubleshooting persistent issues

### `redeploy.sh`
- Stops existing containers
- Rebuilds images
- Restarts services
- Preserves database data

**Use when:**
- Code changes
- Configuration updates
- Routine deployments

### `check-status.sh`
- Shows container status
- Verifies service endpoints
- Checks database connectivity
- Displays table count

**Use when:**
- Verifying deployment
- Troubleshooting issues
- Health monitoring

## Manual Commands

### View logs:
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f charitypay-api
docker-compose logs -f charitypay-frontend
```

### Access containers:
```bash
# API container
docker exec -it charitypay-api bash

# Database
docker exec -it charitypay-postgres psql -U charitypay_user -d charitypay

# Redis
docker exec -it charitypay-redis redis-cli -a redis_password_2024
```

### Run migrations manually:
```bash
# Inside API container
docker exec charitypay-api dotnet ef database update --project src/CharityPay.Infrastructure --startup-project src/CharityPay.API

# Using migration script
docker exec charitypay-api bash /app/migrate.sh
```

## Service URLs

- **Frontend**: http://localhost:5174
- **API**: http://localhost:8081
- **Swagger UI**: http://localhost:8081/swagger
- **Health Check**: http://localhost:8081/health

## Database Access

- **Host**: localhost
- **Port**: 5433
- **Database**: charitypay
- **Username**: charitypay_user
- **Password**: charitypay_password_2024

## Redis Access

- **Host**: localhost
- **Port**: 6380
- **Password**: redis_password_2024

## Test Credentials

**Admin User:**
- Email: `admin@charitypay.pl`
- Password: `admin123`

**Organization User:**
- Email: `org@charitypay.pl`
- Password: `org123`

## Troubleshooting

### Containers not starting:
```bash
# Check logs
docker-compose logs [service-name]

# Restart specific service
docker-compose restart [service-name]

# Full cleanup and redeploy
./clean-deploy.sh
```

### Port conflicts:
```bash
# Find process using port
lsof -i :5174  # Frontend
lsof -i :8081  # API
lsof -i :5433  # PostgreSQL
lsof -i :6380  # Redis

# Kill process
kill -9 [PID]
```

### Database issues:
```bash
# Connect to database
docker exec -it charitypay-postgres psql -U charitypay_user -d charitypay

# Check tables
\dt

# Run migrations manually
docker exec charitypay-api bash -c "cd /app && dotnet ef database update --project src/CharityPay.Infrastructure --startup-project src/CharityPay.API"
```

### Build issues:
```bash
# Clean Docker cache
docker system prune -a
docker volume prune

# Rebuild without cache
docker-compose build --no-cache
```

## Production Deployment

For production deployment:

1. Update environment variables in `.env` file
2. Configure proper SSL certificates
3. Set strong passwords
4. Enable production logging
5. Configure monitoring
6. Set up backup strategy
7. Review security settings

## Monitoring

Check application health:
```bash
# API health
curl http://localhost:8081/health

# Quick system test
./quick-test.sh

# Full test suite
./run-all-tests.sh
```
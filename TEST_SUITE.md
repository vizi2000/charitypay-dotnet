# CharityPay .NET Test Suite

This directory contains comprehensive integration tests for the CharityPay .NET application.

## Available Tests

### 1. Quick Test (`quick-test.sh`)
A fast smoke test that verifies core services are running:
- API health check
- Frontend accessibility  
- Database connectivity
- Redis connectivity
- Authentication endpoints
- Demo data endpoints

**Usage:**
```bash
./quick-test.sh
```

### 2. System Integration Tests (`test-system.sh`)
Comprehensive system-wide tests including:
- Docker container status
- Service health checks
- Database schema verification
- Inter-service connectivity
- Performance baseline tests

**Usage:**
```bash
./test-system.sh
```

### 3. API Integration Tests (`test-api.sh`)
Detailed API endpoint testing:
- All REST endpoints
- Authentication flows
- CORS configuration
- Error handling
- Response time analysis

**Usage:**
```bash
./test-api.sh
```

### 4. Frontend Integration Tests (`test-frontend.sh`)
Frontend functionality testing:
- Asset loading
- Route accessibility
- API connectivity from browser
- Authentication flow
- End-to-end user journeys

**Usage:**
```bash
./test-frontend.sh
```

### 5. Database Migration Tests (`test-migrations.sh`)
Database-specific testing:
- Connection verification
- Schema inspection
- Migration status
- Table structure validation

**Usage:**
```bash
./test-migrations.sh
```

### 6. Complete Test Suite (`run-all-tests.sh`)
Runs all tests in sequence with detailed reporting.

**Usage:**
```bash
./run-all-tests.sh
```

## Prerequisites

Before running tests, ensure:

1. All Docker containers are running:
   ```bash
   docker-compose up -d
   ```

2. Services are healthy:
   ```bash
   docker ps
   ```

3. Test scripts are executable:
   ```bash
   chmod +x *.sh
   ```

## Test Credentials

The demo authentication uses these credentials:

**Admin User:**
- Email: `admin@charitypay.pl`
- Password: `admin123`

**Organization User:**
- Email: `org@charitypay.pl`
- Password: `org123`

## Interpreting Results

- **Green checkmarks (✓)**: Test passed
- **Red X marks (✗)**: Test failed
- **Yellow warnings (⚠)**: Non-critical issue

## Troubleshooting

### Common Issues

1. **"Connection refused" errors**
   - Ensure all containers are running
   - Wait 30 seconds after starting containers

2. **"No tables found" in database**
   - Run migrations: `./test-migrations.sh`
   - Check migration logs

3. **CORS errors**
   - Verify frontend URL in API CORS settings
   - Check API logs for details

4. **Authentication failures**
   - Ensure demo controller is deployed
   - Check JWT configuration

### Debug Commands

View container logs:
```bash
docker logs charitypay-api --tail 50
docker logs charitypay-postgres --tail 50
docker logs charitypay-frontend --tail 50
```

Check container health:
```bash
docker ps --format "table {{.Names}}\t{{.Status}}"
```

## Next Steps

After all tests pass:

1. **Apply database migrations** to enable full authentication
2. **Configure production settings** in appsettings.json
3. **Set up monitoring** for production deployment
4. **Run load tests** for performance optimization
5. **Configure CI/CD** pipeline with these tests
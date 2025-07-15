#!/bin/bash

# CharityPay .NET - Clean Deployment Script
# Performs a complete clean deployment

set -e

echo "======================================"
echo "CharityPay .NET - Clean Deployment"
echo "======================================"
echo ""

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${YELLOW}WARNING: This will remove all containers and volumes!${NC}"
echo "Press Ctrl+C to cancel, or Enter to continue..."
read -r

# Step 1: Complete cleanup
echo ""
echo "1. Cleaning up existing deployment..."
docker-compose down -v --remove-orphans || true
docker system prune -f
echo -e "${GREEN}✓ Cleanup complete${NC}"

# Step 2: Build fresh images
echo ""
echo "2. Building fresh images..."
docker-compose build --no-cache --pull
echo -e "${GREEN}✓ Images built${NC}"

# Step 3: Start services in order
echo ""
echo "3. Starting database and cache services..."
docker-compose up -d charitypay-db charitypay-redis
echo "Waiting for database to initialize..."
sleep 15

# Step 4: Start API
echo ""
echo "4. Starting API service..."
docker-compose up -d charitypay-api
echo "Waiting for API to start..."
sleep 10

# Step 5: Start frontend
echo ""
echo "5. Starting frontend service..."
docker-compose up -d charitypay-frontend
sleep 5

# Step 6: Run database migrations
echo ""
echo "6. Applying database migrations..."
echo -e "${BLUE}Note: If migrations fail, you may need to run them manually${NC}"

# Try to run migrations
if docker exec charitypay-api dotnet ef database update --project src/CharityPay.Infrastructure --startup-project src/CharityPay.API 2>/dev/null; then
    echo -e "${GREEN}✓ Migrations applied${NC}"
else
    echo -e "${YELLOW}⚠ Could not apply migrations automatically${NC}"
    echo "To apply manually, run:"
    echo "  docker exec charitypay-api bash /app/migrate.sh"
fi

# Step 7: Verify deployment
echo ""
echo "7. Verifying deployment..."
echo ""

# Function to check service
check_service() {
    local name=$1
    local check_cmd=$2
    echo -n "  - $name: "
    if eval "$check_cmd" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Running${NC}"
        return 0
    else
        echo -e "${RED}✗ Not ready${NC}"
        return 1
    fi
}

all_ready=true
check_service "PostgreSQL" "docker exec charitypay-postgres psql -U charitypay_user -d charitypay -c '\q'" || all_ready=false
check_service "Redis" "docker exec charitypay-redis redis-cli -a redis_password_2024 ping" || all_ready=false
check_service "API" "curl -s -f http://localhost:8081/health" || all_ready=false
check_service "Frontend" "curl -s -f http://localhost:5174" || all_ready=false

# Step 8: Show status
echo ""
echo "======================================"
if [ "$all_ready" = true ]; then
    echo -e "${GREEN}Deployment Successful!${NC}"
else
    echo -e "${YELLOW}Deployment Complete (with warnings)${NC}"
fi
echo "======================================"
echo ""

# Show access information
echo "Access points:"
echo "  - Frontend: http://localhost:5174"
echo "  - API: http://localhost:8081"
echo "  - API Swagger: http://localhost:8081/swagger"
echo ""
echo "Database:"
echo "  - Host: localhost:5433"
echo "  - Database: charitypay"
echo "  - User: charitypay_user"
echo ""
echo "Redis:"
echo "  - Host: localhost:6380"
echo "  - Password: redis_password_2024"
echo ""

# Show next steps
echo "Next steps:"
echo "1. Run quick test: ./quick-test.sh"
echo "2. View logs: docker-compose logs -f"
echo "3. Apply migrations if needed: docker exec charitypay-api bash /app/migrate.sh"
echo ""

# Run quick test automatically
echo -e "${BLUE}Running quick test...${NC}"
if [ -f "./quick-test.sh" ]; then
    chmod +x quick-test.sh
    ./quick-test.sh
fi
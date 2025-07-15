#!/bin/bash

# CharityPay .NET - Redeploy Script
# Stops, rebuilds, and restarts all containers

set -e

echo "======================================"
echo "CharityPay .NET - Redeployment"
echo "======================================"
echo ""

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# Step 1: Stop existing containers
echo "1. Stopping existing containers..."
docker-compose down || true
echo -e "${GREEN}✓ Containers stopped${NC}"

# Step 2: Remove volumes (optional - uncomment if needed)
# echo "2. Removing volumes..."
# docker-compose down -v
# echo -e "${GREEN}✓ Volumes removed${NC}"

# Step 3: Pull latest base images
echo ""
echo "2. Pulling latest base images..."
docker-compose pull
echo -e "${GREEN}✓ Base images updated${NC}"

# Step 4: Build containers
echo ""
echo "3. Building containers..."
docker-compose build --no-cache
echo -e "${GREEN}✓ Containers built${NC}"

# Step 5: Start containers
echo ""
echo "4. Starting containers..."
docker-compose up -d
echo -e "${GREEN}✓ Containers started${NC}"

# Step 6: Wait for services to be ready
echo ""
echo "5. Waiting for services to be ready..."
sleep 10

# Check container status
echo ""
echo "6. Container Status:"
docker-compose ps

# Step 7: Run quick health check
echo ""
echo "7. Running health checks..."
sleep 5

# Check API
echo -n "  - API Health: "
if curl -s -f http://localhost:8081/health > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Healthy${NC}"
else
    echo -e "${RED}✗ Not responding${NC}"
fi

# Check Frontend
echo -n "  - Frontend: "
if curl -s -f http://localhost:5174 > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Accessible${NC}"
else
    echo -e "${RED}✗ Not accessible${NC}"
fi

# Check Database
echo -n "  - Database: "
if docker exec charitypay-postgres psql -U charitypay_user -d charitypay -c '\q' 2>/dev/null; then
    echo -e "${GREEN}✓ Connected${NC}"
else
    echo -e "${RED}✗ Connection failed${NC}"
fi

# Check Redis
echo -n "  - Redis: "
if docker exec charitypay-redis redis-cli -a redis_password_2024 ping 2>/dev/null | grep -q "PONG"; then
    echo -e "${GREEN}✓ Connected${NC}"
else
    echo -e "${RED}✗ Connection failed${NC}"
fi

echo ""
echo "======================================"
echo "Redeployment Complete!"
echo "======================================"
echo ""
echo "Access points:"
echo "  - Frontend: http://localhost:5174"
echo "  - API: http://localhost:8081"
echo "  - API Swagger: http://localhost:8081/swagger"
echo ""
echo "To view logs:"
echo "  docker-compose logs -f"
echo ""
echo "To run tests:"
echo "  ./quick-test.sh"
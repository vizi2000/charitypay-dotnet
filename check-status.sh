#!/bin/bash

# CharityPay .NET - Deployment Status Check

echo "======================================"
echo "CharityPay .NET - Status Check"
echo "======================================"
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Check Docker daemon
echo "Docker Status:"
if docker info > /dev/null 2>&1; then
    echo -e "  ${GREEN}✓ Docker daemon is running${NC}"
else
    echo -e "  ${RED}✗ Docker daemon not running${NC}"
    exit 1
fi

# Check containers
echo ""
echo "Container Status:"
containers=(
    "charitypay-postgres:PostgreSQL Database"
    "charitypay-redis:Redis Cache"
    "charitypay-api:.NET API"
    "charitypay-frontend:React Frontend"
)

for container_info in "${containers[@]}"; do
    IFS=':' read -r container_name display_name <<< "$container_info"
    
    if docker ps --format '{{.Names}}' | grep -q "^${container_name}$"; then
        status=$(docker inspect -f '{{.State.Status}}' "$container_name" 2>/dev/null)
        health=$(docker inspect -f '{{.State.Health.Status}}' "$container_name" 2>/dev/null || echo "none")
        
        if [ "$status" = "running" ]; then
            if [ "$health" = "healthy" ] || [ "$health" = "none" ]; then
                echo -e "  ${GREEN}✓ $display_name - Running${NC}"
            else
                echo -e "  ${YELLOW}⚠ $display_name - Running (health: $health)${NC}"
            fi
        else
            echo -e "  ${RED}✗ $display_name - Status: $status${NC}"
        fi
    else
        echo -e "  ${RED}✗ $display_name - Not found${NC}"
    fi
done

# Check service endpoints
echo ""
echo "Service Endpoints:"

# API
echo -n "  - API (http://localhost:8081): "
if curl -s -f -o /dev/null http://localhost:8081/health 2>/dev/null; then
    echo -e "${GREEN}✓ Responding${NC}"
else
    echo -e "${RED}✗ Not responding${NC}"
fi

# Frontend
echo -n "  - Frontend (http://localhost:5174): "
if curl -s -f -o /dev/null http://localhost:5174 2>/dev/null; then
    echo -e "${GREEN}✓ Accessible${NC}"
else
    echo -e "${RED}✗ Not accessible${NC}"
fi

# Swagger
echo -n "  - Swagger UI (http://localhost:8081/swagger): "
if curl -s -f -o /dev/null http://localhost:8081/swagger/index.html 2>/dev/null; then
    echo -e "${GREEN}✓ Available${NC}"
else
    echo -e "${RED}✗ Not available${NC}"
fi

# Database check
echo ""
echo "Database Status:"
echo -n "  - Connection: "
if docker exec charitypay-postgres psql -U charitypay_user -d charitypay -c '\q' 2>/dev/null; then
    echo -e "${GREEN}✓ Connected${NC}"
    
    # Check tables
    table_count=$(docker exec charitypay-postgres psql -U charitypay_user -d charitypay -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';" 2>/dev/null | tr -d ' ' || echo "0")
    echo "  - Tables: $table_count found"
else
    echo -e "${RED}✗ Cannot connect${NC}"
fi

# Redis check
echo ""
echo "Redis Status:"
echo -n "  - Connection: "
if docker exec charitypay-redis redis-cli -a redis_password_2024 ping 2>/dev/null | grep -q "PONG"; then
    echo -e "${GREEN}✓ Connected${NC}"
else
    echo -e "${RED}✗ Cannot connect${NC}"
fi

# Show logs command
echo ""
echo "======================================"
echo "To view logs, use:"
echo "  docker-compose logs -f [service]"
echo ""
echo "Services: charitypay-api, charitypay-frontend, charitypay-db, charitypay-redis"
echo ""
echo "To redeploy: ./redeploy.sh"
echo "To run tests: ./quick-test.sh"
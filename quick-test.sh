#!/bin/bash

# Quick test to verify core functionality

echo "CharityPay .NET - Quick Test"
echo "============================"
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

# Test function
test() {
    local description=$1
    local command=$2
    local expected=$3
    
    echo -n "Testing $description... "
    
    result=$(eval "$command" 2>&1)
    
    if [[ $result == *"$expected"* ]]; then
        echo -e "${GREEN}✓${NC}"
        return 0
    else
        echo -e "${RED}✗${NC}"
        echo "  Expected: $expected"
        echo "  Got: $result"
        return 1
    fi
}

# Run tests
test "API Health"     "curl -s http://localhost:8081/health"     "healthy"

test "Frontend"     "curl -s -o /dev/null -w '%{http_code}' http://localhost:5174"     "200"

test "Database"     "docker exec charitypay-postgres psql -U charitypay_user -d charitypay -c 'SELECT 1' 2>/dev/null | grep -c '1 row'"     "1"

test "Redis"     "docker exec charitypay-redis redis-cli -a redis_password_2024 ping 2>/dev/null"     "PONG"

test "Demo Login"     "curl -s -X POST http://localhost:8081/api/v1/auth-demo/login -H 'Content-Type: application/json' -d '{\"email\":\"admin@charitypay.pl\",\"password\":\"admin123\"}' | grep -o 'access_token'"     "access_token"

test "Organizations API"     "curl -s http://localhost:8081/api/v1/demo/organizations | grep -o 'Parafia'"     "Parafia"

echo ""
echo "Quick test completed!"
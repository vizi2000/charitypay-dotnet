#!/bin/bash

# CharityPay .NET System Integration Tests
# This script tests all components of the deployed system

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "=========================================="
echo "CharityPay .NET System Integration Tests"
echo "=========================================="
echo ""

# Function to print test results
print_result() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓ $2${NC}"
    else
        echo -e "${RED}✗ $2${NC}"
        echo "  Error: $3"
    fi
}

# Function to wait for service
wait_for_service() {
    local url=$1
    local service=$2
    local max_attempts=30
    local attempt=1
    
    echo -n "Waiting for $service to be ready..."
    while [ $attempt -le $max_attempts ]; do
        if curl -s -f "$url" > /dev/null 2>&1; then
            echo -e " ${GREEN}Ready!${NC}"
            return 0
        fi
        echo -n "."
        sleep 2
        attempt=$((attempt + 1))
    done
    echo -e " ${RED}Timeout!${NC}"
    return 1
}

# 1. Check Docker containers
echo "1. Checking Docker containers..."
echo "--------------------------------"

# Check if containers are running
containers=("charitypay-postgres" "charitypay-redis" "charitypay-api" "charitypay-frontend")
all_running=true

for container in "${containers[@]}"; do
    if docker ps --format '{{.Names}}' | grep -q "^${container}$"; then
        status=$(docker inspect -f '{{.State.Status}}' "$container")
        if [ "$status" = "running" ]; then
            print_result 0 "$container is running"
        else
            print_result 1 "$container status: $status" "Container is not running"
            all_running=false
        fi
    else
        print_result 1 "$container not found" "Container does not exist"
        all_running=false
    fi
done

echo ""

# 2. Check service health
echo "2. Checking service health..."
echo "-----------------------------"

# Wait for API to be ready
wait_for_service "http://localhost:8081/health" "API"

# Check API health endpoint
echo -n "Testing API health endpoint... "
health_response=$(curl -s -w "\n%{http_code}" http://localhost:8081/health 2>/dev/null || echo "000")
http_code=$(echo "$health_response" | tail -n 1)
if [ "$http_code" = "200" ]; then
    print_result 0 "API health check passed (HTTP $http_code)"
else
    print_result 1 "API health check failed" "HTTP $http_code"
fi

echo ""

# 3. Check database connectivity
echo "3. Checking database..."
echo "-----------------------"

# Check if database is accessible
echo -n "Testing database connection... "
if docker exec charitypay-postgres psql -U charitypay_user -d charitypay -c '\q' 2>/dev/null; then
    print_result 0 "Database connection successful"
else
    print_result 1 "Database connection failed" "Could not connect to PostgreSQL"
fi

# Check if tables exist
echo -n "Checking database tables... "
table_count=$(docker exec charitypay-postgres psql -U charitypay_user -d charitypay -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';" 2>/dev/null | tr -d ' ')
if [ -n "$table_count" ] && [ "$table_count" -gt 0 ]; then
    print_result 0 "Found $table_count tables in database"
    echo "  Tables:"
    docker exec charitypay-postgres psql -U charitypay_user -d charitypay -c "\dt" 2>/dev/null | grep -E "^ public" | awk '{print "    - " $3}'
else
    print_result 1 "No tables found" "Database migrations may not have been applied"
fi

echo ""

# 4. Check Redis connectivity
echo "4. Checking Redis..."
echo "--------------------"

echo -n "Testing Redis connection... "
if docker exec charitypay-redis redis-cli -a redis_password_2024 ping 2>/dev/null | grep -q "PONG"; then
    print_result 0 "Redis connection successful"
else
    print_result 1 "Redis connection failed" "Could not connect to Redis"
fi

echo ""

# 5. Test API endpoints
echo "5. Testing API endpoints..."
echo "---------------------------"

# Test root endpoint
echo -n "Testing root endpoint... "
root_response=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8081/ 2>/dev/null)
if [ "$root_response" = "200" ]; then
    print_result 0 "Root endpoint accessible (HTTP $root_response)"
else
    print_result 1 "Root endpoint failed" "HTTP $root_response"
fi

# Test Swagger UI
echo -n "Testing Swagger UI... "
swagger_response=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8081/swagger/index.html 2>/dev/null)
if [ "$swagger_response" = "200" ]; then
    print_result 0 "Swagger UI accessible (HTTP $swagger_response)"
else
    print_result 1 "Swagger UI not accessible" "HTTP $swagger_response"
fi

# Test demo organizations endpoint
echo -n "Testing organizations endpoint... "
org_response=$(curl -s http://localhost:8081/api/v1/demo/organizations 2>/dev/null)
if echo "$org_response" | grep -q "Parafia"; then
    org_count=$(echo "$org_response" | grep -o '"id"' | wc -l)
    print_result 0 "Organizations endpoint working (found $org_count organizations)"
else
    print_result 1 "Organizations endpoint failed" "No data returned"
fi

echo ""

# 6. Test authentication
echo "6. Testing authentication..."
echo "----------------------------"

# Test demo login endpoint
echo -n "Testing demo login (admin)... "
login_response=$(curl -s -X POST http://localhost:8081/api/v1/auth-demo/login \
    -H "Content-Type: application/json" \
    -d '{"email":"admin@charitypay.pl","password":"admin123"}' 2>/dev/null)

if echo "$login_response" | grep -q "access_token"; then
    print_result 0 "Demo admin login successful"
    access_token=$(echo "$login_response" | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)
    
    # Test authenticated endpoint
    echo -n "Testing authenticated endpoint... "
    me_response=$(curl -s -H "Authorization: Bearer $access_token" http://localhost:8081/api/v1/auth-demo/me 2>/dev/null)
    if echo "$me_response" | grep -q "admin@charitypay.pl"; then
        print_result 0 "Authenticated endpoint working"
    else
        print_result 1 "Authenticated endpoint failed" "Could not retrieve user info"
    fi
else
    print_result 1 "Demo login failed" "No access token received"
fi

echo ""

# 7. Test frontend
echo "7. Testing frontend..."
echo "----------------------"

# Check if frontend is accessible
echo -n "Testing frontend accessibility... "
frontend_response=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5174 2>/dev/null)
if [ "$frontend_response" = "200" ]; then
    print_result 0 "Frontend accessible (HTTP $frontend_response)"
else
    print_result 1 "Frontend not accessible" "HTTP $frontend_response"
fi

# Check if frontend can load assets
echo -n "Testing frontend assets... "
if curl -s http://localhost:5174 | grep -q "CharityPay"; then
    print_result 0 "Frontend content loading correctly"
else
    print_result 1 "Frontend content not loading" "Could not find expected content"
fi

echo ""

# 8. Test CORS
echo "8. Testing CORS configuration..."
echo "---------------------------------"

echo -n "Testing CORS headers... "
cors_response=$(curl -s -I -X OPTIONS http://localhost:8081/api/v1/demo/organizations \
    -H "Origin: http://localhost:5174" \
    -H "Access-Control-Request-Method: GET" 2>/dev/null)

if echo "$cors_response" | grep -q "Access-Control-Allow-Origin"; then
    print_result 0 "CORS headers present"
else
    print_result 1 "CORS headers missing" "API may not accept frontend requests"
fi

echo ""

# 9. Performance check
echo "9. Basic performance check..."
echo "------------------------------"

echo -n "Testing API response time... "
start_time=$(date +%s%N)
curl -s http://localhost:8081/api/v1/demo/organizations > /dev/null 2>&1
end_time=$(date +%s%N)
response_time=$(( (end_time - start_time) / 1000000 ))

if [ $response_time -lt 500 ]; then
    print_result 0 "API response time: ${response_time}ms"
else
    print_result 1 "API response slow" "${response_time}ms (expected < 500ms)"
fi

echo ""

# 10. Summary
echo "=========================================="
echo "Test Summary"
echo "=========================================="

# Count successes and failures
total_tests=$(grep -c "print_result" "$0")
failed_tests=$(grep -c "✗" /tmp/test_output_$$ 2>/dev/null || echo 0)
passed_tests=$((total_tests - failed_tests))

echo -e "Total tests: $total_tests"
echo -e "Passed: ${GREEN}$passed_tests${NC}"
echo -e "Failed: ${RED}$failed_tests${NC}"

if [ $failed_tests -eq 0 ]; then
    echo -e "\n${GREEN}All tests passed! ✓${NC}"
    exit 0
else
    echo -e "\n${RED}Some tests failed. Please check the errors above.${NC}"
    exit 1
fi
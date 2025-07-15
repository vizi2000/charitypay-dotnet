#!/bin/bash

# CharityPay .NET - Run All Tests
# Main test orchestrator

set -e

GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
BOLD='\033[1m'
NC='\033[0m'

echo -e "${BOLD}=========================================="
echo "CharityPay .NET - Complete Test Suite"
echo "==========================================${NC}"
echo ""
echo "This will run all integration tests for the CharityPay system."
echo "Make sure all containers are running before proceeding."
echo ""

# Check if test scripts exist
if [ ! -f "test-system.sh" ] || [ ! -f "test-api.sh" ] || [ ! -f "test-frontend.sh" ]; then
    echo -e "${RED}Error: Test scripts not found in current directory${NC}"
    exit 1
fi

# Make scripts executable
chmod +x test-system.sh test-api.sh test-frontend.sh

# Function to run a test suite
run_test_suite() {
    local test_name=$1
    local test_script=$2
    
    echo -e "\n${BLUE}${BOLD}Running $test_name${NC}"
    echo "----------------------------------------"
    
    if ./$test_script; then
        echo -e "${GREEN}✓ $test_name completed successfully${NC}"
        return 0
    else
        echo -e "${RED}✗ $test_name failed${NC}"
        return 1
    fi
}

# Track test results
declare -a test_results
test_count=0
failed_count=0

# Run system tests
if run_test_suite "System Integration Tests" "test-system.sh"; then
    test_results[test_count]="PASS: System Integration"
else
    test_results[test_count]="FAIL: System Integration"
    ((failed_count++))
fi
((test_count++))

echo -e "\n${BOLD}Press Enter to continue with API tests...${NC}"
read -r

# Run API tests
if run_test_suite "API Integration Tests" "test-api.sh"; then
    test_results[test_count]="PASS: API Integration"
else
    test_results[test_count]="FAIL: API Integration"
    ((failed_count++))
fi
((test_count++))

echo -e "\n${BOLD}Press Enter to continue with Frontend tests...${NC}"
read -r

# Run frontend tests
if run_test_suite "Frontend Integration Tests" "test-frontend.sh"; then
    test_results[test_count]="PASS: Frontend Integration"
else
    test_results[test_count]="FAIL: Frontend Integration"
    ((failed_count++))
fi
((test_count++))

# Final summary
echo -e "\n${BOLD}=========================================="
echo "FINAL TEST SUMMARY"
echo "==========================================${NC}"
echo ""

for result in "${test_results[@]}"; do
    if [[ $result == PASS* ]]; then
        echo -e "${GREEN}✓ $result${NC}"
    else
        echo -e "${RED}✗ $result${NC}"
    fi
done

echo ""
echo -e "Total test suites: $test_count"
echo -e "Passed: ${GREEN}$((test_count - failed_count))${NC}"
echo -e "Failed: ${RED}$failed_count${NC}"

if [ $failed_count -eq 0 ]; then
    echo -e "\n${GREEN}${BOLD}All tests passed! The system is working correctly. ✓${NC}"
    exit 0
else
    echo -e "\n${RED}${BOLD}Some tests failed. Please review the errors above.${NC}"
    exit 1
fi
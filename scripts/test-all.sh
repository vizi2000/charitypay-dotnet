#!/bin/bash

# test-all.sh - Comprehensive test runner script
# Usage: ./scripts/test-all.sh [--quick|--full|--coverage]

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TEST_RESULTS_DIR="$PROJECT_ROOT/test-results"
COVERAGE_DIR="$PROJECT_ROOT/coverage"

# Default mode
MODE="quick"
if [ "$1" = "--full" ]; then
    MODE="full"
elif [ "$1" = "--coverage" ]; then
    MODE="coverage"
elif [ "$1" = "--quick" ]; then
    MODE="quick"
fi

echo -e "${BLUE}🧪 CharityPay .NET Test Suite${NC}"
echo -e "${BLUE}================================${NC}"
echo -e "Mode: ${YELLOW}$MODE${NC}"
echo -e "Project Root: ${YELLOW}$PROJECT_ROOT${NC}"
echo ""

# Cleanup previous results
echo -e "${YELLOW}🧹 Cleaning up previous test results...${NC}"
rm -rf "$TEST_RESULTS_DIR" "$COVERAGE_DIR"
mkdir -p "$TEST_RESULTS_DIR" "$COVERAGE_DIR"

cd "$PROJECT_ROOT"

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK not found. Please install .NET 8 SDK.${NC}"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo -e "${BLUE}🔧 .NET Version: ${YELLOW}$DOTNET_VERSION${NC}"

# Restore dependencies
echo -e "${YELLOW}📦 Restoring NuGet packages...${NC}"
if dotnet restore --verbosity quiet; then
    echo -e "${GREEN}✅ Package restore successful${NC}"
else
    echo -e "${RED}❌ Package restore failed${NC}"
    exit 1
fi

# Build the solution
echo -e "${YELLOW}🔨 Building solution...${NC}"
if dotnet build --configuration Release --no-restore --verbosity quiet; then
    echo -e "${GREEN}✅ Build successful${NC}"
else
    echo -e "${RED}❌ Build failed${NC}"
    exit 1
fi

# Function to run tests based on mode
run_tests() {
    case $MODE in
        "quick")
            echo -e "${YELLOW}🏃‍♂️ Running quick tests (unit tests only)...${NC}"
            dotnet test --configuration Release --no-build --verbosity normal \
                --logger "trx;LogFileName=TestResults.trx" \
                --results-directory "$TEST_RESULTS_DIR" \
                --filter "Category!=Integration&Category!=E2E"
            ;;
        "full")
            echo -e "${YELLOW}🔍 Running full test suite (unit + integration + e2e)...${NC}"
            dotnet test --configuration Release --no-build --verbosity normal \
                --logger "trx;LogFileName=TestResults.trx" \
                --results-directory "$TEST_RESULTS_DIR"
            ;;
        "coverage")
            echo -e "${YELLOW}📊 Running tests with coverage analysis...${NC}"
            dotnet test --configuration Release --no-build --verbosity normal \
                --logger "trx;LogFileName=TestResults.trx" \
                --results-directory "$TEST_RESULTS_DIR" \
                --collect:"XPlat Code Coverage" \
                --settings "$PROJECT_ROOT/CodeCoverage.runsettings"
            
            # Generate coverage report if reportgenerator is available
            if command -v reportgenerator &> /dev/null; then
                echo -e "${YELLOW}📈 Generating coverage report...${NC}"
                reportgenerator \
                    "-reports:$TEST_RESULTS_DIR/**/coverage.cobertura.xml" \
                    "-targetdir:$COVERAGE_DIR" \
                    "-reporttypes:Html;Cobertura;JsonSummary"
                echo -e "${GREEN}📋 Coverage report generated: $COVERAGE_DIR/index.html${NC}"
            else
                echo -e "${YELLOW}⚠️  ReportGenerator not found. Install with: dotnet tool install -g dotnet-reportgenerator-globaltool${NC}"
            fi
            ;;
    esac
}

# Run the tests
TEST_START_TIME=$(date +%s)
if run_tests; then
    TEST_END_TIME=$(date +%s)
    TEST_DURATION=$((TEST_END_TIME - TEST_START_TIME))
    echo -e "${GREEN}✅ All tests passed! Duration: ${TEST_DURATION}s${NC}"
    
    # Display test summary if available
    if [ -f "$TEST_RESULTS_DIR/TestResults.trx" ]; then
        echo -e "${BLUE}📊 Test Summary:${NC}"
        # Parse TRX file for basic stats (simplified)
        if command -v xmllint &> /dev/null; then
            TOTAL_TESTS=$(xmllint --xpath "count(//UnitTestResult)" "$TEST_RESULTS_DIR/TestResults.trx" 2>/dev/null || echo "N/A")
            PASSED_TESTS=$(xmllint --xpath "count(//UnitTestResult[@outcome='Passed'])" "$TEST_RESULTS_DIR/TestResults.trx" 2>/dev/null || echo "N/A")
            echo -e "  Total Tests: ${YELLOW}$TOTAL_TESTS${NC}"
            echo -e "  Passed: ${GREEN}$PASSED_TESTS${NC}"
        fi
    fi
    
    exit 0
else
    TEST_END_TIME=$(date +%s)
    TEST_DURATION=$((TEST_END_TIME - TEST_START_TIME))
    echo -e "${RED}❌ Tests failed! Duration: ${TEST_DURATION}s${NC}"
    echo -e "${RED}Check test results in: $TEST_RESULTS_DIR${NC}"
    exit 1
fi
#!/bin/bash

# CharityPay .NET Test Runner (Unix/Linux/macOS)
# Comprehensive test execution script with coverage and reporting

set -e

# Default values
TEST_TYPE="All"
PROJECT="All"
COVERAGE=false
VERBOSE=false
PARALLEL=true
OUTPUT_PATH="./TestResults"
OPEN_RESULTS=false

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Helper functions
write_header() {
    echo -e "\n${BLUE}============================================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}============================================================${NC}"
}

write_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

write_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

write_error() {
    echo -e "${RED}✗ $1${NC}"
}

write_info() {
    echo -e "${CYAN}ℹ $1${NC}"
}

show_usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -t, --test-type TYPE     Test type: All, Unit, Integration, Performance (default: All)"
    echo "  -p, --project PROJECT    Project: All, Domain, Application, Infrastructure, API (default: All)"
    echo "  -c, --coverage           Enable code coverage"
    echo "  -v, --verbose            Verbose output"
    echo "  --no-parallel            Disable parallel execution"
    echo "  -o, --output PATH        Output directory (default: ./TestResults)"
    echo "  --open                   Open results in browser"
    echo "  -h, --help               Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                                    # Run all tests"
    echo "  $0 -t Unit -p Domain                 # Run unit tests for Domain project"
    echo "  $0 -c --open                         # Run all tests with coverage and open report"
    echo "  $0 -t Integration -v                 # Run integration tests with verbose output"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -t|--test-type)
            TEST_TYPE="$2"
            shift 2
            ;;
        -p|--project)
            PROJECT="$2"
            shift 2
            ;;
        -c|--coverage)
            COVERAGE=true
            shift
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        --no-parallel)
            PARALLEL=false
            shift
            ;;
        -o|--output)
            OUTPUT_PATH="$2"
            shift 2
            ;;
        --open)
            OPEN_RESULTS=true
            shift
            ;;
        -h|--help)
            show_usage
            exit 0
            ;;
        *)
            echo "Unknown option $1"
            show_usage
            exit 1
            ;;
    esac
done

# Ensure output directory exists
mkdir -p "$OUTPUT_PATH"

write_header "CharityPay .NET Test Runner"

# Check .NET installation
if ! command -v dotnet &> /dev/null; then
    write_error ".NET is not installed or not in PATH"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
write_info "Using .NET version: $DOTNET_VERSION"

# Define test projects
declare -A TEST_PROJECTS=(
    ["Domain"]="tests/CharityPay.Domain.Tests/CharityPay.Domain.Tests.csproj"
    ["Application"]="tests/CharityPay.Application.Tests/CharityPay.Application.Tests.csproj"
    ["Infrastructure"]="tests/CharityPay.Infrastructure.Tests/CharityPay.Infrastructure.Tests.csproj"
    ["API"]="tests/CharityPay.API.Tests/CharityPay.API.Tests.csproj"
)

# Define test categories
declare -A TEST_CATEGORIES=(
    ["Unit"]="Category=Unit"
    ["Integration"]="Category=Integration"
    ["Performance"]="Speed=Slow"
)

get_projects_to_run() {
    local project_filter=$1
    local projects=()
    
    if [[ "$project_filter" == "All" ]]; then
        for project in "${TEST_PROJECTS[@]}"; do
            projects+=("$project")
        done
    elif [[ -n "${TEST_PROJECTS[$project_filter]}" ]]; then
        projects+=("${TEST_PROJECTS[$project_filter]}")
    else
        write_warning "Unknown project: $project_filter. Available: ${!TEST_PROJECTS[*]}"
        return 1
    fi
    
    printf '%s\n' "${projects[@]}"
}

get_test_filter() {
    local test_type_filter=$1
    
    case "$test_type_filter" in
        "Unit")
            echo "${TEST_CATEGORIES[Unit]}"
            ;;
        "Integration")
            echo "${TEST_CATEGORIES[Integration]}"
            ;;
        "Performance")
            echo "${TEST_CATEGORIES[Performance]}"
            ;;
        "All")
            echo ""
            ;;
        *)
            write_warning "Unknown test type: $test_type_filter. Available: Unit, Integration, Performance, All"
            echo ""
            ;;
    esac
}

run_tests() {
    local projects=("$@")
    local total_projects=${#projects[@]}
    local passed_projects=0
    local failed_projects=0
    
    for project in "${projects[@]}"; do
        local project_name=$(basename "$project" .csproj)
        write_info "Running tests for $project_name..."
        
        # Build test command
        local test_args=("test" "$project")
        
        # Add test filter if specified
        local filter=$(get_test_filter "$TEST_TYPE")
        if [[ -n "$filter" ]]; then
            test_args+=("--filter" "$filter")
        fi
        
        # Add logger for test results
        local test_results_file="$OUTPUT_PATH/$project_name-results.trx"
        test_args+=("--logger" "trx;LogFileName=$test_results_file")
        
        # Add console logger with appropriate verbosity
        if [[ "$VERBOSE" == true ]]; then
            test_args+=("--logger" "console;verbosity=detailed")
        else
            test_args+=("--logger" "console;verbosity=normal")
        fi
        
        # Add coverage if enabled
        if [[ "$COVERAGE" == true ]]; then
            test_args+=("--collect" "XPlat Code Coverage")
            test_args+=("--results-directory" "$OUTPUT_PATH")
        fi
        
        # Add parallel execution
        if [[ "$PARALLEL" == true ]]; then
            test_args+=("--parallel")
        fi
        
        # Add no-build for optimization
        test_args+=("--no-build")
        
        echo "Command: dotnet ${test_args[*]}"
        
        if dotnet "${test_args[@]}"; then
            write_success "Tests passed for $project_name"
            ((passed_projects++))
        else
            write_error "Tests failed for $project_name"
            ((failed_projects++))
        fi
    done
    
    echo "$total_projects $passed_projects $failed_projects"
}

generate_coverage_report() {
    local output_dir=$1
    
    write_info "Generating coverage report..."
    
    # Find coverage files
    local coverage_files
    coverage_files=$(find "$output_dir" -name "coverage.cobertura.xml" 2>/dev/null || true)
    
    if [[ -z "$coverage_files" ]]; then
        write_warning "No coverage files found"
        return
    fi
    
    # Check if reportgenerator is installed
    if ! command -v reportgenerator &> /dev/null; then
        write_info "Installing ReportGenerator tool..."
        dotnet tool install --global dotnet-reportgenerator-globaltool
        
        # Add .NET tools to PATH if not already there
        export PATH="$PATH:$HOME/.dotnet/tools"
    fi
    
    local coverage_report_dir="$output_dir/coverage-report"
    local coverage_pattern
    coverage_pattern=$(echo "$coverage_files" | tr '\n' ';' | sed 's/;$//')
    
    if reportgenerator "-reports:$coverage_pattern" "-targetdir:$coverage_report_dir" "-reporttypes:Html;Badges;TextSummary"; then
        write_success "Coverage report generated at: $coverage_report_dir"
        
        # Display summary if available
        local summary_file="$coverage_report_dir/Summary.txt"
        if [[ -f "$summary_file" ]]; then
            echo -e "\n${BLUE}Coverage Summary:${NC}"
            cat "$summary_file"
        fi
        
        echo "$coverage_report_dir"
    else
        write_error "Failed to generate coverage report"
    fi
}

show_test_summary() {
    local total=$1
    local passed=$2
    local failed=$3
    
    write_header "Test Results Summary"
    
    echo -e "${BLUE}Total Projects: $total${NC}"
    echo -e "${GREEN}Passed: $passed${NC}"
    echo -e "${RED}Failed: $failed${NC}"
    
    local success_rate=0
    if [[ $total -gt 0 ]]; then
        success_rate=$((passed * 100 / total))
    fi
    
    local color=$RED
    if [[ $success_rate -ge 80 ]]; then
        color=$GREEN
    elif [[ $success_rate -ge 60 ]]; then
        color=$YELLOW
    fi
    
    echo -e "${color}Success Rate: $success_rate%${NC}"
    
    if [[ $failed -gt 0 ]]; then
        write_warning "Some tests failed. Check the detailed output above for more information."
        exit 1
    else
        write_success "All tests passed!"
    fi
}

# Main execution
main() {
    write_info "Test Configuration:"
    echo "  Test Type: $TEST_TYPE"
    echo "  Project: $PROJECT"
    echo "  Coverage: $COVERAGE"
    echo "  Verbose: $VERBOSE"
    echo "  Parallel: $PARALLEL"
    echo "  Output Path: $OUTPUT_PATH"
    
    # Get projects to run
    local projects_to_run
    if ! projects_to_run=($(get_projects_to_run "$PROJECT")); then
        write_error "No projects to run"
        exit 1
    fi
    
    if [[ ${#projects_to_run[@]} -eq 0 ]]; then
        write_error "No projects to run"
        exit 1
    fi
    
    # Build solution first
    write_info "Building solution..."
    if dotnet build --configuration Release; then
        write_success "Build completed"
    else
        write_error "Build failed"
        exit 1
    fi
    
    # Run tests
    local test_results
    test_results=$(run_tests "${projects_to_run[@]}")
    read -r total passed failed <<< "$test_results"
    
    # Generate coverage report if enabled
    if [[ "$COVERAGE" == true ]]; then
        local coverage_report_path
        coverage_report_path=$(generate_coverage_report "$OUTPUT_PATH")
        
        if [[ "$OPEN_RESULTS" == true && -n "$coverage_report_path" ]]; then
            local index_path="$coverage_report_path/index.html"
            if [[ -f "$index_path" ]]; then
                write_info "Opening coverage report..."
                if command -v open &> /dev/null; then
                    open "$index_path"  # macOS
                elif command -v xdg-open &> /dev/null; then
                    xdg-open "$index_path"  # Linux
                else
                    write_info "Please open: $index_path"
                fi
            fi
        fi
    fi
    
    # Show summary
    show_test_summary "$total" "$passed" "$failed"
}

# Run main function
main "$@"
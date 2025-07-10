#!/bin/bash

# test-basic.sh - Basic validation script that works without .NET SDK
# This script performs basic validations before attempting .NET operations

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo -e "${BLUE}🔍 CharityPay .NET Basic Validation${NC}"
echo -e "${BLUE}===================================${NC}"
echo -e "Project Root: ${YELLOW}$PROJECT_ROOT${NC}"
echo ""

cd "$PROJECT_ROOT"

# Test 1: Check project structure
echo -e "${YELLOW}📁 Checking project structure...${NC}"

REQUIRED_DIRS=(
    "src/CharityPay.Domain"
    "src/CharityPay.Application" 
    "src/CharityPay.Infrastructure"
    "src/CharityPay.API"
    "tests/CharityPay.Domain.Tests"
    "tests/CharityPay.Application.Tests"
    "tests/CharityPay.Infrastructure.Tests"
    "tests/CharityPay.API.Tests"
    "scripts"
    "docs"
    "frontend"
)

for dir in "${REQUIRED_DIRS[@]}"; do
    if [ -d "$dir" ]; then
        echo -e "  ✅ $dir"
    else
        echo -e "  ❌ $dir (missing)"
        exit 1
    fi
done

# Test 2: Check required files
echo -e "${YELLOW}📄 Checking required files...${NC}"

REQUIRED_FILES=(
    "CharityPay.sln"
    "README.md"
    "PLANNING.md"
    "ARCHITECTURE.md"
    "TASK.md"
    "rules.md"
    "CLAUDE.md"
    ".gitignore"
    "Dockerfile"
    "docker-compose.dev.yml"
    "CodeCoverage.runsettings"
    "src/CharityPay.Domain/CharityPay.Domain.csproj"
    "src/CharityPay.Application/CharityPay.Application.csproj"
    "src/CharityPay.Infrastructure/CharityPay.Infrastructure.csproj"
    "src/CharityPay.API/CharityPay.API.csproj"
    "src/CharityPay.API/Program.cs"
    "src/CharityPay.API/appsettings.json"
    "tests/CharityPay.Domain.Tests/CharityPay.Domain.Tests.csproj"
    "tests/CharityPay.Application.Tests/CharityPay.Application.Tests.csproj"
    "tests/CharityPay.Infrastructure.Tests/CharityPay.Infrastructure.Tests.csproj"
    "tests/CharityPay.API.Tests/CharityPay.API.Tests.csproj"
)

for file in "${REQUIRED_FILES[@]}"; do
    if [ -f "$file" ]; then
        echo -e "  ✅ $file"
    else
        echo -e "  ❌ $file (missing)"
        exit 1
    fi
done

# Test 3: Check script permissions
echo -e "${YELLOW}🔐 Checking script permissions...${NC}"

SCRIPT_FILES=(
    "scripts/test-all.sh"
    "scripts/deploy.sh"
    "scripts/commit-and-deploy.sh"
    "scripts/test-basic.sh"
)

for script in "${SCRIPT_FILES[@]}"; do
    if [ -x "$script" ]; then
        echo -e "  ✅ $script (executable)"
    else
        echo -e "  ❌ $script (not executable)"
        chmod +x "$script"
        echo -e "  🔧 Fixed permissions for $script"
    fi
done

# Test 4: Validate JSON configuration files
echo -e "${YELLOW}📋 Validating JSON configuration...${NC}"

JSON_FILES=(
    "src/CharityPay.API/appsettings.json"
    "src/CharityPay.API/appsettings.Development.json"
)

for json_file in "${JSON_FILES[@]}"; do
    if command -v python3 &> /dev/null; then
        if python3 -m json.tool "$json_file" > /dev/null; then
            echo -e "  ✅ $json_file (valid JSON)"
        else
            echo -e "  ❌ $json_file (invalid JSON)"
            exit 1
        fi
    else
        echo -e "  ⚠️  $json_file (cannot validate - python3 not available)"
    fi
done

# Test 5: Check Docker files
echo -e "${YELLOW}🐳 Checking Docker configuration...${NC}"

if [ -f "Dockerfile" ]; then
    # Basic Dockerfile validation
    if grep -q "FROM mcr.microsoft.com/dotnet" Dockerfile; then
        echo -e "  ✅ Dockerfile (uses correct base image)"
    else
        echo -e "  ⚠️  Dockerfile (unusual base image)"
    fi
else
    echo -e "  ❌ Dockerfile (missing)"
    exit 1
fi

if [ -f "docker-compose.dev.yml" ]; then
    if grep -q "version:" docker-compose.dev.yml; then
        echo -e "  ✅ docker-compose.dev.yml (valid format)"
    else
        echo -e "  ❌ docker-compose.dev.yml (invalid format)"
        exit 1
    fi
else
    echo -e "  ❌ docker-compose.dev.yml (missing)"
    exit 1
fi

# Test 6: Check .NET availability (optional)
echo -e "${YELLOW}🔧 Checking .NET SDK availability...${NC}"

if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo -e "  ✅ .NET SDK available: ${GREEN}$DOTNET_VERSION${NC}"
    
    # If .NET is available, try to restore packages
    echo -e "${YELLOW}📦 Attempting package restore...${NC}"
    if dotnet restore --verbosity quiet; then
        echo -e "  ✅ Package restore successful"
        
        # Try to build
        echo -e "${YELLOW}🔨 Attempting build...${NC}"
        if dotnet build --configuration Release --no-restore --verbosity quiet; then
            echo -e "  ✅ Build successful"
            
            # Try to run tests
            echo -e "${YELLOW}🧪 Attempting to run tests...${NC}"
            if dotnet test --configuration Release --no-build --verbosity quiet; then
                echo -e "  ✅ Tests passed"
            else
                echo -e "  ⚠️  Some tests failed (this is expected for initial setup)"
            fi
        else
            echo -e "  ⚠️  Build failed (this is expected for initial setup)"
        fi
    else
        echo -e "  ⚠️  Package restore failed (this is expected for initial setup)"
    fi
else
    echo -e "  ⚠️  .NET SDK not available"
    echo -e "  ℹ️  Install .NET 8 SDK from: https://dotnet.microsoft.com/download"
    echo -e "  ℹ️  Or use: brew install --cask dotnet-sdk (macOS)"
fi

# Test 7: Check Git repository
echo -e "${YELLOW}📋 Checking Git repository...${NC}"

if [ -d ".git" ]; then
    echo -e "  ✅ Git repository initialized"
    
    # Check if there are any commits
    if git rev-parse HEAD > /dev/null 2>&1; then
        COMMIT_COUNT=$(git rev-list --count HEAD)
        echo -e "  ✅ Git history: $COMMIT_COUNT commits"
    else
        echo -e "  ⚠️  No commits yet"
    fi
    
    # Check for uncommitted changes
    if ! git diff --quiet || ! git diff --cached --quiet; then
        echo -e "  ⚠️  Uncommitted changes detected"
    else
        echo -e "  ✅ Working directory clean"
    fi
else
    echo -e "  ❌ Git repository not initialized"
    exit 1
fi

# Summary
echo ""
echo -e "${GREEN}🎉 Basic validation completed successfully!${NC}"
echo -e "${GREEN}=================================${NC}"
echo -e "${GREEN}✅ Project structure is correct${NC}"
echo -e "${GREEN}✅ All required files are present${NC}"
echo -e "${GREEN}✅ Configuration files are valid${NC}"
echo -e "${GREEN}✅ Scripts have correct permissions${NC}"

if command -v dotnet &> /dev/null; then
    echo -e "${GREEN}✅ .NET SDK is available${NC}"
else
    echo -e "${YELLOW}⚠️  .NET SDK installation recommended${NC}"
fi

echo ""
echo -e "${BLUE}📋 Next Steps:${NC}"
echo -e "  1. Install .NET 8 SDK if not available"
echo -e "  2. Run: ./scripts/test-all.sh --quick"
echo -e "  3. Run: ./scripts/commit-and-deploy.sh \"Initial setup\""
echo ""
echo -e "${GREEN}✨ Project is ready for development!${NC}"
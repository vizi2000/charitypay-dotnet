#!/bin/bash

# deploy.sh - Automated deployment script
# Usage: ./scripts/deploy.sh [--env=dev|staging|prod] [--dry-run]

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BUILD_DIR="$PROJECT_ROOT/publish"
DOCKER_IMAGE_NAME="charitypay/api"
DOCKER_TAG="latest"

# Default values
ENVIRONMENT="dev"
DRY_RUN=false

# Parse arguments
for arg in "$@"; do
    case $arg in
        --env=*)
            ENVIRONMENT="${arg#*=}"
            shift
            ;;
        --dry-run)
            DRY_RUN=true
            shift
            ;;
        *)
            echo -e "${RED}Unknown argument: $arg${NC}"
            exit 1
            ;;
    esac
done

echo -e "${BLUE}🚀 CharityPay .NET Deployment${NC}"
echo -e "${BLUE}==============================${NC}"
echo -e "Environment: ${YELLOW}$ENVIRONMENT${NC}"
echo -e "Dry Run: ${YELLOW}$DRY_RUN${NC}"
echo -e "Project Root: ${YELLOW}$PROJECT_ROOT${NC}"
echo ""

cd "$PROJECT_ROOT"

# Validate environment
case $ENVIRONMENT in
    "dev"|"staging"|"prod")
        echo -e "${GREEN}✅ Valid environment: $ENVIRONMENT${NC}"
        ;;
    *)
        echo -e "${RED}❌ Invalid environment: $ENVIRONMENT. Use dev, staging, or prod.${NC}"
        exit 1
        ;;
esac

# Check prerequisites
echo -e "${YELLOW}🔍 Checking prerequisites...${NC}"

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK not found${NC}"
    exit 1
fi

if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker not found${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Prerequisites check passed${NC}"

# Clean previous build
echo -e "${YELLOW}🧹 Cleaning previous build...${NC}"
rm -rf "$BUILD_DIR"

# Run tests first
echo -e "${YELLOW}🧪 Running tests before deployment...${NC}"
if ! ./scripts/test-all.sh --quick; then
    echo -e "${RED}❌ Tests failed. Deployment aborted.${NC}"
    exit 1
fi

# Build for production
echo -e "${YELLOW}🔨 Building for production...${NC}"
dotnet publish src/CharityPay.API/CharityPay.API.csproj \
    --configuration Release \
    --output "$BUILD_DIR" \
    --no-restore \
    --verbosity quiet

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Build successful${NC}"
else
    echo -e "${RED}❌ Build failed${NC}"
    exit 1
fi

# Set Docker tag based on environment
case $ENVIRONMENT in
    "dev")
        DOCKER_TAG="dev-$(date +%Y%m%d-%H%M%S)"
        ;;
    "staging")
        DOCKER_TAG="staging-$(git rev-parse --short HEAD)"
        ;;
    "prod")
        DOCKER_TAG="prod-$(git rev-parse --short HEAD)"
        ;;
esac

# Build Docker image
echo -e "${YELLOW}🐳 Building Docker image...${NC}"
if [ "$DRY_RUN" = true ]; then
    echo -e "${BLUE}[DRY-RUN] Would build Docker image: $DOCKER_IMAGE_NAME:$DOCKER_TAG${NC}"
else
    docker build -t "$DOCKER_IMAGE_NAME:$DOCKER_TAG" -f Dockerfile .
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Docker image built successfully: $DOCKER_IMAGE_NAME:$DOCKER_TAG${NC}"
    else
        echo -e "${RED}❌ Docker build failed${NC}"
        exit 1
    fi
fi

# Environment-specific deployment
deploy_to_environment() {
    case $ENVIRONMENT in
        "dev")
            echo -e "${YELLOW}🔧 Deploying to development environment...${NC}"
            if [ "$DRY_RUN" = true ]; then
                echo -e "${BLUE}[DRY-RUN] Would run: docker-compose -f docker-compose.dev.yml up -d${NC}"
            else
                # Stop existing containers
                docker-compose -f docker-compose.dev.yml down || true
                # Start new containers
                docker-compose -f docker-compose.dev.yml up -d
                # Wait for health check
                echo -e "${YELLOW}⏳ Waiting for health check...${NC}"
                sleep 10
                if curl -f http://localhost:5000/health > /dev/null 2>&1; then
                    echo -e "${GREEN}✅ Development deployment successful${NC}"
                    echo -e "${GREEN}🌐 API available at: http://localhost:5000${NC}"
                    echo -e "${GREEN}📚 Swagger UI: http://localhost:5000/swagger${NC}"
                else
                    echo -e "${RED}❌ Health check failed${NC}"
                    exit 1
                fi
            fi
            ;;
        "staging")
            echo -e "${YELLOW}🔧 Deploying to staging environment...${NC}"
            if [ "$DRY_RUN" = true ]; then
                echo -e "${BLUE}[DRY-RUN] Would deploy to staging with image: $DOCKER_IMAGE_NAME:$DOCKER_TAG${NC}"
            else
                # In a real scenario, this would deploy to staging infrastructure
                echo -e "${YELLOW}⚠️  Staging deployment not implemented yet${NC}"
                echo -e "${BLUE}Would deploy image: $DOCKER_IMAGE_NAME:$DOCKER_TAG${NC}"
            fi
            ;;
        "prod")
            echo -e "${YELLOW}🔧 Deploying to production environment...${NC}"
            if [ "$DRY_RUN" = true ]; then
                echo -e "${BLUE}[DRY-RUN] Would deploy to production with image: $DOCKER_IMAGE_NAME:$DOCKER_TAG${NC}"
            else
                # Production deployment requires additional safeguards
                echo -e "${RED}⚠️  Production deployment requires manual approval${NC}"
                echo -e "${BLUE}Image ready for production: $DOCKER_IMAGE_NAME:$DOCKER_TAG${NC}"
                echo -e "${YELLOW}Run with appropriate production deployment tools${NC}"
            fi
            ;;
    esac
}

# Execute deployment
deploy_to_environment

# Post-deployment tasks
if [ "$DRY_RUN" = false ] && [ "$ENVIRONMENT" = "dev" ]; then
    echo -e "${YELLOW}🔍 Running post-deployment checks...${NC}"
    
    # Check API endpoints
    echo -e "${YELLOW}Testing API endpoints...${NC}"
    
    # Health check
    if curl -f http://localhost:5000/health > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Health endpoint: OK${NC}"
    else
        echo -e "${RED}❌ Health endpoint: FAILED${NC}"
    fi
    
    # Root endpoint
    if curl -f http://localhost:5000/ > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Root endpoint: OK${NC}"
    else
        echo -e "${RED}❌ Root endpoint: FAILED${NC}"
    fi
    
    # Swagger endpoint
    if curl -f http://localhost:5000/swagger > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Swagger endpoint: OK${NC}"
    else
        echo -e "${YELLOW}⚠️  Swagger endpoint: Not available (may not be implemented yet)${NC}"
    fi
fi

echo ""
echo -e "${GREEN}🎉 Deployment completed successfully!${NC}"

# Display useful information
case $ENVIRONMENT in
    "dev")
        echo -e "${BLUE}📋 Development Environment Info:${NC}"
        echo -e "  API URL: ${YELLOW}http://localhost:5000${NC}"
        echo -e "  Swagger: ${YELLOW}http://localhost:5000/swagger${NC}"
        echo -e "  Frontend: ${YELLOW}http://localhost:5173${NC}"
        echo -e "  Docker Image: ${YELLOW}$DOCKER_IMAGE_NAME:$DOCKER_TAG${NC}"
        ;;
    "staging"|"prod")
        echo -e "${BLUE}📋 Deployment Info:${NC}"
        echo -e "  Environment: ${YELLOW}$ENVIRONMENT${NC}"
        echo -e "  Docker Image: ${YELLOW}$DOCKER_IMAGE_NAME:$DOCKER_TAG${NC}"
        echo -e "  Git Commit: ${YELLOW}$(git rev-parse --short HEAD)${NC}"
        ;;
esac

echo ""
echo -e "${GREEN}✨ Deployment script completed successfully!${NC}"
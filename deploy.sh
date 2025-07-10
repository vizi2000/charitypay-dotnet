#!/bin/bash

# CharityPay Docker Deployment Script
# This script deploys the CharityPay .NET application using Docker

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Configuration
ENVIRONMENT=${1:-"dev"}
COMPOSE_FILE="docker-compose.yml"

case $ENVIRONMENT in
    "dev"|"development")
        COMPOSE_FILE="docker-compose.dev.yml"
        ENV_FILE=".env.dev"
        ;;
    "simple")
        COMPOSE_FILE="docker-compose.simple.yml"
        ENV_FILE=".env.dev"
        ;;
    "prod"|"production")
        COMPOSE_FILE="docker-compose.yml"
        ENV_FILE=".env"
        ;;
    *)
        print_error "Invalid environment: $ENVIRONMENT"
        echo "Usage: $0 [dev|simple|prod]"
        exit 1
        ;;
esac

print_status "🚀 Deploying CharityPay in $ENVIRONMENT environment"
print_status "📁 Using compose file: $COMPOSE_FILE"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    print_error "Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if compose file exists
if [ ! -f "$COMPOSE_FILE" ]; then
    print_error "Compose file $COMPOSE_FILE not found!"
    exit 1
fi

# Create environment file if it doesn't exist
if [ ! -f "$ENV_FILE" ] && [ "$ENV_FILE" != ".env.dev" ]; then
    print_warning "Environment file $ENV_FILE not found. Creating from template..."
    if [ -f ".env.example" ]; then
        cp .env.example "$ENV_FILE"
        print_warning "Please edit $ENV_FILE with your configuration before proceeding."
        exit 1
    fi
fi

# Stop existing containers
print_status "🛑 Stopping existing containers..."
docker-compose -f "$COMPOSE_FILE" down --remove-orphans || true

# Remove old images if production
if [ "$ENVIRONMENT" = "prod" ] || [ "$ENVIRONMENT" = "production" ]; then
    print_status "🗑️ Removing old images..."
    docker system prune -f || true
fi

# Build and start containers
print_status "🔨 Building and starting containers..."
if [ -f "$ENV_FILE" ]; then
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --build
else
    docker-compose -f "$COMPOSE_FILE" up -d --build
fi

# Wait for services to be ready
print_status "⏳ Waiting for services to be ready..."
sleep 10

# Check service health
print_status "🔍 Checking service health..."
services=$(docker-compose -f "$COMPOSE_FILE" ps --services)

for service in $services; do
    if docker-compose -f "$COMPOSE_FILE" ps "$service" | grep -q "Up"; then
        print_success "✅ $service is running"
    else
        print_error "❌ $service failed to start"
        docker-compose -f "$COMPOSE_FILE" logs "$service" | tail -20
    fi
done

# Show running containers
print_status "📊 Container status:"
docker-compose -f "$COMPOSE_FILE" ps

# Display access information
print_success "🎉 Deployment completed!"
echo ""
echo "📋 Access Information:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if [ "$ENVIRONMENT" = "simple" ]; then
    echo "🗄️  Database (Adminer):  http://localhost:8080"
    echo "   Server: postgres"
    echo "   Database: charitypay_dev"
    echo "   Username: postgres"
    echo "   Password: dev_password_123"
elif [ "$ENVIRONMENT" = "dev" ] || [ "$ENVIRONMENT" = "development" ]; then
    echo "🌐 API:                 http://localhost:5000"
    echo "🗄️  Database (Adminer):  http://localhost:8080"
    echo "📊 Frontend:            http://localhost:5173"
    echo "📈 Redis:               localhost:6379"
else
    echo "🌐 Website:             http://localhost:80"
    echo "🔒 HTTPS:               https://localhost:443"
    echo "📊 Grafana:             http://localhost:3000"
    echo "📈 Prometheus:          http://localhost:9090"
fi

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "🔧 Management commands:"
echo "  View logs:    docker-compose -f $COMPOSE_FILE logs -f [service]"
echo "  Stop all:     docker-compose -f $COMPOSE_FILE down"
echo "  Restart:      docker-compose -f $COMPOSE_FILE restart [service]"
echo "  Shell access: docker-compose -f $COMPOSE_FILE exec [service] /bin/sh"
echo ""

if [ "$ENVIRONMENT" != "simple" ]; then
    print_status "🔍 Testing API health..."
    sleep 5
    if [ "$ENVIRONMENT" = "dev" ] || [ "$ENVIRONMENT" = "development" ]; then
        API_URL="http://localhost:5000/health"
    else
        API_URL="http://localhost:80/health"
    fi
    
    if curl -f -s "$API_URL" > /dev/null; then
        print_success "✅ API health check passed"
    else
        print_warning "⚠️  API health check failed - service may still be starting"
        print_status "   Run: docker-compose -f $COMPOSE_FILE logs api"
    fi
fi

print_success "🚀 CharityPay deployment ready!"
echo "📚 For more information, check the documentation in README.md"
echo ""
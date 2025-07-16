#!/bin/bash

# Check if .env.production exists
if [ ! -f .env.production ]; then
    echo "Error: .env.production file not found!"
    echo "Please copy .env.production.example to .env.production and configure it with your settings."
    exit 1
fi

# Load environment variables
export $(cat .env.production | grep -v '^#' | xargs)

# Validate required environment variables
required_vars=("EXTERNAL_IP" "DB_PASSWORD" "JWT_SECRET")
for var in "${required_vars[@]}"; do
    if [ -z "${!var}" ]; then
        echo "Error: $var is not set in .env.production"
        exit 1
    fi
done

echo "Starting CharityPay in production mode..."
echo "External IP: $EXTERNAL_IP"
echo "API will be available at: http://$EXTERNAL_IP:8081"
echo "Frontend will be available at: http://$EXTERNAL_IP:5174"

# Build and start services
docker-compose -f docker-compose.production.yml build
docker-compose -f docker-compose.production.yml up -d

# Show logs
echo ""
echo "Services are starting up. You can view logs with:"
echo "docker-compose -f docker-compose.production.yml logs -f"
echo ""
echo "To stop services, run:"
echo "docker-compose -f docker-compose.production.yml down"
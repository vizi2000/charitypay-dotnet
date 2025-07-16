# CharityPay Production Deployment Guide

This guide explains how to deploy CharityPay in a production environment to make it accessible from external IP addresses.

## Prerequisites

- Docker and Docker Compose installed
- External IP address or domain name
- Ports 8081 (API) and 5174 (Frontend) available

## Quick Start

1. **Create production environment file**
   ```bash
   cp .env.production.example .env.production
   ```

2. **Edit `.env.production`** with your configuration:
   ```env
   # Your external IP address (required)
   EXTERNAL_IP=194.181.240.37
   
   # Or your domain name (optional)
   DOMAIN_NAME=yourdomain.com
   
   # Security settings (change all of these!)
   DB_PASSWORD=your-secure-database-password
   REDIS_PASSWORD=your-secure-redis-password
   JWT_SECRET=your-super-secret-jwt-key-that-should-be-at-least-32-characters-long
   
   # Polcard/Fiserv settings (if you have them)
   POLCARD_CLIENT_ID=your-client-id
   POLCARD_CLIENT_SECRET=your-client-secret
   POLCARD_WEBHOOK_SECRET=your-webhook-secret
   ```

3. **Start the production environment**
   ```bash
   ./start-production.sh
   ```

## Manual Deployment

If you prefer to run commands manually:

1. **Load environment variables**
   ```bash
   export $(cat .env.production | grep -v '^#' | xargs)
   ```

2. **Build and start services**
   ```bash
   docker-compose -f docker-compose.production.yml build
   docker-compose -f docker-compose.production.yml up -d
   ```

3. **Check logs**
   ```bash
   docker-compose -f docker-compose.production.yml logs -f
   ```

## Accessing the Application

After deployment, your application will be available at:
- **Frontend**: `http://YOUR_EXTERNAL_IP:5174`
- **API**: `http://YOUR_EXTERNAL_IP:8081`
- **API Documentation**: `http://YOUR_EXTERNAL_IP:8081/swagger`

## Production Configuration Details

### CORS Configuration
The production setup automatically configures CORS to allow requests from:
- `http://YOUR_EXTERNAL_IP:5174`
- `https://YOUR_EXTERNAL_IP:5174`
- `http://YOUR_DOMAIN_NAME` (if configured)
- `https://YOUR_DOMAIN_NAME` (if configured)

### Security Features
- JWT authentication with configurable secret
- Rate limiting enabled
- Security headers configured
- HTTPS ready (configure reverse proxy for SSL)

### Database
- PostgreSQL 15 with persistent volume
- Automatic database creation and seeding on first run
- Connection pooling configured

### Frontend
- Production build with optimized assets
- Nginx serving static files
- Gzip compression enabled
- Proper cache headers

## Troubleshooting

### CORS Errors
If you still see CORS errors:
1. Ensure `EXTERNAL_IP` in `.env.production` matches your actual IP
2. Restart the API container: `docker-compose -f docker-compose.production.yml restart charitypay-api`
3. Check API logs: `docker-compose -f docker-compose.production.yml logs charitypay-api`

### Connection Refused
If frontend can't reach the API:
1. Verify API is running: `docker ps`
2. Check API health: `curl http://YOUR_EXTERNAL_IP:8081/health`
3. Ensure firewall allows ports 8081 and 5174

### Database Issues
If you need to reset the database:
```bash
docker-compose -f docker-compose.production.yml down -v
docker-compose -f docker-compose.production.yml up -d
```

## Monitoring

View container status:
```bash
docker-compose -f docker-compose.production.yml ps
```

View logs for specific service:
```bash
docker-compose -f docker-compose.production.yml logs -f charitypay-api
docker-compose -f docker-compose.production.yml logs -f charitypay-frontend
```

## Stopping the Application

```bash
docker-compose -f docker-compose.production.yml down
```

To also remove volumes (database data):
```bash
docker-compose -f docker-compose.production.yml down -v
```

## Next Steps

### For Production Use
1. **SSL/TLS**: Set up a reverse proxy (nginx/traefik) with Let's Encrypt
2. **Domain Name**: Configure a proper domain instead of IP address
3. **Monitoring**: Set up monitoring (Prometheus, Grafana)
4. **Backups**: Configure automated database backups
5. **Secrets**: Use proper secret management (Docker secrets, Vault)

### Environment Variables Reference

| Variable | Description | Example |
|----------|-------------|---------|
| EXTERNAL_IP | Your server's external IP | 194.181.240.37 |
| DOMAIN_NAME | Your domain (optional) | charity.example.com |
| DB_PASSWORD | PostgreSQL password | strong-password-here |
| REDIS_PASSWORD | Redis password | another-strong-password |
| JWT_SECRET | JWT signing key (32+ chars) | your-256-bit-secret-key-here |
| POLCARD_CLIENT_ID | Polcard API client ID | provided-by-polcard |
| POLCARD_CLIENT_SECRET | Polcard API secret | provided-by-polcard |
| POLCARD_WEBHOOK_SECRET | Webhook validation secret | provided-by-polcard |
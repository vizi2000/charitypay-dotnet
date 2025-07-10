# CharityPay Docker Deployment Guide

This guide explains how to deploy CharityPay .NET application using Docker containers.

## 🚀 Quick Start

### Prerequisites
- Docker Desktop installed and running
- Git (to clone the repository)
- 8GB+ RAM recommended

### 1. Simple Database-Only Deployment

Perfect for development and testing:

```bash
# Start just PostgreSQL + Adminer
./deploy.sh simple
```

**Access:**
- 🗄️ Database Admin: http://localhost:8080
  - Server: `postgres`
  - Database: `charitypay_dev` 
  - Username: `postgres`
  - Password: `dev_password_123`

### 2. Development Deployment

Full development environment with hot reload:

```bash
# Start API + Database + Frontend + Redis
./deploy.sh dev
```

**Access:**
- 🌐 API: http://localhost:5000
- 🗄️ Database Admin: http://localhost:8080
- 📱 Frontend: http://localhost:5173
- 🔄 Redis: localhost:6379

### 3. Production Deployment

Full production stack with monitoring:

```bash
# Copy environment template
cp .env.example .env
# Edit .env with your production settings
nano .env

# Deploy production stack
./deploy.sh prod
```

**Access:**
- 🌐 Website: http://localhost:80
- 🔒 HTTPS: https://localhost:443
- 📊 Grafana: http://localhost:3000
- 📈 Prometheus: http://localhost:9090

## 📋 Available Services

| Service | Development Port | Production Port | Description |
|---------|------------------|-----------------|-------------|
| API | 5000 | 80/443 | CharityPay .NET API |
| Database | 5432 | 5432 | PostgreSQL 15 |
| Redis | 6379 | 6379 | Caching & Sessions |
| Frontend | 5173 | 80/443 | React/Vite UI |
| Adminer | 8080 | 8080 | Database Admin |
| Grafana | 3000 | 3000 | Analytics Dashboard |
| Prometheus | 9090 | 9090 | Metrics Collection |
| Nginx | - | 80/443 | Reverse Proxy |

## 🛠️ Management Commands

### View Logs
```bash
# All services
docker-compose -f docker-compose.dev.yml logs -f

# Specific service
docker-compose -f docker-compose.dev.yml logs -f api
docker-compose -f docker-compose.dev.yml logs -f postgres
```

### Stop Services
```bash
# Stop all
docker-compose -f docker-compose.dev.yml down

# Stop and remove volumes (⚠️ deletes data)
docker-compose -f docker-compose.dev.yml down -v
```

### Restart Services
```bash
# Restart all
docker-compose -f docker-compose.dev.yml restart

# Restart specific service
docker-compose -f docker-compose.dev.yml restart api
```

### Shell Access
```bash
# API container
docker-compose -f docker-compose.dev.yml exec api /bin/sh

# Database container
docker-compose -f docker-compose.dev.yml exec postgres psql -U postgres -d charitypay_dev
```

### Database Management
```bash
# Run migrations (when available)
docker-compose -f docker-compose.dev.yml exec api dotnet ef database update

# Create backup
docker exec charitypay-postgres-dev pg_dump -U postgres charitypay_dev > backup.sql

# Restore backup
docker exec -i charitypay-postgres-dev psql -U postgres charitypay_dev < backup.sql
```

## 🔧 Configuration

### Environment Variables

Key environment variables (see `.env.example` for full list):

```env
# Database
DB_PASSWORD=your_secure_password

# JWT Authentication
JWT_SECRET=your-super-secret-jwt-key-minimum-32-characters

# Fiserv Payment Gateway
FISERV_API_KEY=your-fiserv-api-key
FISERV_API_SECRET=your-fiserv-api-secret
FISERV_STORE_ID=your-store-id

# Application URLs
WEBHOOK_BASE_URL=https://yourdomain.com
FRONTEND_BASE_URL=https://yourdomain.com
```

### Docker Compose Files

| File | Purpose | Environment |
|------|---------|-------------|
| `docker-compose.simple.yml` | Database only | Development/Testing |
| `docker-compose.dev.yml` | Full dev stack | Development |
| `docker-compose.yml` | Production stack | Production |

## 🐛 Troubleshooting

### Common Issues

**1. Port Already in Use**
```bash
# Find process using port
lsof -i :5000
# Kill process
kill -9 <PID>
```

**2. Database Connection Failed**
```bash
# Check database health
docker-compose -f docker-compose.dev.yml exec postgres pg_isready -U postgres
# View database logs
docker-compose -f docker-compose.dev.yml logs postgres
```

**3. API Not Starting**
```bash
# Check API logs
docker-compose -f docker-compose.dev.yml logs api
# Rebuild API container
docker-compose -f docker-compose.dev.yml build api
```

**4. Out of Disk Space**
```bash
# Clean up Docker
docker system prune -a
docker volume prune
```

### Health Checks

```bash
# Check all services
docker-compose -f docker-compose.dev.yml ps

# Test API health
curl http://localhost:5000/health

# Test database connection
docker-compose -f docker-compose.dev.yml exec postgres psql -U postgres -d charitypay_dev -c "SELECT 1;"
```

## 📊 Monitoring

### Development
- **API Logs**: `docker-compose logs -f api`
- **Database**: Adminer at http://localhost:8080
- **Hot Reload**: Code changes auto-reload in development

### Production
- **Grafana Dashboards**: http://localhost:3000
  - Username: `admin`
  - Password: Set in `GRAFANA_PASSWORD` env var
- **Prometheus Metrics**: http://localhost:9090
- **Application Logs**: Structured JSON logs with Serilog

## 🔒 Security

### Development
- Uses default passwords (not secure)
- No SSL/TLS encryption
- Debug logging enabled

### Production
- **Change all default passwords**
- Configure SSL certificates
- Use secure JWT secrets
- Enable firewall rules
- Regular security updates

### SSL Setup (Production)

1. Place certificates in `docker/nginx/ssl/`:
   - `cert.pem` (certificate)
   - `private.key` (private key)

2. Update `docker/nginx/conf.d/charitypay.conf` for HTTPS

## 📚 Next Steps

1. **Development**: Start with `./deploy.sh simple` to get database running
2. **API Development**: Use `./deploy.sh dev` for full development environment  
3. **Production**: Configure `.env` and deploy with `./deploy.sh prod`
4. **Monitoring**: Set up Grafana dashboards for your metrics
5. **Backup**: Implement automated database backups
6. **CI/CD**: Integrate with GitHub Actions for automated deployments

## 🆘 Support

For issues and questions:
- Check logs: `docker-compose logs [service]`
- Review environment variables in `.env`
- Verify Docker resources (CPU/Memory)
- Check port availability
- Consult application documentation

---

**🚀 CharityPay Docker Deployment - Ready for Development & Production!**
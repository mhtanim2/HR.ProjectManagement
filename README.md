# HR Project Management API

A Clean Architecture .NET 10 Web API for managing HR projects, teams, and tasks with PostgreSQL database.

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Local Development](#local-development)
- [Running Tests](#running-tests)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Docker Deployment](#docker-deployment)
- [Manual Deployment](#manual-deployment)
- [CI/CD Pipelines](#cicd-pipelines)
- [Environment Configuration](#environment-configuration)
- [Database Migrations](#database-migrations)
- [Troubleshooting](#troubleshooting)

## Features

- **User Management**: Create, update, and manage users with role-based access control
- **Team Management**: Organize users into teams with membership management
- **Task Management**: Create, assign, and track tasks with status workflow
- **Authentication**: JWT-based authentication with refresh tokens
- **Password Reset**: Secure password reset functionality
- **Audit Trail**: Automatic tracking of created/modified dates and users
- **Clean Architecture**: Separation of concerns with layered architecture
- **Comprehensive Testing**: 65 tests (46 unit + 19 integration)

## Tech Stack

- **Framework**: .NET 10.0
- **Database**: PostgreSQL 15
- **ORM**: Entity Framework Core 10.0
- **Authentication**: JWT Bearer Tokens
- **Testing**: xUnit, Moq, FluentAssertions
- **API Documentation**: Swagger/OpenAPI
- **Logging**: Serilog
- **Containerization**: Docker & Docker Compose
- **CI/CD**: GitHub Actions / Azure DevOps

## Prerequisites

### Required Software

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (10.0.101 or later)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (4.0+ for Windows/Mac)
- [Docker Compose](https://docs.docker.com/compose/install/) (included with Docker Desktop)
- Git

### Verify Installation

```bash
dotnet --version    # Should show 10.0.101
docker --version    # Should show 20.10+ or similar
docker-compose --version
```

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/mhtanim2/HR.ProjectManagement.git
cd HR.ProjectManagement
```

### 2. Start with Docker Compose (Recommended)

```bash
# Start API and database
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### 3. Access the Application

- **API**: http://localhost:6000
- **Swagger UI**: http://localhost:6000 (root URL)
- **Database**: localhost:5432

## Local Development

### Docker Compose Development Workflow

This is the **recommended** approach for local development.

#### Starting the Development Environment

```bash
# Build and start all services
docker-compose up --build

# Or run in detached mode
docker-compose up -d --build
```

#### Hot Reload

When using Docker Compose, the application supports hot reload:

1. Make code changes locally
2. Changes are reflected in the container (due to volume mount)
3. The API automatically recompiles

#### Viewing Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f hr.projectmanagement
docker-compose logs -f hr-projectmanagement-db
```

#### Stopping Services

```bash
# Stop and remove containers
docker-compose down

# Stop and remove with volumes
docker-compose down -v
```

### Manual Local Development (Alternative)

If you prefer to run without Docker:

#### 1. Install PostgreSQL

```bash
# Windows: Download from https://www.postgresql.org/download/windows/
# Mac: brew install postgresql@15
# Linux: sudo apt-get install postgresql-15
```

#### 2. Create Database

```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE "ProjectManagement";
```

#### 3. Update Connection String

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "ApplicationDatabaseConnectionString": "Server=localhost;Port=5432;Database=ProjectManagement;User Id=postgres;Password=your_password"
  }
}
```

#### 4. Run Migrations

```bash
dotnet ef database update --project HR.ProjectManagement --startup-project HR.ProjectManagement
```

#### 5. Run the Application

```bash
dotnet run --project HR.ProjectManagement
```

Access at: http://localhost:5000

## Running Tests

### Locally (Without Docker)

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "FullyQualifiedName~UnitTests"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~UserServiceTest"
```

### In Docker (CI/CD Style)

```bash
# Run all tests in containers
docker-compose -f docker-compose.test.yml up

# Run only unit tests
docker-compose -f docker-compose.test.yml run --rm unit-tests

# Run only integration tests
docker-compose -f docker-compose.test.yml run --rm integration-tests

# Run tests with coverage
docker-compose -f docker-compose.test.yml run --rm all-tests

# View test results
ls test-results/
```

### Test Coverage

Test results are saved in `test-results/` directory in TRX format and OpenCover/Cobertura XML format.

## API Documentation

### Swagger UI

When running the application, Swagger UI is available at:

- **Docker**: http://localhost:6000
- **Manual**: http://localhost:5000

### Authentication Endpoints

1. **POST** `/api/auth/login` - User login
2. **POST** `/api/auth/register` - User registration
3. **POST** `/api/auth/refresh-token` - Refresh access token
4. **POST** `/api/auth/logout` - Logout user

### Example API Usage

```bash
# Register a new user
curl -X POST http://localhost:6000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "SecurePass123!",
    "fullName": "Admin User",
    "role": "Admin"
  }'

# Login
curl -X POST http://localhost:6000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "SecurePass123!"
  }'

# Get all teams (with JWT token)
curl -X GET http://localhost:6000/api/teams \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Project Structure

```
HR.ProjectManagement/
├── HR.ProjectManagement/              # Main API project
│   ├── Controllers/                   # API Controllers
│   ├── DataContext/                   # EF Core DbContext
│   ├── DTOs/                          # Data Transfer Objects
│   ├── Entities/                      # Domain Entities
│   ├── Extensions/                    # Dependency injection setup
│   ├── Migrations/                    # Database migrations
│   ├── Middleware/                    # Custom middleware
│   ├── Repositories/                  # Data access layer
│   ├── Services/                      # Business logic layer
│   ├── Validations/                   # FluentValidation validators
│   ├── appsettings.json               # Base configuration
│   ├── appsettings.Development.json   # Development overrides
│   ├── appsettings.Production.json    # Production configuration
│   └── Program.cs                     # Application entry point
├── HR.ProjectManagement.UnitTests/    # Unit tests (xUnit + Moq)
├── HR.ProjectManagement.Persistence.IntegrationTests/  # Integration tests
├── docker-compose.yml                 # Base Docker Compose
├── docker-compose.override.yml        # Development overrides
├── docker-compose.prod.yml            # Production overrides
├── docker-compose.test.yml            # Test execution
├── Dockerfile                         # Production API image
├── Dockerfile.test                    # Test runner image
├── scripts/                           # Migration scripts
│   ├── migrate.sh
│   └── migrate.ps1
├── .github/workflows/                 # CI/CD
│   ├── docker-build.yml
│   └── deploy.yml
└── README.md                          # This file
```

## Docker Deployment

### Build Production Image

```bash
# Build image locally
docker build -t hrprojectmanagement:latest -f HR.ProjectManagement/Dockerfile .

# Or use Docker Compose
docker-compose -f docker-compose.prod.yml build
```

### Deploy to Production

#### 1. Set Environment Variables

Create `.env` file:

```bash
cp .env.example .env
# Edit .env with your values
```

#### 2. Run with Docker Compose

```bash
# Deploy to production
docker-compose -f docker-compose.prod.yml up -d

# View logs
docker-compose -f docker-compose.prod.yml logs -f

# Stop production
docker-compose -f docker-compose.prod.yml down
```

#### 3. Run Database Migrations

```bash
docker-compose -f docker-compose.prod.yml exec hr.projectmanagement \
  dotnet ef database update
```

### Production Considerations

- **Persistent Data**: Database data is stored in Docker volume `postgres-prod-data`
- **Backups**: Regularly backup PostgreSQL data
- **HTTPS**: Configure reverse proxy (nginx/traefik) for SSL termination
- **Secrets**: Use Docker Secrets or environment variables for sensitive data
- **Health Checks**: Configure health check endpoint for load balancers

## Manual Deployment

### Deploy to IIS (Windows)

#### 1. Publish the Application

```bash
dotnet publish HR.ProjectManagement -c Release -o ./publish
```

#### 2. Create Website in IIS

1. Open IIS Manager
2. Add Website
3. Physical path: `./publish` folder
4. Binding: HTTP on port 80
5. Configure Application Pool: .NET CLR Version = No Managed Code

#### 3. Configure Connection String

Edit `publish/appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "ApplicationDatabaseConnectionString": "Server=your_server;Database=ProjectManagement;User Id=your_user;Password=your_password"
  }
}
```

#### 4. Install ASP.NET Core Hosting Bundle

Download from: https://dotnet.microsoft.com/download/dotnet/10.0

### Deploy to Linux

#### 1. Publish Self-Contained Application

```bash
dotnet publish HR.ProjectManagement -c Release \
  -r linux-x64 \
  --self-contained true \
  -o ./publish
```

#### 2. Copy to Server

```bash
scp -r ./publish/* user@server:/var/www/hrprojectmanagement/
```

#### 3. Create Systemd Service

Create `/etc/systemd/system/hrprojectmanagement.service`:

```ini
[Unit]
Description=HR Project Management API
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/hrprojectmanagement
ExecStart=/var/www/hrprojectmanagement/HR.ProjectManagement
Restart=always
RestartSec=10
SyslogIdentifier=hrprojectmanagement
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

#### 4. Start Service

```bash
sudo systemctl daemon-reload
sudo systemctl enable hrprojectmanagement
sudo systemctl start hrprojectmanagement
sudo systemctl status hrprojectmanagement
```

#### 5. Configure Reverse Proxy (Nginx)

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

## CI/CD Pipelines

### GitHub Actions

The project includes GitHub Actions workflows:

- **`.github/workflows/docker-build.yml`**: Build and test on every push
- **`.github/workflows/deploy.yml`**: Deploy to production on master branch

#### Required GitHub Secrets

```
DOCKER_REGISTRY      # e.g., docker.io, ghcr.io
DOCKER_USERNAME      # Registry username
DOCKER_PASSWORD      # Registry password or token
SERVER_HOST          # Deployment server host
SERVER_USER          # SSH username
SSH_PRIVATE_KEY      # SSH private key for deployment
JWT_KEY              # Production JWT secret key
DB_USER              # Database user
DB_PASSWORD          # Database password
DB_NAME              # Database name
```

### Azure DevOps

Example pipeline is in `.azure-pipelines.yml`.

#### Required Azure DevOps Variables

```
docker.registry      # e.g., your-registry.azurecr.io
DB_USER
DB_PASSWORD
JWT_KEY
```

## Environment Configuration

### Development

Uses `appsettings.Development.json` with:
- Local database connection
- Detailed error messages
- Swagger enabled
- Debug logging

### Production

Uses `appsettings.Production.json` with:
- Environment variables for secrets
- Minimal logging
- Production-ready error handling
- Swagger disabled (or secured)

### User Secrets

For local development with sensitive data:

```bash
cd HR.ProjectManagement
dotnet user-secrets set "Jwt:Key" "your_local_jwt_key"
dotnet user-secrets set "ConnectionStrings:ApplicationDatabaseConnectionString" "Server=localhost;..."
```

### Environment Variables

Production deployment uses environment variables:

```bash
export JWT_KEY="your_production_jwt_key"
export DB_USER="postgres"
export DB_PASSWORD="secure_password"
export DB_NAME="ProjectManagement"
```

## Database Migrations

### Create New Migration

```bash
dotnet ef migrations add MigrationName \
  --project HR.ProjectManagement \
  --startup-project HR.ProjectManagement
```

### Update Database

```bash
# In Docker
docker-compose exec hr.projectmanagement dotnet ef database update

# Local
dotnet ef database update --project HR.ProjectManagement --startup-project HR.ProjectManagement

# Using script
./scripts/migrate.sh  # Linux/Mac
./scripts/migrate.ps1 # Windows
```

### Rollback Migration

```bash
dotnet ef database update PreviousMigrationName \
  --project HR.ProjectManagement \
  --startup-project HR.ProjectManagement
```

### Remove Last Migration

```bash
dotnet ef migrations remove \
  --project HR.ProjectManagement \
  --startup-project HR.ProjectManagement
```

### Generate SQL Script

```bash
dotnet ef migrations script \
  --project HR.ProjectManagement \
  --startup-project HR.ProjectManagement \
  --output migration.sql
```

## Troubleshooting

### Docker Issues

**Problem**: `hr.projectmanagement` container exits immediately

**Solution**: Check logs and ensure database is healthy
```bash
docker-compose logs hr.projectmanagement
docker-compose ps
```

**Problem**: API cannot connect to database

**Solution**: Verify connection string uses service name `hr-projectmanagement-db`, not `localhost`

**Problem**: Port already in use

**Solution**: Change port mapping in `docker-compose.override.yml`
```yaml
ports:
  - "6001:8080"  # Change 6000 to 6001
```

### Database Issues

**Problem**: Database migration fails

**Solution**: Check database connection and credentials
```bash
# Test database connection
docker-compose exec hr-projectmanagement-db psql -U postgres -d ProjectManagement
```

**Problem**: Migration locks database

**Solution**: Kill existing connections
```sql
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = 'ProjectManagement' AND pid <> pg_backend_pid();
```

### Build Issues

**Problem**: `dotnet build` fails with SDK errors

**Solution**: Ensure .NET 10 SDK is installed
```bash
dotnet --list-sdks
# Should show 10.0.101
```

**Problem**: Test project cannot find main project reference

**Solution**: Clean and restore
```bash
dotnet clean
dotnet restore
dotnet build
```

### Performance Issues

**Problem**: API is slow

**Solution**: Check database indexes and queries
```bash
# Enable query logging in Serilog
# Review slow queries in PostgreSQL logs
```

## Testing Best Practices

This project follows comprehensive testing practices. See [TESTING_BEST_PRACTICES.md](TESTING_BEST_PRACTICES.md) for details on:

- Test structure and organization
- Clean code principles in testing
- SOLID principles applied to tests
- Helper utilities and builders
- Writing maintainable tests

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Write tests for new functionality
4. Ensure all tests pass (`dotnet test`)
5. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
6. Push to the branch (`git push origin feature/AmazingFeature`)
7. Open a Pull Request

## License

This project is licensed under the MIT License.

## Support

For issues and questions:
- Create an issue on GitHub
- Check existing documentation
- Review Troubleshooting section above

---

**Last Updated**: December 2025
**Version**: 1.0.0

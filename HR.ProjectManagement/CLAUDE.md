# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Essential Commands

### Building and Running
```bash
# Build the project
dotnet build

# Run the application (development)
dotnet run
# URLs: http://localhost:5022 or https://localhost:7106

# Run with hot reload
dotnet watch run
```

### Database Operations (PostgreSQL + EF Core)
```bash
# Add a migration
dotnet ef migrations add <MigrationName>

# Apply migrations to database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove

# Generate SQL script
dotnet ef migrations script
```

### Package Management
```bash
# Restore dependencies
dotnet restore

# Add package
dotnet add package <PackageName>
```

## Architecture Overview

This is an ASP.NET Core 10.0 Web API following **Clean Architecture** with **CQRS** pattern:

### Key Layers
- **Contracts/**: Interface definitions (Identity, Persistence, Messaging)
- **Entities/**: Domain entities (User, Team, TaskItem, TeamMember)
- **DataContext/**: EF Core DbContext with PostgreSQL
- **Repositories/**: Generic repository pattern with Unit of Work
- **Services/**: Business logic and external service integrations
- **Features/**: CQRS command/query handlers organized by domain
- **Controllers/**: API endpoints
- **DTOs/**: Data Transfer Objects
- **Validations/**: FluentValidation rules

### Critical Patterns
- **Repository Pattern**: `IGenericRepository<T>` for data access
- **Unit of Work**: `IUnitOfWork` for transaction management
- **CQRS with MediatR**: Commands write data, Queries read data
- **Mapping**: AutoMapper (`MappingProfile/`) and Mapster for object mapping

## Configuration

### Database
- PostgreSQL required
- Connection string in `appsettings.json`
- Default database: `ProjectManagement`

### Authentication
- JWT Bearer authentication
- Roles: Admin, Manager, Employee
- Demo users pre-seeded:
  - admin@demo.com / Admin123!
  - manager@demo.com / Manager123!
  - employee@demo.com / Employee123!

### Service Registration
All services registered in `Extentions/RegisterDependencyInjection.cs` via `AddApplicationServices()`

## Development Notes

### Entity Relationships
- Users ↔ Teams: Many-to-many via TeamMember
- Users ↔ Tasks: Many-to-many for task assignments
- Teams → Tasks: One-to-many relationship

### Validation
All inputs validated using FluentValidation in `Validations/` directory

### API Documentation
Swagger UI available at `/swagger` in development

### When Adding New Features
1. Create entity in `Entities/` if needed
2. Add contracts in `Contracts/Persistence/` and `Contracts/Messaging/`
3. Implement repository methods if custom queries required
4. Create CQRS commands/queries in `Features/[FeatureName]/`
5. Add DTOs in `DTOs/[FeatureName]/`
6. Add validation rules
7. Map entities to DTOs in `MappingProfile/`
8. Create controller with endpoints
9. Register new services in `RegisterDependencyInjection.cs`
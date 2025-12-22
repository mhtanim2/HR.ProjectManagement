using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.Utils;

public static class SD
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
    public const string AdminOnly = "AdminOnly";
    public const string ManagerOrAdmin = "ManagerOrAdmin";
    public const string ApplicationDatabaseConnectionString= "ApplicationDatabaseConnectionString";
    // Seeded Admin User Details
    public const string AdminEmail = "admin@demo.com";
    public const string AdminFullname = "Admin User";
    public const string AdminPasswordHash = "$2a$11$CV0S5t1HPeUtjyn0cSoVtu1cIEAwLZxxEX4k4RqQFXYTLiJtQoC32";
    // Seeded Manager User Details
    public const string ManagerEmail = "manager@demo.com";
    public const string ManagerFullname = "Manager User";
    public const string ManagerPasswordHash = "$2a$11$2NYjgnLhXimpVceTL02.UOZFFOepwu20LlUEcIQTwmvL7msgHxSW2";
    // Seeded Employee User Details
    public const string EmployeeEmail = "Employee@demo.com";
    public const string EmployeeFullname = "Employee User";
    public const string EmployeePasswordHash = "$2a$11$KbaFgoihjNW8m64tM40SUutX.GjVL9ME6CbSDYDE9NfZP.uxtTRBq";

}
/*
 Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Admin123!"));
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Manager123!"));
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Employee123!"));

.NET Rest API Project: Task & Team Management System 
Objective: Build a mini Task & Team Management System with user roles and task workflows. 
The system will allow managers to assign tasks to users and track status. 
Core Requirements 
1. Entities 
User: Id, FullName, Email, Role (Admin / Manager/Employee) 
Team: Id, Name, Description 
Task: Id, Title, Description, Status (Todo / InProgress / Done), AssignedToUserId, CreatedByUserId, TeamId, DueDate 

2. API Features 
- CRUD operations for Users, Teams, and Tasks 
- Search and filter tasks by Status, AssignedTo, Team, and DueDate 
- Support pagination and sorting 
- Admins can manage users and teams (create, update, delete) 
- Managers can create tasks and update any task details 
- Employees can view and update status of their assigned tasks 
3. Authentication & Authorization 
- Implement JWT-based authentication 
- Seed default users: o Admin: admin@demo.com/ Admin123! o Manager: manager@demo.com/Manager123! o Employee: employee@demo.com/Employee123! 
- Enforce role-based access control 
4. Technology Stack (must use) 
- .NET Core Web API (latest LTS) 
- Entity Framework Core 
- Relational Database (any) 
- Swagger/OpenAPI 
- Centralized Logging in file system (any logging library) 
Unit testing (any framework) 
- FluentValidation for request validation 
- Implement CQRS (Command Query Responsibility Segregation) 
- Global exception handling via middleware 
 */
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
 */
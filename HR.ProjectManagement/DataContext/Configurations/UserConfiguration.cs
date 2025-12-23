using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;
using HR.ProjectManagement.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.ProjectManagement.DataContext.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasData(SeedUser());
    }
    
    private List<User> SeedUser()
    {
        return new List<User>
        {
            new User
            {
                Id = 1,
                FullName = SD.AdminFullname,
                Email = SD.AdminEmail,
                Role = Role.Admin,
                PasswordHash = SD.AdminPasswordHash
            },
            new User
            {
                Id = 2,
                FullName = SD.ManagerFullname,
                Email = SD.ManagerEmail,
                Role = Role.Manager,
                PasswordHash = SD.ManagerPasswordHash
            },
            new User
            {
                Id = 3,
                FullName = SD.EmployeeFullname,
                Email = SD.EmployeeEmail,
                Role = Role.Employee,
                PasswordHash = SD.EmployeePasswordHash
            }
        };
    }
}

using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;
using HR.ProjectManagement.Persistence.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.ProjectManagement.Persistence.IntegrationTests;

public class UserEntityTests : TestDataBase
{
    [Fact]
    public async Task Can_Add_User_To_Database()
    {
        // Arrange
        var user = EntityTestDataBuilder.BuildUser();

        // Act
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

        // Assert
        var savedUser = await Context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.NotNull(savedUser);
        Assert.Equal("Test User", savedUser.FullName);
        Assert.Equal(Role.Employee, savedUser.Role);
        TestAssertions.AssertAuditFields(savedUser);
    }

    [Fact]
    public async Task Can_Update_User_In_Database()
    {
        // Arrange
        var user = EntityTestDataBuilder.BuildUser();
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

        // Act
        user.FullName = "Updated User";
        await Context.SaveChangesAsync();

        // Assert
        var updatedUser = await Context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.NotNull(updatedUser);
        Assert.Equal("Updated User", updatedUser.FullName);
        Assert.True(updatedUser.LastModifiedDate > updatedUser.CreatedDate);
    }

    [Fact]
    public async Task Can_Delete_User_From_Database()
    {
        // Arrange
        var user = EntityTestDataBuilder.BuildUser();
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

        // Act
        Context.Users.Remove(user);
        await Context.SaveChangesAsync();

        // Assert
        var deletedUser = await Context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task SaveChangesAsync_Sets_Audit_Fields_For_New_Entity()
    {
        // Arrange
        var user = EntityTestDataBuilder.BuildUser();

        // Act
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

        // Assert
        TestAssertions.AssertAuditFields(user);
    }

    [Fact]
    public async Task SaveChangesAsync_Updates_Audit_Fields_For_Modified_Entity()
    {
        // Arrange
        var user = EntityTestDataBuilder.BuildUser();
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

        var originalModifiedDate = user.LastModifiedDate;

        // Wait a bit to ensure time difference
        await Task.Delay(100);

        // Act
        user.FullName = "Updated User";
        await Context.SaveChangesAsync();

        // Assert
        TestAssertions.AssertAuditFieldsUpdated(user, originalModifiedDate);
    }
}

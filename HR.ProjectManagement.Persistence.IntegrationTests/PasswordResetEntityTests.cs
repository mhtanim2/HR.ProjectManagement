using HR.ProjectManagement.Persistence.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.ProjectManagement.Persistence.IntegrationTests;

public class PasswordResetEntityTests : TestDataBase
{
    [Fact]
    public async Task Can_Add_PasswordReset_To_Database()
    {
        // Arrange
        var passwordReset = EntityTestDataBuilder.BuildPasswordReset();

        // Act
        await Context.PasswordResets.AddAsync(passwordReset);
        await Context.SaveChangesAsync();

        // Assert
        var savedReset = await Context.PasswordResets
            .FirstOrDefaultAsync(pr => pr.Token == "reset_token_123");

        Assert.NotNull(savedReset);
        Assert.Equal("reset_token_123", savedReset.Token);
        Assert.Equal("test@example.com", savedReset.Email);
        Assert.False(savedReset.IsUsed);
    }
}

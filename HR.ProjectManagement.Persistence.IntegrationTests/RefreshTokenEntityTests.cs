using HR.ProjectManagement.Persistence.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.ProjectManagement.Persistence.IntegrationTests;

public class RefreshTokenEntityTests : TestDataBase
{
    [Fact]
    public async Task Can_Add_RefreshToken_To_Database()
    {
        // Arrange
        var user = EntityTestDataBuilder.BuildUser();
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

        var refreshToken = EntityTestDataBuilder.BuildRefreshToken(user.Id);

        // Act
        await Context.RefreshTokens.AddAsync(refreshToken);
        await Context.SaveChangesAsync();

        // Assert
        var savedToken = await Context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == "sample_token_123");

        Assert.NotNull(savedToken);
        Assert.Equal("sample_token_123", savedToken.Token);
        Assert.NotNull(savedToken.User);
        Assert.Equal("Test User", savedToken.User.FullName);
    }

    [Fact]
    public async Task User_Can_Have_Multiple_RefreshTokens()
    {
        // Arrange
        var user = EntityTestDataBuilder.BuildUser();
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

        var token1 = EntityTestDataBuilder.BuildRefreshToken(user.Id, token: "token_1");
        var token2 = EntityTestDataBuilder.BuildRefreshToken(user.Id, token: "token_2");

        // Act
        await Context.RefreshTokens.AddAsync(token1);
        await Context.RefreshTokens.AddAsync(token2);
        await Context.SaveChangesAsync();

        // Assert
        var userTokens = await Context.RefreshTokens
            .Where(rt => rt.UserId == user.Id)
            .ToListAsync();

        Assert.Equal(2, userTokens.Count);
    }

    [Fact]
    public async Task Deleting_User_Deletes_Assigned_RefreshTokens()
    {
        // Arrange
        var user = EntityTestDataBuilder.BuildUser();
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();

        var refreshToken = EntityTestDataBuilder.BuildRefreshToken(user.Id, token: "token_to_delete");
        await Context.RefreshTokens.AddAsync(refreshToken);
        await Context.SaveChangesAsync();

        // Act
        Context.Users.Remove(user);
        await Context.SaveChangesAsync();

        // Assert
        var deletedToken = await Context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == "token_to_delete");

        Assert.Null(deletedToken);
    }
}

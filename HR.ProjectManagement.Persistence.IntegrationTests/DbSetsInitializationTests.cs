using Xunit;

namespace HR.ProjectManagement.Persistence.IntegrationTests;

public class DbSetsInitializationTests : TestDataBase
{
    [Fact]
    public void DbSets_AreInitialized()
    {
        // Assert
        Assert.NotNull(Context.Users);
        Assert.NotNull(Context.Teams);
        Assert.NotNull(Context.TaskItems);
        Assert.NotNull(Context.TeamMembers);
        Assert.NotNull(Context.RefreshTokens);
        Assert.NotNull(Context.PasswordResets);
    }
}

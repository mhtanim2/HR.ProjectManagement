using HR.ProjectManagement.Persistence.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.ProjectManagement.Persistence.IntegrationTests;

public class TeamEntityTests : TestDataBase
{
    [Fact]
    public async Task Can_Add_Team_To_Database()
    {
        // Arrange
        var team = EntityTestDataBuilder.BuildTeam();

        // Act
        await Context.Teams.AddAsync(team);
        await Context.SaveChangesAsync();

        // Assert
        var savedTeam = await Context.Teams.FirstOrDefaultAsync(t => t.Name == "Development Team");
        Assert.NotNull(savedTeam);
        Assert.Equal("Development Team", savedTeam.Name);
        Assert.Equal("Software development team", savedTeam.Description);
        Assert.NotNull(savedTeam.CreatedDate);
    }
}

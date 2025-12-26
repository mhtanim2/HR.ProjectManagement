using HR.ProjectManagement.DataContext;
using HR.ProjectManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HR.ProjectManagement.Persistence.IntegrationTests.Helpers;

public abstract class TestDataBase : IDisposable
{
    protected readonly ApplicationDBContext Context;
    protected readonly Mock<ICurrentUserService> MockCurrentUser;

    protected TestDataBase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        MockCurrentUser = new Mock<ICurrentUserService>();
        MockCurrentUser.Setup(x => x.UserId).Returns(1);

        Context = new ApplicationDBContext(options, MockCurrentUser.Object);
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}

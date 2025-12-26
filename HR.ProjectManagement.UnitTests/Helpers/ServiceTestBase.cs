using FluentValidation;
using Moq;

namespace HR.ProjectManagement.UnitTests.Helpers;

public abstract class ServiceTestBase
{
    protected readonly Mock<IUnitOfWork> MockUnitOfWork;

    protected ServiceTestBase()
    {
        MockUnitOfWork = new Mock<IUnitOfWork>();
        MockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
    }
}

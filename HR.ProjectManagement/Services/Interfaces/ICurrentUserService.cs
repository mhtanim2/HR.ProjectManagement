namespace HR.ProjectManagement.Services.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    bool IsAuthenticated { get; }
}

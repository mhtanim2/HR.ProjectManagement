using HR.ProjectManagement.Entities;

namespace HR.ProjectManagement.Contracts.Persistence;

public interface ITeamRepository : IGenericRepository<Team>
{
    Task<bool> IsAnyTeamAvailableByNameAsync(string name);
    Task<Team?> GetByNameAsync(string name);
    Task<Team?> GetWithMembersAsync(int id);
    Task<Team?> GetWithTasksAsync(int id);
    Task<Team?> GetWithMembersAndTasksAsync(int id);
}

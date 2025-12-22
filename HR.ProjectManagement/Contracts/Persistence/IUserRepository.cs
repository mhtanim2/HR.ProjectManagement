using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.Contracts.Persistence;

public interface IUserRepository:IGenericRepository<User>
{
    Task<bool> IsAnyUserAvailableByEmail(string email);
    Task<bool> IsAnyUserAvailableByEmailExcept(int id, string email);
    Task<User?> GetByEmailAsync(string email);
    Task<IReadOnlyList<User>> GetByRoleAsync(Role role);
    Task<User?> GetWithTeamsAsync(int id);
    Task<User?> GetWithTasksAsync(int id);
}

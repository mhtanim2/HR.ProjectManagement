using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.Contracts.Persistence;

public interface ITaskListRepository : IGenericRepository<TaskItem>
{
    Task<IReadOnlyList<TaskItem>> GetByAssignedUserAsync(int userId);
    Task<IReadOnlyList<TaskItem>> GetByCreatedByUserAsync(int userId);
    Task<IReadOnlyList<TaskItem>> GetByTeamAsync(int teamId);
    Task<IReadOnlyList<TaskItem>> GetByStatusAsync(Status status);
    Task<TaskItem?> GetWithDetailsAsync(int id);
    Task<IReadOnlyList<TaskItem>> GetTasksForUserAsync(int userId, Status? status = null);
    Task<IReadOnlyList<TaskItem>> GetTasksByMultipleFiltersAsync(int? userId = null, int? teamId = null, Status? status = null);
}

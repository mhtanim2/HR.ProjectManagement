using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.Services.Interfaces;

public interface ITaskItemService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request);
    Task<TaskResponse> UpdateAsync(int id, UpdateTaskRequest request);
    Task<TaskResponse> UpdateStatusAsync(int id, UpdateTaskStatusRequest request);
    Task<bool> DeleteAsync(int id);
    Task<TaskResponse?> GetByIdAsync(int id);
    Task<TaskWithDetailsResponse?> GetWithDetailsAsync(int id);
    Task<IReadOnlyList<TaskResponse>> GetAllAsync();
    Task<IReadOnlyList<TaskResponse>> GetMyTasksAsync(int userId);
    Task<IReadOnlyList<TaskResponse>> GetTasksByUserAsync(int userId);
    Task<IReadOnlyList<TaskResponse>> GetTasksByTeamAsync(int teamId);
    Task<IReadOnlyList<TaskResponse>> GetTasksByStatusAsync(Status status);
}
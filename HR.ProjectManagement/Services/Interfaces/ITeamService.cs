using HR.ProjectManagement.DTOs;

namespace HR.ProjectManagement.Services.Interfaces;

public interface ITeamService
{
    Task<TeamResponse> CreateAsync(CreateTeamRequest request);
    Task<TeamResponse> UpdateAsync(int id, UpdateTeamRequest request);
    Task<bool> DeleteAsync(int id);
    Task<TeamResponse?> GetByIdAsync(int id);
    Task<IReadOnlyList<TeamResponse>> GetAllAsync();
    Task<TeamWithMembersResponse?> GetWithMembersAsync(int id);
    Task<TeamWithTasksResponse?> GetWithTasksAsync(int id);
}
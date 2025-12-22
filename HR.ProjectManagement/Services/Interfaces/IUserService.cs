using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.Services.Interfaces;

public interface IUserService
{
    Task<UserResponse> CreateAsync(CreateUserRequest request);
    Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteAsync(int id);
    Task<UserResponse?> GetByIdAsync(int id);
    Task<IReadOnlyList<UserResponse>> GetAllAsync();
    Task<IReadOnlyList<UserResponse>> GetByRoleAsync(Role role);
    Task<UserResponse?> GetByEmailAsync(string email);
}

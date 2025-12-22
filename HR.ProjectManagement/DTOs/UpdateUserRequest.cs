using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.DTOs;

public class UpdateUserRequest
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Role Role { get; set; }
    public string? Password { get; set; }
}
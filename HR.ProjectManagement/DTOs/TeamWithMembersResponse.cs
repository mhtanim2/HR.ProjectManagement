using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.DTOs;

public class TeamWithMembersResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<UserSummary> Members { get; set; } = new();
}

public class UserSummary
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Role Role { get; set; }
}
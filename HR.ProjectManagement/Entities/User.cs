using HR.ProjectManagement.Entities.Common;
using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Role Role { get; set; }

    public string PasswordHash { get; set; } = null!;

    // Navigation
    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
}

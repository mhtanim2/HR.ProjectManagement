using HR.ProjectManagement.Entities.Common;

namespace HR.ProjectManagement.Entities;

public class Team : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Navigation
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}

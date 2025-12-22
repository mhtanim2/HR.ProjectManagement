using HR.ProjectManagement.Entities.Common;

namespace HR.ProjectManagement.Entities;

public class TeamMember : BaseEntity
{
    public int UserId { get; set; }
    public int TeamId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Team Team { get; set; } = null!;
}

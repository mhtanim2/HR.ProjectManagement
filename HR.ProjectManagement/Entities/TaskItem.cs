using HR.ProjectManagement.Entities.Common;
using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public Status Status { get; set; }
    public DateTime DueDate { get; set; }

    // Foreign Keys
    public int AssignedToUserId { get; set; }
    public int CreatedByUserId { get; set; }
    public int TeamId { get; set; }

    // Navigation
    public User AssignedToUser { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public Team Team { get; set; } = null!;
}

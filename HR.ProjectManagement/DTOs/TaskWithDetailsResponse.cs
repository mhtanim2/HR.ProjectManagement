using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.DTOs;

public class TaskWithDetailsResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public Status Status { get; set; }
    public DateTime DueDate { get; set; }

    public UserSummary AssignedToUser { get; set; } = null!;
    public UserSummary CreatedByUser { get; set; } = null!;
    public TeamSummary Team { get; set; } = null!;
}

public class TeamSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
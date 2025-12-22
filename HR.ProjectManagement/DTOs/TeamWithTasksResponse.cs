using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.DTOs;

public class TeamWithTasksResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<TaskSummary> Tasks { get; set; } = new();
}

public class TaskSummary
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public Status Status { get; set; }
    public DateTime DueDate { get; set; }
    public string AssignedToUserName { get; set; } = null!;
}
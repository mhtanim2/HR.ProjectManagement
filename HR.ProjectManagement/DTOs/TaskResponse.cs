using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.DTOs;

public class TaskResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public Status Status { get; set; }
    public DateTime DueDate { get; set; }

    public int AssignedToUserId { get; set; }
    public int TeamId { get; set; }
}

namespace HR.ProjectManagement.DTOs;

public class CreateTaskRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int AssignedToUserId { get; set; }
    public int TeamId { get; set; }
    public DateTime DueDate { get; set; }
    public int CreatedByUserId { get; set; } // This should be set from current user context
}

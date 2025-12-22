namespace HR.ProjectManagement.DTOs;

public class UpdateTeamRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
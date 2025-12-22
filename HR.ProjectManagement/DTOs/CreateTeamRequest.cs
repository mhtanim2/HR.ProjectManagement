namespace HR.ProjectManagement.DTOs;

public class CreateTeamRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

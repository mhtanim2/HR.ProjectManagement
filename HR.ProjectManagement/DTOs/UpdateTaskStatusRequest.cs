using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.DTOs;

public class UpdateTaskStatusRequest
{
    public Status Status { get; set; }
}

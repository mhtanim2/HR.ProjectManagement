using HR.ProjectManagement.Entities.Common;

namespace HR.ProjectManagement.Entities;

public class PasswordReset : BaseEntity
{
    public string Token { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime Expires { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
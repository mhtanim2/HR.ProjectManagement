using HR.ProjectManagement.Entities.Common;

namespace HR.ProjectManagement.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = null!;
    public DateTime Expires { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
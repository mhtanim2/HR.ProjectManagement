namespace HR.ProjectManagement.DTOs;

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = null!;
    public UserResponse User { get; set; } = null!;
}
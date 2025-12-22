namespace HR.ProjectManagement.DTOs;

public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = null!;
}
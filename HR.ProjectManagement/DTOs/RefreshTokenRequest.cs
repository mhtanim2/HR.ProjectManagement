using System.ComponentModel.DataAnnotations;

namespace HR.ProjectManagement.DTOs;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = null!;

    public string AccessToken { get; set; } = null!;
}
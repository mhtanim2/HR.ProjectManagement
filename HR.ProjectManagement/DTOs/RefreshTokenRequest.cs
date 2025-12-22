using System.ComponentModel.DataAnnotations;

namespace HR.ProjectManagement.DTOs;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = null!;

    [Required]
    public string AccessToken { get; set; } = null!;
}
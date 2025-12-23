using System.ComponentModel.DataAnnotations;

namespace HR.ProjectManagement.DTOs;

public class ResetPasswordRequest
{
    public string Token { get; set; } = null!;

    public string NewPassword { get; set; } = null!;

    public string ConfirmPassword { get; set; } = null!;
}
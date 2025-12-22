using System.ComponentModel.DataAnnotations;

namespace HR.ProjectManagement.DTOs;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
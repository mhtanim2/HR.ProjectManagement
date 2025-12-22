namespace HR.ProjectManagement.DTOs;

public class ForgotPasswordResponse
{
    public string Message { get; set; } = null!;
    public bool Success { get; set; }
    public string? ResetToken { get; set; } // Only for development/testing
}
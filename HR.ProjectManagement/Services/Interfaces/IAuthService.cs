using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities;

namespace HR.ProjectManagement.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<string> GenerateTokenAsync(User user);
    Task<User> ValidateUserCredentialsAsync(string email, string password);
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task LogoutAsync(string refreshToken);
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);
}
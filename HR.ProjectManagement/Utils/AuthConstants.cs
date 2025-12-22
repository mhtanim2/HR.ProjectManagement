namespace HR.ProjectManagement.Utils;

public static class AuthConstants
{
    public const string InvalidCredentialsMessage = "Invalid email or password";
    public const string UserNotFoundMessage = "User not found";
    public const string TokenGenerationFailedMessage = "Failed to generate authentication token";
    public const string InvalidRefreshTokenMessage = "Invalid or expired refresh token";
    public const string RefreshTokenRevokedMessage = "Refresh token has been revoked";
    public const string PasswordResetTokenSent = "Password reset token has been sent to your email";
    public const string InvalidPasswordResetToken = "Invalid or expired password reset token";
    public const string PasswordResetSuccess = "Password has been reset successfully";
    public const string EmailClaimType = "email";
    public const string NameIdentifierClaimType = "nameidentifier";
    public const string RoleClaimType = "role";
}
using HR.ProjectManagement.Entities;

namespace HR.ProjectManagement.Contracts.Persistence;

public interface IPasswordResetRepository : IGenericRepository<PasswordReset>
{
    Task<PasswordReset?> GetByTokenAsync(string token);
    Task<PasswordReset?> GetValidTokenByEmailAsync(string email);
    Task<bool> IsValidTokenAsync(string token);
    Task InvalidateAllTokensForEmailAsync(string email);
}
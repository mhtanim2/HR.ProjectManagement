using HR.ProjectManagement.Entities;

namespace HR.ProjectManagement.Contracts.Persistence;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(int userId);
    Task RevokeTokensByUserIdAsync(int userId);
    Task<bool> IsValidTokenAsync(string token);
}
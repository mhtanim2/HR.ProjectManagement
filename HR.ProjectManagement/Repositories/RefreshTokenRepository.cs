using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DataContext;
using HR.ProjectManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.ProjectManagement.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDBContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(int userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync();
    }

    public async Task RevokeTokensByUserIdAsync(int userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsValidTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        return refreshToken != null &&
               !refreshToken.IsUsed &&
               !refreshToken.IsRevoked &&
               refreshToken.Expires > DateTime.UtcNow;
    }
}
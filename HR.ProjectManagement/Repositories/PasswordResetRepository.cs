using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DataContext;
using HR.ProjectManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.ProjectManagement.Repositories;

public class PasswordResetRepository : GenericRepository<PasswordReset>, IPasswordResetRepository
{
    public PasswordResetRepository(ApplicationDBContext context) : base(context)
    {
    }

    public async Task<PasswordReset?> GetByTokenAsync(string token)
    {
        return await _context.PasswordResets
            .FirstOrDefaultAsync(pr => pr.Token == token);
    }

    public async Task<PasswordReset?> GetValidTokenByEmailAsync(string email)
    {
        return await _context.PasswordResets
            .Where(pr => pr.Email == email && !pr.IsUsed && pr.Expires > DateTime.UtcNow)
            .OrderByDescending(pr => pr.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsValidTokenAsync(string token)
    {
        var passwordReset = await _context.PasswordResets
            .FirstOrDefaultAsync(pr => pr.Token == token);

        return passwordReset != null &&
               !passwordReset.IsUsed &&
               passwordReset.Expires > DateTime.UtcNow;
    }

    public async Task InvalidateAllTokensForEmailAsync(string email)
    {
        var activeTokens = await _context.PasswordResets
            .Where(pr => pr.Email == email && !pr.IsUsed)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsUsed = true;
        }

        await _context.SaveChangesAsync();
    }
}
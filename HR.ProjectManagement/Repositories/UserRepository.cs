using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DataContext;
using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace HR.ProjectManagement.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDBContext context):base(context)
    {
    }

    public async Task<bool> IsAnyUserAvailableByEmail(string email)
    {
         return await _context.Users.AnyAsync(u=>u.Email == email);
    }

    public async Task<bool> IsAnyUserAvailableByEmailExcept(int id, string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email && u.Id != id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IReadOnlyList<User>> GetByRoleAsync(Role role)
    {
        return await _context.Users
            .Where(u => u.Role == role)
            .ToListAsync();
    }

    public async Task<User?> GetWithTeamsAsync(int id)
    {
        return await _context.Users
            .Include(u => u.TeamMemberships)
            .ThenInclude(tm => tm.Team)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetWithTasksAsync(int id)
    {
        return await _context.Users
            .Include(u => u.AssignedTasks)
            .Include(u => u.CreatedTasks)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}

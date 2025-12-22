using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DataContext;
using HR.ProjectManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.ProjectManagement.Repositories;

public class TeamRepository : GenericRepository<Team>, ITeamRepository
{
    public TeamRepository(ApplicationDBContext context) : base(context)
    {
    }

    public async Task<bool> IsAnyTeamAvailableByNameAsync(string name)
    {
        return await _context.Teams.AnyAsync(t => t.Name == name);
    }

    public async Task<Team?> GetByNameAsync(string name)
    {
        return await _context.Teams
            .FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<Team?> GetWithMembersAsync(int id)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .ThenInclude(tm => tm.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Team?> GetWithTasksAsync(int id)
    {
        return await _context.Teams
            .Include(t => t.Tasks)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Team?> GetWithMembersAndTasksAsync(int id)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .ThenInclude(tm => tm.User)
            .Include(t => t.Tasks)
            .ThenInclude(task => task.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
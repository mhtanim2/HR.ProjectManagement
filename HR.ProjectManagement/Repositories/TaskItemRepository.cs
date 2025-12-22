using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DataContext;
using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace HR.ProjectManagement.Repositories;

public class TaskItemRepository : GenericRepository<TaskItem>, ITaskListRepository
{
    public TaskItemRepository(ApplicationDBContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TaskItem>> GetByAssignedUserAsync(int userId)
    {
        return await _context.TaskItems
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Team)
            .Where(t => t.AssignedToUserId == userId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TaskItem>> GetByCreatedByUserAsync(int userId)
    {
        return await _context.TaskItems
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Team)
            .Where(t => t.CreatedByUserId == userId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TaskItem>> GetByTeamAsync(int teamId)
    {
        return await _context.TaskItems
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Team)
            .Where(t => t.TeamId == teamId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TaskItem>> GetByStatusAsync(Status status)
    {
        return await _context.TaskItems
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Team)
            .Where(t => t.Status == status)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetWithDetailsAsync(int id)
    {
        return await _context.TaskItems
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Team)
            .ThenInclude(team => team.Members)
            .ThenInclude(tm => tm.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksForUserAsync(int userId, Status? status = null)
    {
        var query = _context.TaskItems
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Team)
            .Where(t => t.AssignedToUserId == userId);

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksByMultipleFiltersAsync(
        int? userId = null,
        int? teamId = null,
        Status? status = null)
    {
        var query = _context.TaskItems
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Team)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == userId.Value);
        }

        if (teamId.HasValue)
        {
            query = query.Where(t => t.TeamId == teamId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        return await query.ToListAsync();
    }
}
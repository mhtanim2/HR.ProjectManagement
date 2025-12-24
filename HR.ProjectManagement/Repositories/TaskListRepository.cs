using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DataContext;
using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;
using HR.ProjectManagement.QueryExtensions;
using Microsoft.EntityFrameworkCore;

namespace HR.ProjectManagement.Repositories;

public class TaskListRepository : GenericRepository<TaskItem>, ITaskListRepository
{
    public TaskListRepository(ApplicationDBContext context) : base(context)
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

    public async Task<PagedResult<TaskItem>> SearchTasksAsync(TaskSearchRequest request)
    {
        IQueryable<TaskItem> query = _context.TaskItems
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Team)
            .AsNoTracking();

        query = query
            .ApplySearch(request.SearchTerm)
            .ApplyFilters(request)
            .ApplySorting(request);

       
        var totalCount = await query.CountAsync();

        //query = ApplySorting(query, request.SortBy, request.SortDescending);

        // Apply pagination
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .AsNoTracking() // Read-only query optimization
            .ToListAsync();

        return new PagedResult<TaskItem>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    private IQueryable<TaskItem> ApplySorting(IQueryable<TaskItem> query, string? sortBy, bool descending)
    {
        var sortField = sortBy?.ToLower() switch
        {
            "title" => "Title",
            "status" => "Status",
            "duedate" => "DueDate",
            "createddate" => "CreatedDate",
            "assignedto" => "AssignedToUserId",
            "team" => "TeamId",
            _ => "DueDate" // Default sort
        };

        return (sortField, descending) switch
        {
            ("Title", true) => query.OrderByDescending(t => t.Title),
            ("Title", false) => query.OrderBy(t => t.Title),

            ("Status", true) => query.OrderByDescending(t => t.Status),
            ("Status", false) => query.OrderBy(t => t.Status),

            ("DueDate", true) => query.OrderByDescending(t => t.DueDate),
            ("DueDate", false) => query.OrderBy(t => t.DueDate),

            ("CreatedDate", true) => query.OrderByDescending(t => t.CreatedDate),
            ("CreatedDate", false) => query.OrderBy(t => t.CreatedDate),

            ("AssignedToUserId", true) => query.OrderByDescending(t => t.AssignedToUser.FullName),
            ("AssignedToUserId", false) => query.OrderBy(t => t.AssignedToUser.FullName),

            ("TeamId", true) => query.OrderByDescending(t => t.Team.Name),
            ("TeamId", false) => query.OrderBy(t => t.Team.Name),

            _ => query.OrderBy(t => t.DueDate) // Default
        };
    }
}
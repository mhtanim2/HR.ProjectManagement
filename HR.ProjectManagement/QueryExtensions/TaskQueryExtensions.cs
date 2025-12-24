using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities;

namespace HR.ProjectManagement.QueryExtensions;

public static class TaskQueryExtensions
{
    public static IQueryable<TaskItem> ApplySearch(
        this IQueryable<TaskItem> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        search = search.ToLower();

        return query.Where(t =>
            t.Title.ToLower().Contains(search) ||
            (t.Description != null && t.Description.ToLower().Contains(search))
        );
    }

    public static IQueryable<TaskItem> ApplyFilters(
        this IQueryable<TaskItem> query, TaskSearchRequest parameters)
    {
        if (parameters.Status.HasValue)
            query = query.Where(t => t.Status == parameters.Status);

        if (parameters.AssignedToUserId.HasValue)
            query = query.Where(t => t.AssignedToUserId == parameters.AssignedToUserId);

        if (parameters.TeamId.HasValue)
            query = query.Where(t => t.TeamId == parameters.TeamId);

        if (parameters.DueDateFrom.HasValue)
            query = query.Where(t => t.DueDate >= parameters.DueDateFrom);

        if (parameters.DueDateTo.HasValue)
            query = query.Where(t => t.DueDate <= parameters.DueDateTo);

        return query;
    }

    public static IQueryable<TaskItem> ApplySorting(
        this IQueryable<TaskItem> query, TaskSearchRequest parameters)
    {
        return parameters?.SortBy?.ToLower() switch
        {
            "title" => parameters.SortDescending
                ? query.OrderByDescending(t => t.Title)
                : query.OrderBy(t => t.Title),

            "status" => parameters.SortDescending
                ? query.OrderByDescending(t => t.Status)
                : query.OrderBy(t => t.Status),

            _ => parameters.SortDescending
                ? query.OrderByDescending(t => t.DueDate)
                : query.OrderBy(t => t.DueDate)
        };
    }
}
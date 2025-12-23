using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.DTOs;

public class TaskSearchRequest
{
    // Filter Parameters
    public Status? Status { get; set; }
    public int? AssignedToUserId { get; set; }
    public int? TeamId { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string? SearchTerm { get; set; }

    // Pagination Parameters
    private int _pageNumber = 1;
    private int _pageSize = 10;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value < 1 || value > 100) ? 10 : value;
    }

    // Sorting Parameters
    public string? SortBy { get; set; } = "DueDate";
    public bool SortDescending { get; set; } = false;
}
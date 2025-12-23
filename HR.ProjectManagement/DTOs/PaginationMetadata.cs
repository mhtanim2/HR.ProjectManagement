namespace HR.ProjectManagement.DTOs;

public class PaginationMetadata
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static PaginationMetadata FromPagedResponse(int pageNumber, int pageSize, int totalCount)
    {
        return new PaginationMetadata
        {
            Page = pageNumber,
            Limit = pageSize,
            Total = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
}

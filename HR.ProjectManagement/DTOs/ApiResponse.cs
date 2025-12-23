namespace HR.ProjectManagement.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = "Success";
    public PaginationMetadata? Pagination { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");

    public static ApiResponse<T> OkResponse(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> OkResponse(T data, string message, PaginationMetadata pagination)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            Pagination = pagination
        };
    }

    public static ApiResponse<T> OkResponse(string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public new static ApiResponse OkResponse(object? data = null, string message = "Success", PaginationMetadata? pagination = null)
    {
        return new ApiResponse
        {
            Success = true,
            Data = data,
            Message = message,
            Pagination = pagination
        };
    }
}

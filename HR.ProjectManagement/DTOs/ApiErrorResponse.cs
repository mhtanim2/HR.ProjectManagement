namespace HR.ProjectManagement.DTOs;

public class ApiErrorResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
    public int StatusCode { get; set; }

    public static ApiErrorResponse ErrorResponse(string message, int statusCode, string? error = null, Dictionary<string, string[]>? errors = null)
    {
        return new ApiErrorResponse
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Error = error,
            Errors = errors
        };
    }
}

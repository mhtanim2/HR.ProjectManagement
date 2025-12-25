using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Exceptions;
using System.Net;

namespace HR.ProjectManagement.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
    {
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        string message = "An error occurred";
        string? error = null;
        Dictionary<string, string[]>? errors = null;

        switch (ex)
        {
            case BadRequestException badRequestException:
                statusCode = HttpStatusCode.BadRequest;
                message = badRequestException.Message;
                error = badRequestException.InnerException?.Message;
                errors = badRequestException.ValidationErrors != null
                    ? new Dictionary<string, string[]>(badRequestException.ValidationErrors)
                    : null;
                break;
            case HR.ProjectManagement.Exceptions.ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                message = validationException.Message;
                error = validationException.InnerException?.Message;
                errors = validationException.ValidationErrors != null
                    ? new Dictionary<string, string[]>(validationException.ValidationErrors)
                    : null;
                break;
            case NotFoundException NotFound:
                statusCode = HttpStatusCode.NotFound;
                message = NotFound.Message;
                error = NotFound.InnerException?.Message;
                break;
            case AuthenticationException authenticationException:
                statusCode = HttpStatusCode.Unauthorized;
                message = authenticationException.Message;
                error = authenticationException.InnerException?.Message;
                break;
            case UnauthorizedException unauthorizedException:
                statusCode = HttpStatusCode.Unauthorized;
                message = unauthorizedException.Message;
                error = unauthorizedException.InnerException?.Message;
                break;
            case TokenExpiredException tokenExpiredException:
                statusCode = HttpStatusCode.Unauthorized;
                message = tokenExpiredException.Message;
                error = tokenExpiredException.InnerException?.Message;
                break;
            case UnauthorizedAccessException unauthorizedAccessException:
                statusCode = HttpStatusCode.Forbidden;
                message = "Access denied";
                error = unauthorizedAccessException.Message;
                break;
            case SecurityException securityException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Invalid user Id claim";
                error = securityException.Message;
                break;
            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = ex.Message;
                error = ex.StackTrace;
                break;
        }

        httpContext.Response.StatusCode = (int)statusCode;

        var errorResponse = ApiErrorResponse.ErrorResponse(
            message,
            (int)statusCode,
            error,
            errors
        );

        /*        var logMessage = JsonSerializer.Serialize(errorResponse);
                _logger.LogError(ex, logMessage);
        */

        _logger.LogError(ex, $"Request failed with status {errorResponse.StatusCode}. Error: {errorResponse.Message}, Details: {errorResponse.Errors}");
        await httpContext.Response.WriteAsJsonAsync(errorResponse);
    }
}
using HR.ProjectManagement.Exceptions;
using HR.ProjectManagement.Utils;
using Newtonsoft.Json;
using System.Net;

namespace HR.ProjectManagement.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    //private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next/*, ILogger<ExceptionMiddleware> logger*/)
    {
        _next = next;
        //this._logger = logger;
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
        CustomProblemDetails problem = new();

        switch (ex)
        {
            case BadRequestException badRequestException:
                statusCode = HttpStatusCode.BadRequest;
                problem = new CustomProblemDetails
                {
                    Title = badRequestException.Message,
                    Status = (int)statusCode,
                    Detail = badRequestException.InnerException?.Message,
                    Type = nameof(BadRequestException),
                    Errors = badRequestException.ValidationErrors
                };
                break;
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                problem = new CustomProblemDetails
                {
                    Title = validationException.Message,
                    Status = (int)statusCode,
                    Detail = validationException.InnerException?.Message,
                    Type = nameof(ValidationException),
                    Errors = validationException.ValidationErrors
                };
                break;
            case NotFoundException NotFound:
                statusCode = HttpStatusCode.NotFound;
                problem = new CustomProblemDetails
                {
                    Title = NotFound.Message,
                    Status = (int)statusCode,
                    Type = nameof(NotFoundException),
                    Detail = NotFound.InnerException?.Message,
                };
                break;
            case AuthenticationException authenticationException:
                statusCode = HttpStatusCode.Unauthorized;
                problem = new CustomProblemDetails
                {
                    Title = authenticationException.Message,
                    Status = (int)statusCode,
                    Type = nameof(AuthenticationException),
                    Detail = authenticationException.InnerException?.Message,
                };
                break;
            case UnauthorizedException unauthorizedException:
                statusCode = HttpStatusCode.Unauthorized;
                problem = new CustomProblemDetails
                {
                    Title = unauthorizedException.Message,
                    Status = (int)statusCode,
                    Type = nameof(UnauthorizedException),
                    Detail = unauthorizedException.InnerException?.Message,
                };
                break;
            case TokenExpiredException tokenExpiredException:
                statusCode = HttpStatusCode.Unauthorized;
                problem = new CustomProblemDetails
                {
                    Title = tokenExpiredException.Message,
                    Status = (int)statusCode,
                    Type = nameof(TokenExpiredException),
                    Detail = tokenExpiredException.InnerException?.Message,
                };
                break;
            default:
                problem = new CustomProblemDetails
                {
                    Title = ex.Message,
                    Status = (int)statusCode,
                    Type = nameof(HttpStatusCode.InternalServerError),
                    Detail = ex.StackTrace,
                };
                break;
        }

        httpContext.Response.StatusCode = (int)statusCode;
        var logMessage = JsonConvert.SerializeObject(problem);
    //    _logger.LogError(logMessage);
        await httpContext.Response.WriteAsJsonAsync(problem);

    }
}
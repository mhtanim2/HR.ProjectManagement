using HR.ProjectManagement.Exceptions;
using HR.ProjectManagement.Services.Interfaces;
using HR.ProjectManagement.Utils;


namespace HR.ProjectManagement.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public int UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(AuthConstants.NameIdentifierClaimType);

            if (userIdClaim == null)
                throw new UnauthorizedException("User is not authenticated.");
            
            if (!int.TryParse(userIdClaim.Value, out var userId))
                throw new SecurityException("Invalid user id claim.");

            return userId;
        }
    }
}

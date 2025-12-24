using HR.ProjectManagement.Contracts.Identity;
using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DataContext;
using HR.ProjectManagement.Middleware;
using HR.ProjectManagement.Repositories;
using HR.ProjectManagement.Services;
using HR.ProjectManagement.Services.Interfaces;
using HR.ProjectManagement.Utils;
using HR.ProjectManagement.Validations.UserValidations;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using HR.ProjectManagement.Exceptions;

namespace HR.ProjectManagement.Extentions;

public static class RegisterDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDBContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString(SD.ApplicationDatabaseConnectionString)));

        services.AddMapster();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork,UnitOfWork>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddHttpContextAccessor();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = configuration["Jwt:Issuer"] ?? throw new AuthenticationException("jwt Issuer not configured"),
                    ValidAudience = configuration["Jwt:Audience"] ?? throw new AuthenticationException("JWT Audience not configured"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!?? throw new AuthenticationException("JWT Key not configured"))
                    ),

                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(SD.AdminOnly, p => p.RequireRole(SD.Admin));
            options.AddPolicy(SD.ManagerOrAdmin, p => p.RequireRole(SD.Manager, SD.Admin));
            options.AddPolicy(SD.ManagerOnly, p => p.RequireRole(SD.Manager));
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<ITeamService, TeamService>();

        services.AddScoped<ITaskListRepository, TaskListRepository>();
        services.AddScoped<ITaskItemService, TaskItemService>();

        services.AddValidatorsFromAssemblyContaining<LoginValidation>();

        services.AddScoped<ApiResponseFilter>();

        return services;
    }
}

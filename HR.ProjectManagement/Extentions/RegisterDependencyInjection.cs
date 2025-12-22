using HR.ProjectManagement.Contracts.Identity;
using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DataContext;
using HR.ProjectManagement.Repositories;
using HR.ProjectManagement.Services;
using HR.ProjectManagement.Services.Interfaces;
using HR.ProjectManagement.Utils;
using HR.ProjectManagement.Validations.UserValidations;
using HR.ProjectManagement.Validations.TeamValidations;
using HR.ProjectManagement.Validations.TaskValidations;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using FluentValidation;

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

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
                    ),

                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(SD.AdminOnly, p => p.RequireRole(SD.Admin));
            options.AddPolicy(SD.ManagerOrAdmin, p => p.RequireRole(SD.Manager, SD.Admin));
        });

        // Register User services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();

        // Register Team services
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<ITeamService, TeamService>();

        // Register Task services
        services.AddScoped<ITaskListRepository, TaskItemRepository>();
        services.AddScoped<ITaskItemService, TaskItemService>();

        // Register FluentValidation validators
        services.AddValidatorsFromAssemblyContaining<CreateUserValidation>();
        services.AddValidatorsFromAssemblyContaining<CreateTeamValidation>();
        services.AddValidatorsFromAssemblyContaining<CreateTaskValidation>();

        return services;
    }
}

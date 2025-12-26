using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;

namespace HR.ProjectManagement.Persistence.IntegrationTests.Helpers;

public static class EntityTestDataBuilder
{
    public static User BuildUser(
        string? fullName = null,
        string? email = null,
        Role? role = null,
        string? passwordHash = null)
    {
        return new User
        {
            FullName = fullName ?? "Test User",
            Email = email ?? "test@example.com",
            Role = role ?? Role.Employee,
            PasswordHash = passwordHash ?? "hashed_password"
        };
    }

    public static Team BuildTeam(
        string? name = null,
        string? description = null)
    {
        return new Team
        {
            Name = name ?? "Development Team",
            Description = description ?? "Software development team"
        };
    }

    public static TaskItem BuildTask(
        int? assignedToUserId = null,
        int? createdByUserId = null,
        int? teamId = null,
        string? title = null,
        string? description = null,
        Status? status = null,
        DateTime? dueDate = null)
    {
        return new TaskItem
        {
            Title = title ?? "Complete Task",
            Description = description ?? "Task description",
            Status = status ?? Status.Todo,
            DueDate = dueDate ?? DateTime.UtcNow.AddDays(7),
            AssignedToUserId = assignedToUserId ?? 1,
            CreatedByUserId = createdByUserId ?? 1,
            TeamId = teamId ?? 1
        };
    }

    public static TeamMember BuildTeamMember(int userId, int teamId)
    {
        return new TeamMember
        {
            UserId = userId,
            TeamId = teamId
        };
    }

    public static RefreshToken BuildRefreshToken(
        int userId,
        string? token = null,
        DateTime? expires = null,
        bool isUsed = false,
        bool isRevoked = false)
    {
        return new RefreshToken
        {
            Token = token ?? "sample_token_123",
            UserId = userId,
            Expires = expires ?? DateTime.UtcNow.AddDays(7),
            IsUsed = isUsed,
            IsRevoked = isRevoked
        };
    }

    public static PasswordReset BuildPasswordReset(
        string? email = null,
        string? token = null,
        DateTime? expires = null,
        bool isUsed = false)
    {
        return new PasswordReset
        {
            Token = token ?? "reset_token_123",
            Email = email ?? "test@example.com",
            Expires = expires ?? DateTime.UtcNow.AddHours(1),
            IsUsed = isUsed,
            CreatedAt = DateTime.UtcNow
        };
    }
}

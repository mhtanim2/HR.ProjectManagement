using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Common;
using HR.ProjectManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HR.ProjectManagement.DataContext;

public class ApplicationDBContext : DbContext
{
    private readonly ICurrentUserService _currentUser;

    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options, ICurrentUserService currentUser) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordReset> PasswordResets => Set<PasswordReset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDBContext).Assembly);

        base.OnModelCreating(modelBuilder);

        // User ↔ Task (Assigned)
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.AssignedToUser)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // User ↔ Task (CreatedBy)
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.CreatedByUser)
            .WithMany(u => u.CreatedTasks)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // User ↔ Team (Many-to-Many)
        modelBuilder.Entity<TeamMember>()
            .HasOne(tm => tm.User)
            .WithMany(u => u.TeamMemberships)
            .HasForeignKey(tm => tm.UserId);

        modelBuilder.Entity<TeamMember>()
            .HasOne(tm => tm.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(tm => tm.TeamId);

        modelBuilder.Entity<TeamMember>()
            .HasIndex(tm => new { tm.UserId, tm.TeamId })
            .IsUnique();

        // User ↔ RefreshToken (One-to-Many)
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        modelBuilder.Entity<PasswordReset>()
            .HasIndex(pr => pr.Token)
            .IsUnique();

        modelBuilder.Entity<PasswordReset>()
            .HasIndex(pr => new { pr.Email, pr.IsUsed });

        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.Status);

        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.DueDate);

        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.AssignedToUserId);

        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.TeamId);

        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => new { t.Status, t.DueDate });
    }


    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in base.ChangeTracker.Entries<BaseEntity>()
            .Where(q => q.State == EntityState.Added || q.State == EntityState.Modified))
        {
            entry.Entity.LastModifiedDate = DateTime.Now;
            entry.Entity.ModifiedBy = _currentUser.UserId;
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = DateTime.Now;
                entry.Entity.CreatedBy = _currentUser.UserId;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

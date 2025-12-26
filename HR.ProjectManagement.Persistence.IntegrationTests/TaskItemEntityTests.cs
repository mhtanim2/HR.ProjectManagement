using HR.ProjectManagement.Entities.Enums;
using HR.ProjectManagement.Persistence.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HR.ProjectManagement.Persistence.IntegrationTests;

public class TaskItemEntityTests : TestDataBase
{
    [Fact]
    public async Task Can_Add_TaskItem_To_Database()
    {
        // Arrange
        var user = EntityTestDataBuilder.BuildUser();
        var team = EntityTestDataBuilder.BuildTeam();
        await Context.Users.AddAsync(user);
        await Context.Teams.AddAsync(team);
        await Context.SaveChangesAsync();

        var task = EntityTestDataBuilder.BuildTask(
            assignedToUserId: user.Id,
            createdByUserId: user.Id,
            teamId: team.Id
        );

        // Act
        await Context.TaskItems.AddAsync(task);
        await Context.SaveChangesAsync();

        // Assert
        var savedTask = await Context.TaskItems.FirstOrDefaultAsync(t => t.Title == "Complete Task");
        Assert.NotNull(savedTask);
        Assert.Equal("Complete Task", savedTask.Title);
        Assert.Equal(Status.Todo, savedTask.Status);
        Assert.NotNull(savedTask.CreatedDate);
    }

    [Fact]
    public async Task TaskItem_Has_Relationship_With_User_And_Team()
    {
        // Arrange
        var user = EntityTestDataBuilder.BuildUser();
        var team = EntityTestDataBuilder.BuildTeam();
        await Context.Users.AddAsync(user);
        await Context.Teams.AddAsync(team);
        await Context.SaveChangesAsync();

        var task = EntityTestDataBuilder.BuildTask(
            assignedToUserId: user.Id,
            createdByUserId: user.Id,
            teamId: team.Id
        );
        await Context.TaskItems.AddAsync(task);
        await Context.SaveChangesAsync();

        // Act
        var taskWithRelations = await Context.TaskItems
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == task.Id);

        // Assert
        Assert.NotNull(taskWithRelations);
        Assert.NotNull(taskWithRelations.AssignedToUser);
        Assert.NotNull(taskWithRelations.CreatedByUser);
        Assert.NotNull(taskWithRelations.Team);
        Assert.Equal("Test User", taskWithRelations.AssignedToUser.FullName);
        Assert.Equal("Development Team", taskWithRelations.Team.Name);
    }

    [Fact]
    public async Task Can_Query_Tasks_With_User_And_Team_Includes()
    {
        // Arrange
        var user = EntityTestDataBuilder.BuildUser();
        var team = EntityTestDataBuilder.BuildTeam();
        await Context.Users.AddAsync(user);
        await Context.Teams.AddAsync(team);
        await Context.SaveChangesAsync();

        var task1 = EntityTestDataBuilder.BuildTask(
            assignedToUserId: user.Id,
            createdByUserId: user.Id,
            teamId: team.Id,
            title: "Task 1",
            description: "First task"
        );
        var task2 = EntityTestDataBuilder.BuildTask(
            assignedToUserId: user.Id,
            createdByUserId: user.Id,
            teamId: team.Id,
            title: "Task 2",
            description: "Second task",
            status: Status.InProgress,
            dueDate: DateTime.UtcNow.AddDays(14)
        );

        await Context.TaskItems.AddAsync(task1);
        await Context.TaskItems.AddAsync(task2);
        await Context.SaveChangesAsync();

        // Act
        var tasks = await Context.TaskItems
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Team)
            .Where(t => t.AssignedToUserId == user.Id)
            .ToListAsync();

        // Assert
        Assert.Equal(2, tasks.Count);
        Assert.All(tasks, t => Assert.NotNull(t.AssignedToUser));
        Assert.All(tasks, t => Assert.NotNull(t.Team));
    }

    [Fact]
    public async Task Can_Filter_Tasks_By_Status()
    {
        // Arrange
        var user = EntityTestDataBuilder.BuildUser();
        var team = EntityTestDataBuilder.BuildTeam();
        await Context.Users.AddAsync(user);
        await Context.Teams.AddAsync(team);
        await Context.SaveChangesAsync();

        var task1 = EntityTestDataBuilder.BuildTask(
            assignedToUserId: user.Id,
            createdByUserId: user.Id,
            teamId: team.Id,
            title: "Todo Task",
            status: Status.Todo
        );
        var task2 = EntityTestDataBuilder.BuildTask(
            assignedToUserId: user.Id,
            createdByUserId: user.Id,
            teamId: team.Id,
            title: "Done Task",
            status: Status.Done,
            dueDate: DateTime.UtcNow.AddDays(1)
        );

        await Context.TaskItems.AddAsync(task1);
        await Context.TaskItems.AddAsync(task2);
        await Context.SaveChangesAsync();

        // Act
        var todoTasks = await Context.TaskItems
            .Where(t => t.Status == Status.Todo)
            .ToListAsync();

        var doneTasks = await Context.TaskItems
            .Where(t => t.Status == Status.Done)
            .ToListAsync();

        // Assert
        Assert.Single(todoTasks);
        Assert.Single(doneTasks);
        Assert.Equal("Todo Task", todoTasks[0].Title);
        Assert.Equal("Done Task", doneTasks[0].Title);
    }
}

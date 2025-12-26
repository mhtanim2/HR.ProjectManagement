using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;
using HR.ProjectManagement.Repositories;
using Moq;

namespace HR.ProjectManagement.UnitTests.Mocks;

public static class MockTaskListRepository
{
    public static Mock<ITaskListRepository> GetMockTaskListRepository()
    {
        var mockRepository = new Mock<ITaskListRepository>();

        // Sample test data
        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = 1,
                Title = "Complete API Development",
                Description = "Develop REST API endpoints",
                Status = Status.InProgress,
                DueDate = DateTime.UtcNow.AddDays(7),
                AssignedToUserId = 2,
                CreatedByUserId = 1,
                TeamId = 1,
                AssignedToUser = new User { Id = 2, FullName = "Manager User", Email = "manager@example.com", Role = Role.Manager },
                CreatedByUser = new User { Id = 1, FullName = "Admin User", Email = "admin@example.com", Role = Role.Admin },
                Team = new Team { Id = 1, Name = "Development Team", Description = "Software development team" },
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = 2,
                Title = "Write Unit Tests",
                Description = "Write comprehensive unit tests",
                Status = Status.Todo,
                DueDate = DateTime.UtcNow.AddDays(14),
                AssignedToUserId = 3,
                CreatedByUserId = 2,
                TeamId = 1,
                AssignedToUser = new User { Id = 3, FullName = "Employee User", Email = "employee@example.com", Role = Role.Employee },
                CreatedByUser = new User { Id = 2, FullName = "Manager User", Email = "manager@example.com", Role = Role.Manager },
                Team = new Team { Id = 1, Name = "Development Team", Description = "Software development team" },
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = 3,
                Title = "Code Review",
                Description = "Review pull requests",
                Status = Status.Done,
                DueDate = DateTime.UtcNow.AddDays(3),
                AssignedToUserId = 2,
                CreatedByUserId = 1,
                TeamId = 2,
                AssignedToUser = new User { Id = 2, FullName = "Manager User", Email = "manager@example.com", Role = Role.Manager },
                CreatedByUser = new User { Id = 1, FullName = "Admin User", Email = "admin@example.com", Role = Role.Admin },
                Team = new Team { Id = 2, Name = "QA Team", Description = "Quality assurance team" },
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            }
        };

        // Setup GetAllAsync
        mockRepository.Setup(r => r.GetAsync()).ReturnsAsync(tasks);

        // Setup GetByIdAsync
        mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => tasks.FirstOrDefault(t => t.Id == id));

        // Setup GetByAssignedUserAsync
        mockRepository.Setup(r => r.GetByAssignedUserAsync(It.IsAny<int>()))
            .ReturnsAsync((int userId) => tasks.Where(t => t.AssignedToUserId == userId).ToList());

        // Setup GetByCreatedByUserAsync
        mockRepository.Setup(r => r.GetByCreatedByUserAsync(It.IsAny<int>()))
            .ReturnsAsync((int userId) => tasks.Where(t => t.CreatedByUserId == userId).ToList());

        // Setup GetByTeamAsync
        mockRepository.Setup(r => r.GetByTeamAsync(It.IsAny<int>()))
            .ReturnsAsync((int teamId) => tasks.Where(t => t.TeamId == teamId).ToList());

        // Setup GetByStatusAsync
        mockRepository.Setup(r => r.GetByStatusAsync(It.IsAny<Status>()))
            .ReturnsAsync((Status status) => tasks.Where(t => t.Status == status).ToList());

        // Setup GetWithDetailsAsync
        mockRepository.Setup(r => r.GetWithDetailsAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => tasks.FirstOrDefault(t => t.Id == id));

        // Setup GetTasksForUserAsync
        mockRepository.Setup(r => r.GetTasksForUserAsync(It.IsAny<int>(), It.IsAny<Status?>()))
            .ReturnsAsync((int userId, Status? status) =>
            {
                var query = tasks.Where(t => t.AssignedToUserId == userId);
                if (status.HasValue)
                    query = query.Where(t => t.Status == status.Value);
                return query.ToList();
            });

        // Setup SearchTasksAsync
        mockRepository.Setup(r => r.SearchTasksAsync(It.IsAny<TaskSearchRequest>()))
            .ReturnsAsync((TaskSearchRequest request) =>
            {
                var query = tasks.AsQueryable();

                // Apply search term
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(t =>
                        t.Title.Contains(request.SearchTerm) ||
                        (t.Description != null && t.Description.Contains(request.SearchTerm)));
                }

                // Apply filters
                if (request.AssignedToUserId.HasValue)
                    query = query.Where(t => t.AssignedToUserId == request.AssignedToUserId.Value);

                if (request.TeamId.HasValue)
                    query = query.Where(t => t.TeamId == request.TeamId.Value);

                if (request.Status.HasValue)
                    query = query.Where(t => t.Status == request.Status.Value);

                var totalCount = query.Count();

                // Apply pagination
                var items = query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                return new PagedResult<TaskItem> { Items = items, TotalCount = totalCount };
            });

        // Setup CreateAsync
        mockRepository.Setup(r => r.CreateAsync(It.IsAny<TaskItem>()))
            .Callback((TaskItem task) =>
            {
                task.Id = tasks.Max(t => t.Id) + 1;
                task.CreatedDate = DateTime.UtcNow;
                task.LastModifiedDate = DateTime.UtcNow;
                tasks.Add(task);
            });

        // Setup UpdateAsync
        mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>()))
            .Callback((TaskItem task) =>
            {
                var existingTask = tasks.FirstOrDefault(t => t.Id == task.Id);
                if (existingTask != null)
                {
                    existingTask.Title = task.Title;
                    existingTask.Description = task.Description;
                    existingTask.Status = task.Status;
                    existingTask.DueDate = task.DueDate;
                    existingTask.AssignedToUserId = task.AssignedToUserId;
                    existingTask.TeamId = task.TeamId;
                    existingTask.LastModifiedDate = DateTime.UtcNow;
                }
            });

        // Setup DeleteAsync
        mockRepository.Setup(r => r.DeleteAsync(It.IsAny<TaskItem>()))
            .Callback((TaskItem task) =>
            {
                var existingTask = tasks.FirstOrDefault(t => t.Id == task.Id);
                if (existingTask != null)
                {
                    tasks.Remove(existingTask);
                }
            });

        return mockRepository;
    }

    public static Mock<ITaskListRepository> GetMockTaskListRepositoryEmpty()
    {
        var mockRepository = new Mock<ITaskListRepository>();
        var emptyTasks = new List<TaskItem>();

        mockRepository.Setup(r => r.GetAsync()).ReturnsAsync(emptyTasks);
        mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((TaskItem?)null);
        mockRepository.Setup(r => r.GetByAssignedUserAsync(It.IsAny<int>())).ReturnsAsync(emptyTasks);
        mockRepository.Setup(r => r.GetByCreatedByUserAsync(It.IsAny<int>())).ReturnsAsync(emptyTasks);
        mockRepository.Setup(r => r.GetByTeamAsync(It.IsAny<int>())).ReturnsAsync(emptyTasks);
        mockRepository.Setup(r => r.GetByStatusAsync(It.IsAny<Status>())).ReturnsAsync(emptyTasks);
        mockRepository.Setup(r => r.GetWithDetailsAsync(It.IsAny<int>())).ReturnsAsync((TaskItem?)null);
        mockRepository.Setup(r => r.GetTasksForUserAsync(It.IsAny<int>(), It.IsAny<Status?>())).ReturnsAsync(emptyTasks);
        mockRepository.Setup(r => r.SearchTasksAsync(It.IsAny<TaskSearchRequest>()))
            .ReturnsAsync(new PagedResult<TaskItem> { Items = emptyTasks, TotalCount = 0 });

        return mockRepository;
    }
}

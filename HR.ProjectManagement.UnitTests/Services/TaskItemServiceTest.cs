using FluentValidation;
using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities.Enums;
using HR.ProjectManagement.Exceptions;
using HR.ProjectManagement.Services;
using HR.ProjectManagement.UnitTests.Helpers;
using HR.ProjectManagement.UnitTests.Mocks;
using Moq;
using Xunit;

namespace HR.ProjectManagement.UnitTests.Services;

public class TaskItemServiceTest : ServiceTestBase
{
    private readonly Mock<ITaskListRepository> _mockTaskRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITeamRepository> _mockTeamRepository;
    private readonly Mock<IValidator<CreateTaskRequest>> _mockCreateValidator;
    private readonly Mock<IValidator<UpdateTaskRequest>> _mockUpdateValidator;
    private readonly Mock<IValidator<TaskSearchRequest>> _mockSearchValidator;
    private readonly TaskItemService _service;

    public TaskItemServiceTest()
    {
        _mockTaskRepository = MockTaskListRepository.GetMockTaskListRepository();
        _mockUserRepository = MockUserRepository.GetMockUserRepository();
        _mockTeamRepository = MockTeamRepository.GetMockTeamRepository();
        _mockCreateValidator = new Mock<IValidator<CreateTaskRequest>>();
        _mockUpdateValidator = new Mock<IValidator<UpdateTaskRequest>>();
        _mockSearchValidator = new Mock<IValidator<TaskSearchRequest>>();

        ValidatorTestHelper.SetupValidatorToPass(_mockCreateValidator);
        ValidatorTestHelper.SetupValidatorToPass(_mockUpdateValidator);
        ValidatorTestHelper.SetupValidatorToPass(_mockSearchValidator);

        _service = new TaskItemService(
            _mockTaskRepository.Object,
            _mockUserRepository.Object,
            _mockTeamRepository.Object,
            MockUnitOfWork.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object,
            _mockSearchValidator.Object
        );
    }

    #region Query Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllTasks()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsTask()
    {
        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Complete API Development", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var service = CreateServiceWithEmptyRepository();

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTasksByUserAsync_ValidUserId_ReturnsUserTasks()
    {
        // Act
        var result = await _service.GetTasksByUserAsync(2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetTasksByTeamAsync_ValidTeamId_ReturnsTeamTasks()
    {
        // Act
        var result = await _service.GetTasksByTeamAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetTasksByStatusAsync_ValidStatus_ReturnsTasksWithStatus()
    {
        // Act
        var result = await _service.GetTasksByStatusAsync(Status.Done);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(Status.Done, result[0].Status);
    }

    [Fact]
    public async Task GetMyTasksAsync_ValidUserId_ReturnsUserTasks()
    {
        // Act
        var result = await _service.GetMyTasksAsync(2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetWithDetailsAsync_ExistingId_ReturnsTaskWithDetails()
    {
        // Act
        var result = await _service.GetWithDetailsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.NotNull(result.AssignedToUser);
        Assert.NotNull(result.CreatedByUser);
        Assert.NotNull(result.Team);
    }

    [Fact]
    public async Task GetWithDetailsAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var service = CreateServiceWithEmptyRepository();

        // Act
        var result = await service.GetWithDetailsAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchTasksAsync_ValidRequest_ReturnsPagedResults()
    {
        // Arrange
        var request = new TaskSearchRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Status = Status.Todo
        };

        // Act
        var result = await _service.SearchTasksAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.NotNull(result.Data);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsTaskResponse()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "New task description",
            AssignedToUserId = 2,
            TeamId = 1,
            DueDate = DateTime.UtcNow.AddDays(10)
        };

        // Act
        var result = await _service.CreateAsync(request, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Task", result.Title);
        Assert.Equal(Status.Todo, result.Status);

        _mockTaskRepository.Verify(r => r.CreateAsync(It.IsAny<Entities.TaskItem>()), Times.Once);
        MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_UserDoesNotExist_ThrowsBadRequestException()
    {
        // Arrange
        var service = CreateServiceWithEmptyUserRepository();
        var request = CreateValidTaskRequest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateAsync(request, 1)
        );

        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_TeamDoesNotExist_ThrowsBadRequestException()
    {
        // Arrange
        var service = CreateServiceWithEmptyTeamRepository();
        var request = CreateValidTaskRequest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateAsync(request, 1)
        );

        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_PastDueDate_ThrowsBadRequestException()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            AssignedToUserId = 2,
            TeamId = 1,
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _service.CreateAsync(request, 1)
        );

        Assert.Contains("must be in the future", exception.Message);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_ExistingTask_ReturnsUpdatedTask()
    {
        // Arrange
        var request = new UpdateTaskRequest
        {
            Title = "Updated Task",
            Description = "Updated description",
            Status = Status.InProgress,
            DueDate = DateTime.UtcNow.AddDays(15)
        };

        // Act
        var result = await _service.UpdateAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Task", result.Title);
        Assert.Equal(Status.InProgress, result.Status);

        _mockTaskRepository.Verify(r => r.UpdateAsync(It.IsAny<Entities.TaskItem>()), Times.Once);
        MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var service = CreateServiceWithEmptyRepository();
        var request = new UpdateTaskRequest
        {
            Title = "Updated Task",
            Status = Status.Todo,
            DueDate = DateTime.UtcNow.AddDays(10)
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateAsync(999, request)
        );
    }

    #endregion

    #region Status Update Tests

    [Fact]
    public async Task UpdateStatusAsync_ValidStatusUpdate_ReturnsUpdatedTask()
    {
        // Arrange
        var request = new UpdateTaskStatusRequest { Status = Status.InProgress };

        // Act
        var result = await _service.UpdateStatusAsync(2, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Status.InProgress, result.Status);

        _mockTaskRepository.Verify(r => r.UpdateAsync(It.IsAny<Entities.TaskItem>()), Times.Once);
        MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_FromDoneToOther_ThrowsBadRequestException()
    {
        // Arrange
        var request = new UpdateTaskStatusRequest { Status = Status.Todo };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _service.UpdateStatusAsync(3, request)
        );

        Assert.Contains("Cannot change status from Done", exception.Message);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_ExistingTask_ReturnsTrue()
    {
        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
        _mockTaskRepository.Verify(r => r.DeleteAsync(It.IsAny<Entities.TaskItem>()), Times.Once);
        MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var service = CreateServiceWithEmptyRepository();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteAsync(999)
        );
    }

    #endregion

    #region Helper Methods

    private TaskItemService CreateServiceWithEmptyRepository()
    {
        var emptyRepo = MockTaskListRepository.GetMockTaskListRepositoryEmpty();
        return new TaskItemService(
            emptyRepo.Object,
            _mockUserRepository.Object,
            _mockTeamRepository.Object,
            MockUnitOfWork.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object,
            _mockSearchValidator.Object
        );
    }

    private TaskItemService CreateServiceWithEmptyUserRepository()
    {
        var emptyUserRepo = MockUserRepository.GetMockUserRepositoryEmpty();
        return new TaskItemService(
            _mockTaskRepository.Object,
            emptyUserRepo.Object,
            _mockTeamRepository.Object,
            MockUnitOfWork.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object,
            _mockSearchValidator.Object
        );
    }

    private TaskItemService CreateServiceWithEmptyTeamRepository()
    {
        var emptyTeamRepo = MockTeamRepository.GetMockTeamRepositoryEmpty();
        return new TaskItemService(
            _mockTaskRepository.Object,
            _mockUserRepository.Object,
            emptyTeamRepo.Object,
            MockUnitOfWork.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object,
            _mockSearchValidator.Object
        );
    }

    private static CreateTaskRequest CreateValidTaskRequest()
    {
        return new CreateTaskRequest
        {
            Title = "New Task",
            AssignedToUserId = 2,
            TeamId = 1,
            DueDate = DateTime.UtcNow.AddDays(10)
        };
    }

    #endregion
}

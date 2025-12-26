using FluentValidation;
using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Exceptions;
using HR.ProjectManagement.Services;
using HR.ProjectManagement.UnitTests.Mocks;
using Moq;
using Xunit;

namespace HR.ProjectManagement.UnitTests.Services;

public class TeamServiceTest
{
    private readonly Mock<ITeamRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IValidator<CreateTeamRequest>> _mockCreateValidator;
    private readonly Mock<IValidator<UpdateTeamRequest>> _mockUpdateValidator;
    private readonly TeamService _service;

    public TeamServiceTest()
    {
        _mockRepository = MockTeamRepository.GetMockTeamRepository();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCreateValidator = new Mock<IValidator<CreateTeamRequest>>();
        _mockUpdateValidator = new Mock<IValidator<UpdateTeamRequest>>();

        // Setup validators to pass by default
        _mockCreateValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateTeamRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockUpdateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateTeamRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _service = new TeamService(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object
        );
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTeams()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsTeam()
    {
        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Development Team", result.Name);
        Assert.Equal("Software development team", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var emptyRepo = MockTeamRepository.GetMockTeamRepositoryEmpty();
        var service = new TeamService(
            emptyRepo.Object,
            _mockUnitOfWork.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object
        );

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsTeamResponse()
    {
        // Arrange
        var request = new CreateTeamRequest
        {
            Name = "New Team",
            Description = "A new team description"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Team", result.Name);
        Assert.Equal("A new team description", result.Description);

        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Team>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTeam_ReturnsUpdatedTeam()
    {
        // Arrange
        var request = new UpdateTeamRequest
        {
            Name = "Updated Development Team",
            Description = "Updated description"
        };

        // Act
        var result = await _service.UpdateAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Development Team", result.Name);
        Assert.Equal("Updated description", result.Description);

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Team>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ThrowsBadRequestException()
    {
        // Arrange
        var request = new UpdateTeamRequest
        {
            Name = "QA Team", // Name already exists
            Description = "Test description"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _service.UpdateAsync(1, request)
        );

        Assert.Contains("already in use", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var emptyRepo = MockTeamRepository.GetMockTeamRepositoryEmpty();
        var service = new TeamService(
            emptyRepo.Object,
            _mockUnitOfWork.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object
        );

        var request = new UpdateTeamRequest
        {
            Name = "Test Team",
            Description = "Test description"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateAsync(999, request)
        );
    }

    [Fact]
    public async Task DeleteAsync_ExistingTeam_ReturnsTrue()
    {
        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Team>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_TeamWithTasks_ThrowsBadRequestException()
    {
        // Arrange
        var repoWithTasks = MockTeamRepository.GetMockTeamRepositoryWithTasks();
        var service = new TeamService(
            repoWithTasks.Object,
            _mockUnitOfWork.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.DeleteAsync(1)
        );

        Assert.Contains("existing tasks", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var emptyRepo = MockTeamRepository.GetMockTeamRepositoryEmpty();
        var service = new TeamService(
            emptyRepo.Object,
            _mockUnitOfWork.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteAsync(999)
        );
    }

    [Fact]
    public async Task GetWithMembersAsync_ExistingId_ReturnsTeamWithMembers()
    {
        // Act
        var result = await _service.GetWithMembersAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Development Team", result.Name);
        Assert.NotNull(result.Members);
    }

    [Fact]
    public async Task GetWithMembersAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var emptyRepo = MockTeamRepository.GetMockTeamRepositoryEmpty();
        var service = new TeamService(
            emptyRepo.Object,
            _mockUnitOfWork.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object
        );

        // Act
        var result = await service.GetWithMembersAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetWithTasksAsync_ExistingId_ReturnsTeamWithTasks()
    {
        // Act
        var result = await _service.GetWithTasksAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Development Team", result.Name);
        Assert.NotNull(result.Tasks);
    }

    [Fact]
    public async Task GetWithTasksAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var emptyRepo = MockTeamRepository.GetMockTeamRepositoryEmpty();
        var service = new TeamService(
            emptyRepo.Object,
            _mockUnitOfWork.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object
        );

        // Act
        var result = await service.GetWithTasksAsync(999);

        // Assert
        Assert.Null(result);
    }
}

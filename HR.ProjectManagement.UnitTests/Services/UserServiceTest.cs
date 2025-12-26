using FluentValidation;
using HR.ProjectManagement.Contracts.Identity;
using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;
using HR.ProjectManagement.Exceptions;
using HR.ProjectManagement.Services;
using HR.ProjectManagement.UnitTests.Helpers;
using HR.ProjectManagement.UnitTests.Mocks;
using Moq;
using Xunit;

namespace HR.ProjectManagement.UnitTests.Services;

public class UserServiceTest : ServiceTestBase
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IValidator<CreateUserRequest>> _mockCreateValidator;
    private readonly Mock<IValidator<UpdateUserRequest>> _mockUpdateValidator;
    private readonly UserService _service;

    public UserServiceTest()
    {
        _mockRepository = MockUserRepository.GetMockUserRepository();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockCreateValidator = new Mock<IValidator<CreateUserRequest>>();
        _mockUpdateValidator = new Mock<IValidator<UpdateUserRequest>>();

        _mockPasswordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed_password");
        ValidatorTestHelper.SetupValidatorToPass(_mockCreateValidator);
        ValidatorTestHelper.SetupValidatorToPass(_mockUpdateValidator);

        _service = new UserService(
            _mockRepository.Object,
            MockUnitOfWork.Object,
            _mockPasswordHasher.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object
        );
    }

    #region Query Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsUser()
    {
        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Admin User", result.FullName);
        Assert.Equal("admin@example.com", result.Email);
        Assert.Equal(Role.Admin, result.Role);
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
    public async Task GetByRoleAsync_FilterByRole_ReturnsCorrectUsers()
    {
        // Act
        var result = await _service.GetByRoleAsync(Role.Employee);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Employee User", result[0].FullName);
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
    {
        // Act
        var result = await _service.GetByEmailAsync("admin@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("admin@example.com", result.Email);
        Assert.Equal("Admin User", result.FullName);
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistingEmail_ReturnsNull()
    {
        // Arrange
        var service = CreateServiceWithEmptyRepository();

        // Act
        var result = await service.GetByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsUserResponse()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FullName = "New User",
            Email = "newuser@example.com",
            Role = Role.Employee,
            Password = "password123"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New User", result.FullName);
        Assert.Equal("newuser@example.com", result.Email);
        Assert.Equal(Role.Employee, result.Role);

        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockPasswordHasher.Verify(p => p.Hash("password123"), Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_ExistingUser_ReturnsUpdatedUser()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            FullName = "Updated Admin User",
            Email = "updatedadmin@example.com",
            Role = Role.Manager,
            Password = "newpassword123"
        };

        // Act
        var result = await _service.UpdateAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Admin User", result.FullName);
        Assert.Equal("updatedadmin@example.com", result.Email);
        Assert.Equal(Role.Manager, result.Role);

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
        MockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_EmailAlreadyExists_ThrowsBadRequestException()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            FullName = "Test User",
            Email = "manager@example.com",
            Role = Role.Employee
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
        var service = CreateServiceWithEmptyRepository();
        var request = new UpdateUserRequest
        {
            FullName = "Test User",
            Email = "test@example.com",
            Role = Role.Employee
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateAsync(999, request)
        );
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_ExistingUser_ReturnsTrue()
    {
        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<User>()), Times.Once);
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

    private UserService CreateServiceWithEmptyRepository()
    {
        var emptyRepo = MockUserRepository.GetMockUserRepositoryEmpty();
        return new UserService(
            emptyRepo.Object,
            MockUnitOfWork.Object,
            _mockPasswordHasher.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object
        );
    }

    #endregion
}

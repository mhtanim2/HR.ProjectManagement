using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;
using Moq;

namespace HR.ProjectManagement.UnitTests.Mocks;

public static class MockUserRepository
{
    public static Mock<IUserRepository> GetMockUserRepository()
    {
        var mockRepository = new Mock<IUserRepository>();

        // Sample test data
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                FullName = "Admin User",
                Email = "admin@example.com",
                Role = Role.Admin,
                PasswordHash = "hashed_password_1",
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            },
            new User
            {
                Id = 2,
                FullName = "Manager User",
                Email = "manager@example.com",
                Role = Role.Manager,
                PasswordHash = "hashed_password_2",
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            },
            new User
            {
                Id = 3,
                FullName = "Employee User",
                Email = "employee@example.com",
                Role = Role.Employee,
                PasswordHash = "hashed_password_3",
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            }
        };

        // Setup GetAllAsync
        mockRepository.Setup(r => r.GetAsync()).ReturnsAsync(users);

        // Setup GetByIdAsync
        mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => users.FirstOrDefault(u => u.Id == id));

        // Setup GetByEmailAsync
        mockRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) => users.FirstOrDefault(u => u.Email == email));

        // Setup IsAnyUserAvailableByEmail
        mockRepository.Setup(r => r.IsAnyUserAvailableByEmail(It.IsAny<string>()))
            .ReturnsAsync((string email) => users.Any(u => u.Email == email));

        // Setup IsAnyUserAvailableByEmailExcept
        mockRepository.Setup(r => r.IsAnyUserAvailableByEmailExcept(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((int id, string email) => users.Any(u => u.Email == email && u.Id != id));

        // Setup GetByRoleAsync
        mockRepository.Setup(r => r.GetByRoleAsync(It.IsAny<Role>()))
            .ReturnsAsync((Role role) => users.Where(u => u.Role == role).ToList());

        // Setup CreateAsync
        mockRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Callback((User user) =>
            {
                user.Id = users.Max(u => u.Id) + 1;
                user.CreatedDate = DateTime.UtcNow;
                user.LastModifiedDate = DateTime.UtcNow;
                users.Add(user);
            });

        // Setup UpdateAsync
        mockRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Callback((User user) =>
            {
                var existingUser = users.FirstOrDefault(u => u.Id == user.Id);
                if (existingUser != null)
                {
                    existingUser.FullName = user.FullName;
                    existingUser.Email = user.Email;
                    existingUser.Role = user.Role;
                    existingUser.PasswordHash = user.PasswordHash;
                    existingUser.LastModifiedDate = DateTime.UtcNow;
                }
            });

        // Setup DeleteAsync
        mockRepository.Setup(r => r.DeleteAsync(It.IsAny<User>()))
            .Callback((User user) =>
            {
                var existingUser = users.FirstOrDefault(u => u.Id == user.Id);
                if (existingUser != null)
                {
                    users.Remove(existingUser);
                }
            });

        return mockRepository;
    }

    public static Mock<IUserRepository> GetMockUserRepositoryEmpty()
    {
        var mockRepository = new Mock<IUserRepository>();
        var emptyUsers = new List<User>();

        mockRepository.Setup(r => r.GetAsync()).ReturnsAsync(emptyUsers);
        mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);
        mockRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        mockRepository.Setup(r => r.IsAnyUserAvailableByEmail(It.IsAny<string>())).ReturnsAsync(false);
        mockRepository.Setup(r => r.IsAnyUserAvailableByEmailExcept(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(false);
        mockRepository.Setup(r => r.GetByRoleAsync(It.IsAny<Role>())).ReturnsAsync(emptyUsers);

        return mockRepository;
    }
}

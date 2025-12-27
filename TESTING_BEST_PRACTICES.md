# Testing Best Practices & Guidelines

This document outlines the clean code and SOLID principles applied to testing in the HR.ProjectManagement project.

## Table of Contents

1. [Integration Tests](#integration-tests)
2. [Unit Tests](#unit-tests)
3. [Test Structure](#test-structure)
4. [Clean Code Principles](#clean-code-principles)
5. [SOLID Principles in Testing](#solid-principles-in-testing)

---

## Integration Tests

### Location
`HR.ProjectManagement.Persistence.IntegrationTests/`

### Key Files

#### Base Test Class
**[TestDataBase.cs](HR.ProjectManagement.Persistence.IntegrationTests/Helpers/TestDataBase.cs)**
- Provides common setup/teardown for all integration tests
- Manages InMemory database lifecycle
- Sets up ICurrentUserService mock for audit trail testing

```csharp
public abstract class TestDataBase : IDisposable
{
    protected readonly ApplicationDBContext Context;
    protected readonly Mock<ICurrentUserService> MockCurrentUser;

    protected TestDataBase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        MockCurrentUser = new Mock<ICurrentUserService>();
        MockCurrentUser.Setup(x => x.UserId).Returns(1);

        Context = new ApplicationDBContext(options, MockCurrentUser.Object);
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
```

#### Test Data Builders
**[EntityTestDataBuilder.cs](HR.ProjectManagement.Persistence.IntegrationTests/Helpers/EntityTestDataBuilder.cs)**

Uses the Builder Pattern to create test entities with sensible defaults:

```csharp
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
        string? description = null) { ... }

    public static TaskItem BuildTask(...) { ... }
    public static TeamMember BuildTeamMember(int userId, int teamId) { ... }
    public static RefreshToken BuildRefreshToken(...) { ... }
    public static PasswordReset BuildPasswordReset(...) { ... }
}
```

**Benefits:**
- Eliminates code duplication
- Provides sensible defaults
- Makes tests more readable
- Easy to modify test data

#### Custom Assertions
**[TestAssertions.cs](HR.ProjectManagement.Persistence.IntegrationTests/Helpers/TestAssertions.cs)**

Encapsulates common assertion logic:

```csharp
public static class TestAssertions
{
    public static void AssertAuditFields(BaseEntity entity, int expectedUserId = 1)
    {
        Assert.NotNull(entity.CreatedDate);
        Assert.NotNull(entity.LastModifiedDate);
        Assert.Equal(expectedUserId, entity.CreatedBy);
        Assert.Equal(expectedUserId, entity.ModifiedBy);
    }

    public static void AssertAuditFieldsUpdated(BaseEntity entity, DateTime originalModifiedDate)
    {
        Assert.True(entity.LastModifiedDate > originalModifiedDate);
    }
}
```

### Test Organization by Entity

Tests are split into focused test classes by entity:

| Test File | Responsibility | Test Count |
|-----------|----------------|------------|
| [DbSetsInitializationTests.cs](HR.ProjectManagement.Persistence.IntegrationTests/DbSetsInitializationTests.cs) | Verify DbSets initialization | 1 |
| [UserEntityTests.cs](HR.ProjectManagement.Persistence.IntegrationTests/UserEntityTests.cs) | User CRUD & audit trail | 5 |
| [TeamEntityTests.cs](HR.ProjectManagement.Persistence.IntegrationTests/TeamEntityTests.cs) | Team CRUD | 1 |
| [TaskItemEntityTests.cs](HR.ProjectManagement.Persistence.IntegrationTests/TaskItemEntityTests.cs) | Task CRUD, relationships, queries | 4 |
| [TeamMemberRelationshipTests.cs](HR.ProjectManagement.Persistence.IntegrationTests/TeamMemberRelationshipTests.cs) | Many-to-many relationships | 4 |
| [RefreshTokenEntityTests.cs](HR.ProjectManagement.Persistence.IntegrationTests/RefreshTokenEntityTests.cs) | Token CRUD & cascade delete | 3 |
| [PasswordResetEntityTests.cs](HR.ProjectManagement.Persistence.IntegrationTests/PasswordResetEntityTests.cs) | Password reset CRUD | 1 |

**Total Integration Tests: 19**

**Before Refactoring:**
- Single file: ApplicationDBContextTest.cs (763 lines)
- Violated Single Responsibility Principle
- Hard to navigate and maintain

**After Refactoring:**
- 8 focused test classes
- Average ~50-100 lines per class
- Each class has one clear responsibility
- Easy to find and maintain specific tests

---

## Unit Tests

### Location
`HR.ProjectManagement.UnitTests/`

### Key Files

#### Base Test Class for Services
**[ServiceTestBase.cs](HR.ProjectManagement.UnitTests/Helpers/ServiceTestBase.cs)**

Provides common setup for all service tests:

```csharp
public abstract class ServiceTestBase
{
    protected readonly Mock<IUnitOfWork> MockUnitOfWork;

    protected ServiceTestBase()
    {
        MockUnitOfWork = new Mock<IUnitOfWork>();
        MockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
    }
}
```

#### Validator Test Helper
**[ValidatorTestHelper.cs](HR.ProjectManagement.UnitTests/Helpers/ValidatorTestHelper.cs)**

Simplifies validator mock setup:

```csharp
public static class ValidatorTestHelper
{
    public static void SetupValidatorToPass<T>(Mock<IValidator<T>> validator)
        where T : class
    {
        validator.Setup(v => v.ValidateAsync(It.IsAny<T>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    public static void SetupValidatorToFail<T>(
        Mock<IValidator<T>> validator,
        string errorMessage,
        string propertyName = "") where T : class
    {
        var failure = new FluentValidation.Results.ValidationFailure(propertyName, errorMessage);
        var result = new FluentValidation.Results.ValidationResult();
        result.Errors.Add(failure);

        validator.Setup(v => v.ValidateAsync(It.IsAny<T>(), default))
            .ReturnsAsync(result);
    }
}
```

### Test Organization by Service

| Test File | Responsibility | Test Count |
|-----------|----------------|------------|
| [UserServiceTest.cs](HR.ProjectManagement.UnitTests/Services/UserServiceTest.cs) | User service CRUD & queries | 12 |
| [TeamServiceTest.cs](HR.ProjectManagement.UnitTests/Services/TeamServiceTest.cs) | Team service CRUD & relationships | 11 |
| [TaskItemServiceTest.cs](HR.ProjectManagement.UnitTests/Services/TaskItemServiceTest.cs) | Task service CRUD, workflow, search | 23 |

**Total Unit Tests: 46**

### Test Structure Within Classes

Tests are organized using `#region` directives:

```csharp
public class UserServiceTest : ServiceTestBase
{
    #region Query Tests
    [Fact] public async Task GetAllAsync_ReturnsAllUsers() { }
    [Fact] public async Task GetByIdAsync_ExistingId_ReturnsUser() { }
    #endregion

    #region Create Tests
    [Fact] public async Task CreateAsync_ValidRequest_ReturnsUserResponse() { }
    #endregion

    #region Update Tests
    [Fact] public async Task UpdateAsync_ExistingUser_ReturnsUpdatedUser() { }
    #endregion

    #region Delete Tests
    [Fact] public async Task DeleteAsync_ExistingUser_ReturnsTrue() { }
    #endregion

    #region Helper Methods
    private UserService CreateServiceWithEmptyRepository() { }
    #endregion
}
```

### Example: Refactored Test

**Before (266 lines):**
```csharp
public class UserServiceTest
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IValidator<CreateUserRequest>> _mockCreateValidator;
    private readonly Mock<IValidator<UpdateUserRequest>> _mockUpdateValidator;
    private readonly UserService _service;

    public UserServiceTest()
    {
        _mockRepository = MockUserRepository.GetMockUserRepository();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockCreateValidator = new Mock<IValidator<CreateUserRequest>>();
        _mockUpdateValidator = new Mock<IValidator<UpdateUserRequest>>();

        // Setup password hasher
        _mockPasswordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed_password");

        // Setup validators to pass by default
        _mockCreateValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateUserRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockUpdateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateUserRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _service = new UserService(...);
    }
    // ... 12 tests with duplicated service creation logic
}
```

**After (262 lines, but more maintainable):**
```csharp
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

        _service = new UserService(...);
    }

    #region Query Tests
    [Fact] public async Task GetAllAsync_ReturnsAllUsers() { }
    #endregion

    #region Create Tests
    [Fact] public async Task CreateAsync_ValidRequest_ReturnsUserResponse() { }
    #endregion

    #region Update Tests
    [Fact] public async Task UpdateAsync_ExistingUser_ReturnsUpdatedUser() { }
    [Fact] public async Task UpdateAsync_EmailAlreadyExists_ThrowsBadRequestException() { }
    #endregion

    #region Delete Tests
    [Fact] public async Task DeleteAsync_ExistingUser_ReturnsTrue() { }
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
```

**Improvements:**
- Inherits from `ServiceTestBase` (DRY principle)
- Uses `ValidatorTestHelper` for cleaner validator setup
- Organized into logical regions
- Helper methods extract duplicated service creation logic
- More readable and maintainable

---

## Test Structure

### AAA Pattern (Arrange-Act-Assert)

All tests follow the AAA pattern:

```csharp
[Fact]
public async Task Can_Add_User_To_Database()
{
    // Arrange
    var user = EntityTestDataBuilder.BuildUser();

    // Act
    await Context.Users.AddAsync(user);
    await Context.SaveChangesAsync();

    // Assert
    var savedUser = await Context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
    Assert.NotNull(savedUser);
    Assert.Equal("Test User", savedUser.FullName);
}
```

### Test Naming Convention

Tests use descriptive names following the pattern:
```
MethodName_StateUnderTest_ExpectedBehavior
```

Examples:
- `GetAllAsync_ReturnsAllTasks`
- `GetByIdAsync_ExistingId_ReturnsTask`
- `GetByIdAsync_NonExistingId_ReturnsNull`
- `CreateAsync_ValidRequest_ReturnsTaskResponse`
- `CreateAsync_UserDoesNotExist_ThrowsBadRequestException`
- `UpdateStatusAsync_FromDoneToOther_ThrowsBadRequestException`

---

## Clean Code Principles

### 1. DRY (Don't Repeat Yourself)

**Problem:** Repeated test setup code
**Solution:**
- Base test classes ([TestDataBase](HR.ProjectManagement.Persistence.IntegrationTests/Helpers/TestDataBase.cs), [ServiceTestBase](HR.ProjectManagement.UnitTests/Helpers/ServiceTestBase.cs))
- Test data builders ([EntityTestDataBuilder](HR.ProjectManagement.Persistence.IntegrationTests/Helpers/EntityTestDataBuilder.cs))
- Helper methods in test classes
- Custom assertions ([TestAssertions](HR.ProjectManagement.Persistence.IntegrationTests/Helpers/TestAssertions.cs))

### 2. Single Responsibility Principle (SRP)

**Problem:** Large test files with multiple responsibilities
**Solution:**
- Split ApplicationDBContextTest.cs (763 lines) into 8 focused test classes
- Each test class tests one entity or feature
- Test classes organized by entity/domain concept

**Before:**
```
ApplicationDBContextTest.cs (763 lines)
- DbSet tests
- User tests
- Team tests
- Task tests
- TeamMember tests
- RefreshToken tests
- PasswordReset tests
- Cascade delete tests
- Query tests
- Audit tests
```

**After:**
```
DbSetsInitializationTests.cs (30 lines) - DbSets
UserEntityTests.cs (110 lines) - User CRUD & audit
TeamEntityTests.cs (25 lines) - Team CRUD
TaskItemEntityTests.cs (150 lines) - Task CRUD & queries
TeamMemberRelationshipTests.cs (120 lines) - Relationships
RefreshTokenEntityTests.cs (85 lines) - Tokens & cascade
PasswordResetEntityTests.cs (25 lines) - Password reset
```

### 3. Readability

**Improvements:**
- Clear test names
- AAA pattern with comments
- Logical regions (#region)
- Helper methods with descriptive names
- Test data builders instead of inline object creation

**Before:**
```csharp
var user = new User
{
    FullName = "Test User",
    Email = "test@example.com",
    Role = Role.Employee,
    PasswordHash = "hashed_password"
};
```

**After:**
```csharp
var user = EntityTestDataBuilder.BuildUser();
```

### 4. Maintainability

**Improvements:**
- Centralized test data management
- Easy to modify test data in one place
- Clear separation between test data and test logic
- Reusable test infrastructure

---

## SOLID Principles in Testing

### S - Single Responsibility Principle

Each test class has one reason to change:
- [UserEntityTests](HR.ProjectManagement.Persistence.IntegrationTests/UserEntityTests.cs) - Tests User entity persistence
- [TeamEntityTests](HR.ProjectManagement.Persistence.IntegrationTests/TeamEntityTests.cs) - Tests Team entity persistence
- [TaskItemEntityTests](HR.ProjectManagement.Persistence.IntegrationTests/TaskItemEntityTests.cs) - Tests TaskItem entity persistence
- [TeamMemberRelationshipTests](HR.ProjectManagement.Persistence.IntegrationTests/TeamMemberRelationshipTests.cs) - Tests many-to-many relationships

### O - Open/Closed Principle

Test infrastructure is open for extension but closed for modification:
- Base classes can be inherited without modification
- New test classes can use existing builders and helpers
- Test helpers are extensible (e.g., `ValidatorTestHelper`)

### L - Liskov Substitution Principle

Any test class inheriting from [TestDataBase](HR.ProjectManagement.Persistence.IntegrationTests/Helpers/TestDataBase.cs) or [ServiceTestBase](HR.ProjectManagement.UnitTests/Helpers/ServiceTestBase.cs) can be used interchangeably:

```csharp
// All these classes can be used wherever TestDataBase is expected
public class UserEntityTests : TestDataBase { }
public class TeamEntityTests : TestDataBase { }
public class TaskItemEntityTests : TestDataBase { }
```

### I - Interface Segregation Principle

Test helpers are focused and don't force unnecessary dependencies:
- [ValidatorTestHelper](HR.ProjectManagement.UnitTests/Helpers/ValidatorTestHelper.cs) - Only validator-related helpers
- [EntityTestDataBuilder](HR.ProjectManagement.Persistence.IntegrationTests/Helpers/EntityTestDataBuilder.cs) - Only entity builders
- [TestAssertions](HR.ProjectManagement.Persistence.IntegrationTests/Helpers/TestAssertions.cs) - Only assertion helpers

### D - Dependency Inversion Principle

Test classes depend on abstractions (base classes, interfaces) rather than concrete implementations:
- Inherit from abstract base classes
- Use mocked dependencies (Moq)
- Test to interfaces (IUserRepository, IUnitOfWork, etc.)

---

## Benefits Achieved

### Code Metrics

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| Integration test file size | 763 lines | ~50-150 lines per file | 80-90% reduction |
| Test class responsibilities | 1 class, 10+ responsibilities | 8 classes, 1 each | Clear SRP |
| Test setup duplication | High | Low | DRY applied |
| Test readability | Medium | High | Builders & helpers |

### Maintainability

- **Easy to find tests:** Organized by entity/feature
- **Easy to modify tests:** Centralized test data and helpers
- **Easy to add tests:** Clear structure and patterns to follow
- **Easy to understand:** Descriptive names and AAA pattern

### Test Coverage

- **Total Tests:** 65 (46 unit + 19 integration)
- **All Passing:** 100%
- **Coverage:**
  - CRUD operations for all entities
  - Business rules (e.g., task status flow, team deletion constraints)
  - Entity relationships (one-to-many, many-to-many)
  - Cascade delete behavior
  - Audit trail functionality
  - Query operations (filtering, includes, pagination)

---

## Running Tests

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "FullyQualifiedName~UnitTests"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Run specific test class
dotnet test --filter "FullyQualifiedName~UserServiceTest"

# Run specific test
dotnet test --filter "FullyQualifiedName~Can_Add_User_To_Database"
```

---

## Guidelines for Adding New Tests

1. **Choose the right test class:**
   - Integration test? Add to appropriate entity test class in `Persistence.IntegrationTests`
   - Unit test? Add to appropriate service test class in `UnitTests/Services`

2. **Use test data builders:**
   ```csharp
   var user = EntityTestDataBuilder.BuildUser(
       fullName: "Specific User",
       email: "specific@example.com"
   );
   ```

3. **Follow AAA pattern:**
   ```csharp
   [Fact]
   public async Task Method_State_ExpectedBehavior()
   {
       // Arrange
       // ...

       // Act
       // ...

       // Assert
       // ...
   }
   ```

4. **Use custom assertions:**
   ```csharp
   TestAssertions.AssertAuditFields(savedEntity);
   ```

5. **Organize with regions:**
   ```csharp
   #region Feature Tests
   [Fact] ...
   #endregion
   ```

6. **Extract helper methods for duplication:**
   ```csharp
   #region Helper Methods
   private YourService CreateServiceWithSpecialSetup() { }
   #endregion
   ```

---

## Summary

By applying clean code and SOLID principles to testing, we've achieved:

1. **More maintainable tests** - Smaller, focused test classes
2. **Less duplication** - Base classes, builders, helpers
3. **Better readability** - Clear names, AAA pattern, regions
4. **Easier to extend** - Clear patterns and infrastructure
5. **Higher quality** - All 65 tests passing with good coverage

The refactoring transformed a monolithic 763-line test file into a well-organized test suite with:
- 8 focused integration test classes
- 3 organized unit test classes
- Reusable test infrastructure (base classes, builders, helpers)
- Clear patterns to follow for future tests

This approach ensures the test suite remains maintainable and scalable as the application grows.

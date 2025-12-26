using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.Entities;
using Moq;

namespace HR.ProjectManagement.UnitTests.Mocks;

public static class MockTeamRepository
{
    public static Mock<ITeamRepository> GetMockTeamRepository()
    {
        var mockRepository = new Mock<ITeamRepository>();

        // Sample test data
        var teams = new List<Team>
        {
            new Team
            {
                Id = 1,
                Name = "Development Team",
                Description = "Software development team",
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Members = new List<TeamMember>()
            },
            new Team
            {
                Id = 2,
                Name = "QA Team",
                Description = "Quality assurance team",
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Members = new List<TeamMember>()
            },
            new Team
            {
                Id = 3,
                Name = "DevOps Team",
                Description = "DevOps and infrastructure team",
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Members = new List<TeamMember>()
            }
        };

        // Setup GetAllAsync (inherited from GenericRepository)
        mockRepository.Setup(r => r.GetAsync()).ReturnsAsync(teams);

        // Setup GetByIdAsync
        mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => teams.FirstOrDefault(t => t.Id == id));

        // Setup GetByNameAsync
        mockRepository.Setup(r => r.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => teams.FirstOrDefault(t => t.Name == name));

        // Setup IsAnyTeamAvailableByNameAsync
        mockRepository.Setup(r => r.IsAnyTeamAvailableByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => teams.Any(t => t.Name == name));

        // Setup GetWithMembersAsync
        mockRepository.Setup(r => r.GetWithMembersAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => teams.FirstOrDefault(t => t.Id == id));

        // Setup GetWithTasksAsync
        mockRepository.Setup(r => r.GetWithTasksAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) =>
            {
                var team = teams.FirstOrDefault(t => t.Id == id);
                if (team != null)
                {
                    // Return a copy with Tasks initialized
                    var teamWithTasks = new Team
                    {
                        Id = team.Id,
                        Name = team.Name,
                        Description = team.Description,
                        CreatedDate = team.CreatedDate,
                        LastModifiedDate = team.LastModifiedDate,
                        Members = team.Members,
                        Tasks = new List<Entities.TaskItem>() // Empty tasks list for testing
                    };
                    return teamWithTasks;
                }
                return null;
            });

        // Setup GetWithMembersAndTasksAsync
        mockRepository.Setup(r => r.GetWithMembersAndTasksAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => teams.FirstOrDefault(t => t.Id == id));

        // Setup CreateAsync
        mockRepository.Setup(r => r.CreateAsync(It.IsAny<Team>()))
            .Callback((Team team) =>
            {
                team.Id = teams.Max(t => t.Id) + 1;
                team.CreatedDate = DateTime.UtcNow;
                team.LastModifiedDate = DateTime.UtcNow;
                team.Members = new List<TeamMember>();
                teams.Add(team);
            });

        // Setup UpdateAsync
        mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Team>()))
            .Callback((Team team) =>
            {
                var existingTeam = teams.FirstOrDefault(t => t.Id == team.Id);
                if (existingTeam != null)
                {
                    existingTeam.Name = team.Name;
                    existingTeam.Description = team.Description;
                    existingTeam.LastModifiedDate = DateTime.UtcNow;
                }
            });

        // Setup DeleteAsync
        mockRepository.Setup(r => r.DeleteAsync(It.IsAny<Team>()))
            .Callback((Team team) =>
            {
                var existingTeam = teams.FirstOrDefault(t => t.Id == team.Id);
                if (existingTeam != null)
                {
                    teams.Remove(existingTeam);
                }
            });

        return mockRepository;
    }

    public static Mock<ITeamRepository> GetMockTeamRepositoryEmpty()
    {
        var mockRepository = new Mock<ITeamRepository>();
        var emptyTeams = new List<Team>();

        mockRepository.Setup(r => r.GetAsync()).ReturnsAsync(emptyTeams);
        mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Team?)null);
        mockRepository.Setup(r => r.GetByNameAsync(It.IsAny<string>())).ReturnsAsync((Team?)null);
        mockRepository.Setup(r => r.IsAnyTeamAvailableByNameAsync(It.IsAny<string>())).ReturnsAsync(false);
        mockRepository.Setup(r => r.GetWithMembersAsync(It.IsAny<int>())).ReturnsAsync((Team?)null);
        mockRepository.Setup(r => r.GetWithTasksAsync(It.IsAny<int>())).ReturnsAsync((Team?)null);
        mockRepository.Setup(r => r.GetWithMembersAndTasksAsync(It.IsAny<int>())).ReturnsAsync((Team?)null);

        return mockRepository;
    }

    public static Mock<ITeamRepository> GetMockTeamRepositoryWithTasks()
    {
        var mockRepository = GetMockTeamRepository();

        // Override GetWithTasksAsync to return teams with tasks
        mockRepository.Setup(r => r.GetWithTasksAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) =>
            {
                var team = new Team
                {
                    Id = id,
                    Name = $"Team {id}",
                    Description = $"Description {id}",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Members = new List<TeamMember>(),
                    Tasks = new List<Entities.TaskItem>
                    {
                        new TaskItem
                        {
                            Id = 1,
                            Title = $"Task for team {id}",
                            Status = Entities.Enums.Status.Todo,
                            DueDate = DateTime.UtcNow.AddDays(7),
                            AssignedToUserId = 1,
                            CreatedByUserId = 1,
                            TeamId = id,
                            CreatedDate = DateTime.UtcNow
                        }
                    }
                };
                return team;
            });

        return mockRepository;
    }
}

using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Exceptions;
using HR.ProjectManagement.Services.Interfaces;
using HR.ProjectManagement.Validations.TeamValidations;
using Mapster;
using FluentValidation;

namespace HR.ProjectManagement.Services;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateTeamRequest> _createTeamValidator;
    private readonly IValidator<UpdateTeamRequest> _updateTeamValidator;

    public TeamService(
        ITeamRepository teamRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateTeamRequest> createTeamValidator,
        IValidator<UpdateTeamRequest> updateTeamValidator)
    {
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _createTeamValidator = createTeamValidator;
        _updateTeamValidator = updateTeamValidator;
    }

    public async Task<TeamResponse> CreateAsync(CreateTeamRequest request)
    {
        // Validate input
        var validationResult = await _createTeamValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new BadRequestException("Validation failed", validationResult);

        var team = new Team
        {
            Name = request.Name,
            Description = request.Description
        };

        await _teamRepository.CreateAsync(team);
        await _unitOfWork.SaveChangesAsync();

        return team.Adapt<TeamResponse>();
    }

    public async Task<TeamResponse> UpdateAsync(int id, UpdateTeamRequest request)
    {
        // Validate input
        var validationResult = await _updateTeamValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new BadRequestException("Validation failed", validationResult);

        var existingTeam = await _teamRepository.GetByIdAsync(id);
        if (existingTeam == null)
            throw new NotFoundException("Team", id);

        // Check if name is already used by another team
        if (await _teamRepository.GetByNameAsync(request.Name) != null && existingTeam.Name != request.Name)
        {
            throw new BadRequestException($"Team name '{request.Name}' is already in use by another team");
        }

        // Update properties
        existingTeam.Name = request.Name;
        existingTeam.Description = request.Description;

        await _teamRepository.UpdateAsync(existingTeam);
        await _unitOfWork.SaveChangesAsync();

        return existingTeam.Adapt<TeamResponse>();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null)
            throw new NotFoundException("Team", id);

        // Check if team has tasks (business rule: cannot delete team with active tasks)
        var teamWithTasks = await _teamRepository.GetWithTasksAsync(id);
        if (teamWithTasks?.Tasks.Any() == true)
        {
            throw new BadRequestException("Cannot delete team with existing tasks. Please reassign or delete tasks first.");
        }

        await _teamRepository.DeleteAsync(team);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<TeamResponse?> GetByIdAsync(int id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        return team?.Adapt<TeamResponse>();
    }

    public async Task<IReadOnlyList<TeamResponse>> GetAllAsync()
    {
        var teams = await _teamRepository.GetAsync();
        return teams.Adapt<IReadOnlyList<TeamResponse>>();
    }

    public async Task<TeamWithMembersResponse?> GetWithMembersAsync(int id)
    {
        var team = await _teamRepository.GetWithMembersAsync(id);
        if (team == null)
            return null;

        var response = new TeamWithMembersResponse
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            Members = team.Members.Select(tm => new UserSummary
            {
                Id = tm.User.Id,
                FullName = tm.User.FullName,
                Email = tm.User.Email,
                Role = tm.User.Role
            }).ToList()
        };

        return response;
    }

    public async Task<TeamWithTasksResponse?> GetWithTasksAsync(int id)
    {
        var team = await _teamRepository.GetWithTasksAsync(id);
        if (team == null)
            return null;

        var response = new TeamWithTasksResponse
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            Tasks = team.Tasks.Select(task => new TaskSummary
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status,
                DueDate = task.DueDate,
                AssignedToUserName = task.AssignedToUser?.FullName ?? "Unassigned"
            }).ToList()
        };

        return response;
    }
}
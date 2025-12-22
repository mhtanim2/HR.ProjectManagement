using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities;
using HR.ProjectManagement.Entities.Enums;
using HR.ProjectManagement.Exceptions;
using HR.ProjectManagement.Services.Interfaces;
using HR.ProjectManagement.Validations.TaskValidations;
using Mapster;
using FluentValidation;

namespace HR.ProjectManagement.Services;

public class TaskItemService : ITaskItemService
{
    private readonly ITaskListRepository _taskListRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateTaskRequest> _createTaskValidator;
    private readonly IValidator<UpdateTaskRequest> _updateTaskValidator;

    public TaskItemService(
        ITaskListRepository taskListRepository,
        IUserRepository userRepository,
        ITeamRepository teamRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateTaskRequest> createTaskValidator,
        IValidator<UpdateTaskRequest> updateTaskValidator)
    {
        _taskListRepository = taskListRepository;
        _userRepository = userRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _createTaskValidator = createTaskValidator;
        _updateTaskValidator = updateTaskValidator;
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request)
    {
        // Validate input
        var validationResult = await _createTaskValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new BadRequestException("Validation failed", validationResult);

        // Validate assigned user exists
        var assignedUser = await _userRepository.GetByIdAsync(request.AssignedToUserId);
        if (assignedUser == null)
            throw new BadRequestException($"Assigned user with ID {request.AssignedToUserId} does not exist");

        // Validate team exists
        var team = await _teamRepository.GetByIdAsync(request.TeamId);
        if (team == null)
            throw new BadRequestException($"Team with ID {request.TeamId} does not exist");

        // Business validation: assigned user must be member of the team
        var teamWithMembers = await _teamRepository.GetWithMembersAsync(request.TeamId);
        if (teamWithMembers?.Members.All(m => m.UserId != request.AssignedToUserId) == true)
        {
            throw new BadRequestException($"User {assignedUser.FullName} is not a member of team {team.Name}");
        }

        // Validate due date is in the future
        if (request.DueDate <= DateTime.UtcNow)
        {
            throw new BadRequestException("Due date must be in the future");
        }

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Status = Status.Todo, // Default status for new tasks
            DueDate = request.DueDate,
            AssignedToUserId = request.AssignedToUserId,
            CreatedByUserId = request.CreatedByUserId, // This should come from current user context
            TeamId = request.TeamId
        };

        await _taskListRepository.CreateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        return task.Adapt<TaskResponse>();
    }

    public async Task<TaskResponse> UpdateAsync(int id, UpdateTaskRequest request)
    {
        // Validate input
        var validationResult = await _updateTaskValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new BadRequestException("Validation failed", validationResult);

        var existingTask = await _taskListRepository.GetByIdAsync(id);
        if (existingTask == null)
            throw new NotFoundException("Task", id);

        // Validate assigned user if provided
        if (request.AssignedToUserId.HasValue)
        {
            var assignedUser = await _userRepository.GetByIdAsync(request.AssignedToUserId.Value);
            if (assignedUser == null)
                throw new BadRequestException($"Assigned user with ID {request.AssignedToUserId.Value} does not exist");
        }

        // Validate team if provided
        if (request.TeamId.HasValue)
        {
            var team = await _teamRepository.GetByIdAsync(request.TeamId.Value);
            if (team == null)
                throw new BadRequestException($"Team with ID {request.TeamId.Value} does not exist");

            // If assigned user and team are both provided, validate team membership
            if (request.AssignedToUserId.HasValue)
            {
                var teamWithMembers = await _teamRepository.GetWithMembersAsync(request.TeamId.Value);
                if (teamWithMembers?.Members.All(m => m.UserId != request.AssignedToUserId.Value) == true)
                {
                    throw new BadRequestException($"Assigned user is not a member of the specified team");
                }
            }
        }

        // Validate due date is in the future
        if (request.DueDate <= DateTime.UtcNow)
        {
            throw new BadRequestException("Due date must be in the future");
        }

        // Update properties
        existingTask.Title = request.Title;
        existingTask.Description = request.Description;
        existingTask.Status = request.Status;
        existingTask.DueDate = request.DueDate;

        if (request.AssignedToUserId.HasValue)
            existingTask.AssignedToUserId = request.AssignedToUserId.Value;

        if (request.TeamId.HasValue)
            existingTask.TeamId = request.TeamId.Value;

        await _taskListRepository.UpdateAsync(existingTask);
        await _unitOfWork.SaveChangesAsync();

        return existingTask.Adapt<TaskResponse>();
    }

    public async Task<TaskResponse> UpdateStatusAsync(int id, UpdateTaskStatusRequest request)
    {
        var task = await _taskListRepository.GetByIdAsync(id);
        if (task == null)
            throw new NotFoundException("Task", id);

        // Validate status transition (business rule)
        if (task.Status == Status.Done && request.Status != Status.Done)
        {
            throw new BadRequestException("Cannot change status from Done to other statuses");
        }

        task.Status = request.Status;

        await _taskListRepository.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        return task.Adapt<TaskResponse>();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _taskListRepository.GetByIdAsync(id);
        if (task == null)
            throw new NotFoundException("Task", id);

        await _taskListRepository.DeleteAsync(task);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<TaskResponse?> GetByIdAsync(int id)
    {
        var task = await _taskListRepository.GetByIdAsync(id);
        return task?.Adapt<TaskResponse>();
    }

    public async Task<TaskWithDetailsResponse?> GetWithDetailsAsync(int id)
    {
        var task = await _taskListRepository.GetWithDetailsAsync(id);
        if (task == null)
            return null;

        var response = new TaskWithDetailsResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            DueDate = task.DueDate,
            AssignedToUser = new UserSummary
            {
                Id = task.AssignedToUser.Id,
                FullName = task.AssignedToUser.FullName,
                Email = task.AssignedToUser.Email,
                Role = task.AssignedToUser.Role
            },
            CreatedByUser = new UserSummary
            {
                Id = task.CreatedByUser.Id,
                FullName = task.CreatedByUser.FullName,
                Email = task.CreatedByUser.Email,
                Role = task.CreatedByUser.Role
            },
            Team = new TeamSummary
            {
                Id = task.Team.Id,
                Name = task.Team.Name,
                Description = task.Team.Description
            }
        };

        return response;
    }

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync()
    {
        var tasks = await _taskListRepository.GetAsync();
        return tasks.Adapt<IReadOnlyList<TaskResponse>>();
    }

    public async Task<IReadOnlyList<TaskResponse>> GetMyTasksAsync(int userId)
    {
        var tasks = await _taskListRepository.GetTasksForUserAsync(userId);
        return tasks.Adapt<IReadOnlyList<TaskResponse>>();
    }

    public async Task<IReadOnlyList<TaskResponse>> GetTasksByUserAsync(int userId)
    {
        var tasks = await _taskListRepository.GetByAssignedUserAsync(userId);
        return tasks.Adapt<IReadOnlyList<TaskResponse>>();
    }

    public async Task<IReadOnlyList<TaskResponse>> GetTasksByTeamAsync(int teamId)
    {
        var tasks = await _taskListRepository.GetByTeamAsync(teamId);
        return tasks.Adapt<IReadOnlyList<TaskResponse>>();
    }

    public async Task<IReadOnlyList<TaskResponse>> GetTasksByStatusAsync(Status status)
    {
        var tasks = await _taskListRepository.GetByStatusAsync(status);
        return tasks.Adapt<IReadOnlyList<TaskResponse>>();
    }
}
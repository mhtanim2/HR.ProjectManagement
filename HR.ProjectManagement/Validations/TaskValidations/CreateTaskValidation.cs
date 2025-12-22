using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities.Enums;
using FluentValidation;

namespace HR.ProjectManagement.Validations.TaskValidations;

public class CreateTaskValidation : AbstractValidator<CreateTaskRequest>
{
    private readonly IUserRepository _userRepository;
    private readonly ITeamRepository _teamRepository;

    public CreateTaskValidation(IUserRepository userRepository, ITeamRepository teamRepository)
    {
        _userRepository = userRepository;
        _teamRepository = teamRepository;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.AssignedToUserId)
            .GreaterThan(0).WithMessage("Assigned user ID is required")
            .MustAsync(async (userId, cancellation) => await _userRepository.GetByIdAsync(userId) != null)
            .WithMessage("Assigned user does not exist");

        RuleFor(x => x.TeamId)
            .GreaterThan(0).WithMessage("Team ID is required")
            .MustAsync(async (teamId, cancellation) => await _teamRepository.GetByIdAsync(teamId) != null)
            .WithMessage("Team does not exist");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future")
            .LessThan(DateTime.UtcNow.AddYears(1)).WithMessage("Due date cannot be more than 1 year in the future");

        RuleFor(x => x.CreatedByUserId)
            .GreaterThan(0).WithMessage("Created by user ID is required");

        RuleFor(x => x)
            .MustAsync(async (request, cancellation) =>
            {
                var teamWithMembers = await _teamRepository.GetWithMembersAsync(request.TeamId);
                return teamWithMembers?.Members.Any(m => m.UserId == request.AssignedToUserId) == true;
            })
            .WithMessage("Assigned user must be a member of the team");
    }
}
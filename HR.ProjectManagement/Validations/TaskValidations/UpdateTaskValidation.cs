using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities.Enums;
using FluentValidation;

namespace HR.ProjectManagement.Validations.TaskValidations;

public class UpdateTaskValidation : AbstractValidator<UpdateTaskRequest>
{
    private readonly IUserRepository _userRepository;
    private readonly ITeamRepository _teamRepository;

    public UpdateTaskValidation(IUserRepository userRepository, ITeamRepository teamRepository)
    {
        _userRepository = userRepository;
        _teamRepository = teamRepository;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        When(x => x.AssignedToUserId.HasValue, () =>
        {
            RuleFor(x => x.AssignedToUserId.Value)
                .GreaterThan(0).WithMessage("Assigned user ID must be greater than 0")
                .MustAsync(async (userId, cancellation) => await _userRepository.GetByIdAsync(userId) != null)
                .WithMessage("Assigned user does not exist");
        });

        When(x => x.TeamId.HasValue, () =>
        {
            RuleFor(x => x.TeamId.Value)
                .GreaterThan(0).WithMessage("Team ID must be greater than 0")
                .MustAsync(async (teamId, cancellation) => await _teamRepository.GetByIdAsync(teamId) != null)
                .WithMessage("Team does not exist");
        });

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future")
            .LessThan(DateTime.UtcNow.AddYears(1)).WithMessage("Due date cannot be more than 1 year in the future");

        // Business validation: If both assigned user and team are provided, user must be team member
        RuleFor(x => x)
            .MustAsync(async (request, cancellation) =>
            {
                if (!request.AssignedToUserId.HasValue || !request.TeamId.HasValue)
                    return true; // Skip validation if either is not provided

                var teamWithMembers = await _teamRepository.GetWithMembersAsync(request.TeamId.Value);
                return teamWithMembers?.Members.Any(m => m.UserId == request.AssignedToUserId.Value) == true;
            })
            .WithMessage("Assigned user must be a member of the team when both are specified");
    }
}
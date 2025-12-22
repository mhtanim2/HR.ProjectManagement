using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DTOs;
using FluentValidation;

namespace HR.ProjectManagement.Validations.TeamValidations;

public class UpdateTeamValidation : AbstractValidator<UpdateTeamRequest>
{
    private readonly ITeamRepository _teamRepository;

    public UpdateTeamValidation(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required")
            .MaximumLength(100).WithMessage("Team name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
    }
}
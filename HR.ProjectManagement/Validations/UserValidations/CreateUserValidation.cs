using FluentValidation;
using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DTOs;

namespace HR.ProjectManagement.Validations.UserValidations;

public class CreateUserValidation : AbstractValidator<CreateUserRequest>
{
    private readonly IUserRepository _userRepository;

    public CreateUserValidation(IUserRepository userRepository)
    {
        _userRepository = userRepository;

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters")
            .MustAsync(async (email, cancellation) => !await _userRepository.IsAnyUserAvailableByEmail(email))
            .WithMessage("Email is already in use");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long");
    }
}

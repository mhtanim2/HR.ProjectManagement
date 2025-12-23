using FluentValidation;
using HR.ProjectManagement.DTOs;

namespace HR.ProjectManagement.Validations.AuthValidations;

public class LoginRequestValidation:AbstractValidator<LoginRequest>
{
    public LoginRequestValidation()
    {
        RuleFor(l=>l.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
        RuleFor(l=>l.Password)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]").WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");
    }
}

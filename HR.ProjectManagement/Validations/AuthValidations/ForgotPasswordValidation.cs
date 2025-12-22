using FluentValidation;
using HR.ProjectManagement.DTOs;

namespace HR.ProjectManagement.Validations.AuthValidations;

public class ForgotPasswordValidation : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
    }
}
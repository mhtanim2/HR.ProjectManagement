using FluentValidation;
using HR.ProjectManagement.DTOs;

namespace HR.ProjectManagement.Validations.AuthValidations;

public class RefreshTokenValidation : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidation()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required")
            .MaximumLength(500).WithMessage("Refresh token cannot exceed 500 characters");

        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required")
            .MaximumLength(1000).WithMessage("Access token cannot exceed 1000 characters");
    }
}
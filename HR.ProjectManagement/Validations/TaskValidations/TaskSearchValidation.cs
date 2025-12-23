using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Entities.Enums;
using FluentValidation;

namespace HR.ProjectManagement.Validations.TaskValidations;

public class TaskSearchValidation : AbstractValidator<TaskSearchRequest>
{
    public TaskSearchValidation()
    {
        // PageNumber validation
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0");

        // PageSize validation
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100");

        // Status validation (if provided)
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value")
            .When(x => x.Status.HasValue);

        // Date range validation
        RuleFor(x => x.DueDateFrom)
            .LessThan(x => x.DueDateTo)
            .WithMessage("DueDateFrom must be less than DueDateTo")
            .When(x => x.DueDateFrom.HasValue && x.DueDateTo.HasValue);

        // SearchTerm length validation
        RuleFor(x => x.SearchTerm)
            .MaximumLength(200).WithMessage("Search term cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        // SortBy validation
        RuleFor(x => x.SortBy)
            .Must(BeAValidSortField)
            .WithMessage("Invalid sort field. Valid fields: title, status, duedate, createddate, assignedto, team")
            .When(x => !string.IsNullOrEmpty(x.SortBy));
    }

    private bool BeAValidSortField(string? sortBy)
    {
        if (string.IsNullOrEmpty(sortBy)) return true;

        var validFields = new[] { "title", "status", "duedate", "createddate", "assignedto", "team" };
        return validFields.Contains(sortBy.ToLower());
    }
}

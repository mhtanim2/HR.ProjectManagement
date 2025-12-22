using FluentValidation.Results;

namespace HR.ProjectManagement.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, ValidationResult validationResult) : base(message)
    {
        ValidationErrors = validationResult.ToDictionary();
    }

    public IDictionary<string, string[]> ValidationErrors { get; set; }
}
using FluentValidation;
using Moq;

namespace HR.ProjectManagement.UnitTests.Helpers;

public static class ValidatorTestHelper
{
    public static void SetupValidatorToPass<T>(Mock<IValidator<T>> validator) where T : class
    {
        validator.Setup(v => v.ValidateAsync(It.IsAny<T>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    public static void SetupValidatorToFail<T>(
        Mock<IValidator<T>> validator,
        string errorMessage,
        string propertyName = "") where T : class
    {
        var failure = new FluentValidation.Results.ValidationFailure(propertyName, errorMessage);
        var result = new FluentValidation.Results.ValidationResult();
        result.Errors.Add(failure);

        validator.Setup(v => v.ValidateAsync(It.IsAny<T>(), default))
            .ReturnsAsync(result);
    }
}

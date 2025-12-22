namespace HR.ProjectManagement.Contracts.Messaging;

public record Results(bool IsSuccess, string Error = "")
{
    public static Results Success() => new(true);
    public static Results Failure(string error) => new(false, error);
}

public record Results<TValue>(TValue? Value, bool IsSuccess, string Error = "")
    : Results(IsSuccess, Error)
{
    public static Results<TValue> Success(TValue value) => new(value, true);
    public new static Results<TValue> Failure(string error) => new(default, false, error);
    //public static implicit operator Results<TValue>(TValue value) => Success(value);
}
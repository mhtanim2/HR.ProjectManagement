namespace HR.ProjectManagement.Exceptions;

public class TokenExpiredException : Exception
{
    public TokenExpiredException(string message) : base(message)
    {
    }

    public TokenExpiredException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
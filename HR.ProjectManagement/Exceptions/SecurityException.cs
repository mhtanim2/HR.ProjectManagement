namespace HR.ProjectManagement.Exceptions;

public class SecurityException:Exception
{
    public SecurityException()
    {
    }
    public SecurityException(string message)
    : base(message)
    {
    }

}

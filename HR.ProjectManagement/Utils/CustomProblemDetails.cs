using Microsoft.AspNetCore.Mvc;

namespace HR.ProjectManagement.Utils;

public class CustomProblemDetails : ProblemDetails
{
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
}
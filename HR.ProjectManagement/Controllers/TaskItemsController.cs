using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HR.ProjectManagement.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TaskItemsController : ControllerBase
{
    private readonly ITaskItemService _taskItemService;

    public TaskItemsController(ITaskItemService taskItemService)
    {
        _taskItemService = taskItemService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<ActionResult<TaskResponse>> Create([FromBody] CreateTaskRequest request)
    {
        // Set the CreatedByUserId from current user
        request.CreatedByUserId = GetCurrentUserId();
        var task = await _taskItemService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAll()
    {
        var tasks = await _taskItemService.GetAllAsync();
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<TaskResponse>> GetById(int id)
    {
        var task = await _taskItemService.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        return Ok(task);
    }

    [HttpGet("{id:int}/details")]
    [Authorize]
    public async Task<ActionResult<TaskWithDetailsResponse>> GetWithDetails(int id)
    {
        var task = await _taskItemService.GetWithDetailsAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        return Ok(task);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<ActionResult<TaskResponse>> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        var task = await _taskItemService.UpdateAsync(id, request);
        return Ok(task);
    }

    [HttpPut("{id:int}/status")]
    [Authorize]
    public async Task<ActionResult<TaskResponse>> UpdateStatus(int id, [FromBody] UpdateTaskStatusRequest request)
    {
        // Get current user to check if they can update the status
        var currentUserId = GetCurrentUserId();
        var task = await _taskItemService.GetByIdAsync(id);

        if (task == null)
        {
            return NotFound();
        }

        // Check if user is assigned to this task or is Manager/Admin
        if (task.AssignedToUserId != currentUserId && !User.IsInRole("Manager") && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var updatedTask = await _taskItemService.UpdateStatusAsync(id, request);
        return Ok(updatedTask);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<ActionResult> Delete(int id)
    {
        await _taskItemService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("my-tasks")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetMyTasks()
    {
        var currentUserId = GetCurrentUserId();
        var tasks = await _taskItemService.GetMyTasksAsync(currentUserId);
        return Ok(tasks);
    }

    [HttpGet("user/{userId:int}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetTasksByUser(int userId)
    {
        var tasks = await _taskItemService.GetTasksByUserAsync(userId);
        return Ok(tasks);
    }

    [HttpGet("team/{teamId:int}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetTasksByTeam(int teamId)
    {
        var tasks = await _taskItemService.GetTasksByTeamAsync(teamId);
        return Ok(tasks);
    }
}

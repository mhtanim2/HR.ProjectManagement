using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Services.Interfaces;
using HR.ProjectManagement.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.ProjectManagement.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TaskItemsController : ControllerBase
{
    private readonly ITaskItemService _taskItemService;
    private readonly ICurrentUserService _currentUser;

    public TaskItemsController(ITaskItemService taskItemService,
        ICurrentUserService currentUser)
    {
        _taskItemService = taskItemService;
        _currentUser = currentUser;
    }

    
    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<ActionResult<TaskResponse>> Create([FromBody] CreateTaskRequest request)
    {
        var currentUserId = _currentUser.UserId;
        var task = await _taskItemService.CreateAsync(request, currentUserId);
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
            return NotFound();
        return Ok(task);
    }

    [HttpGet("{id:int}/details")]
    [Authorize]
    public async Task<ActionResult<TaskWithDetailsResponse>> GetWithDetails(int id)
    {
        var task = await _taskItemService.GetWithDetailsAsync(id);
        if (task == null)
            return NotFound();
        return Ok(task);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = SD.ManagerOrAdmin)]
    public async Task<ActionResult<TaskResponse>> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        var task = await _taskItemService.UpdateAsync(id, request);
        return Ok(task);
    }

    [HttpPut("{id:int}/status")]
    [Authorize]
    public async Task<ActionResult<TaskResponse>> UpdateStatus(int id, [FromBody] UpdateTaskStatusRequest request)
    {
        var currentUserId = _currentUser.UserId;
        var task = await _taskItemService.GetByIdAsync(id);

        if (task == null)
            return NotFound();

        if (task.AssignedToUserId != currentUserId && !User.IsInRole("Manager") && !User.IsInRole("Admin"))
            return Forbid();

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
        var currentUserId = _currentUser.UserId;
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

    [HttpGet("search")]
    [Authorize]
    public async Task<ActionResult<PagedResponse<TaskSearchResponse>>> SearchTasks([FromQuery] TaskSearchRequest request)
    {
        var result = await _taskItemService.SearchTasksAsync(request);
        return Ok(result);
    }
}

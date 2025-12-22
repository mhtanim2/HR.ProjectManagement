using HR.ProjectManagement.DTOs;
using HR.ProjectManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.ProjectManagement.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<TeamResponse>> Create([FromBody] CreateTeamRequest request)
    {
        var team = await _teamService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = team.Id }, team);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<TeamResponse>>> GetAll()
    {
        var teams = await _teamService.GetAllAsync();
        return Ok(teams);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<TeamResponse>> GetById(int id)
    {
        var team = await _teamService.GetByIdAsync(id);
        if (team == null)
        {
            return NotFound();
        }
        return Ok(team);
    }

    [HttpGet("{id:int}/members")]
    [Authorize]
    public async Task<ActionResult<TeamWithMembersResponse>> GetWithMembers(int id)
    {
        var team = await _teamService.GetWithMembersAsync(id);
        if (team == null)
        {
            return NotFound();
        }
        return Ok(team);
    }

    [HttpGet("{id:int}/tasks")]
    [Authorize]
    public async Task<ActionResult<TeamWithTasksResponse>> GetWithTasks(int id)
    {
        var team = await _teamService.GetWithTasksAsync(id);
        if (team == null)
        {
            return NotFound();
        }
        return Ok(team);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<TeamResponse>> Update(int id, [FromBody] UpdateTeamRequest request)
    {
        var team = await _teamService.UpdateAsync(id, request);
        return Ok(team);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> Delete(int id)
    {
        await _teamService.DeleteAsync(id);
        return NoContent();
    }
}

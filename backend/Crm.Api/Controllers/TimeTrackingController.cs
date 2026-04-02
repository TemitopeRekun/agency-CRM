using Crm.Application.DTOs.TimeTracking;
using Crm.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TimeTrackingController : ControllerBase
{
    private readonly TimeTrackingService _timeTrackingService;

    public TimeTrackingController(TimeTrackingService timeTrackingService)
    {
        _timeTrackingService = timeTrackingService;
    }

    [HttpPost]
    public async Task<ActionResult<TimeEntryDto>> LogTime(CreateTimeEntryRequest request)
    {
        var response = await _timeTrackingService.LogTimeAsync(request);
        return Ok(response);
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<TimeEntryDto>>> GetProjectTimeEntries(Guid projectId)
    {
        var entries = await _timeTrackingService.GetProjectTimeEntriesAsync(projectId);
        return Ok(entries);
    }

    [HttpGet("project/{projectId}/team")]
    public async Task<ActionResult<ProjectTeamResponse>> GetProjectTeam(Guid projectId)
    {
        var team = await _timeTrackingService.GetProjectTeamAsync(projectId);
        return Ok(team);
    }

    [HttpPost("project/{projectId}/team")]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<ActionResult> AddTeamMember(Guid projectId, AddTeamMemberRequest request)
    {
        await _timeTrackingService.AddTeamMemberAsync(projectId, request);
        return Ok();
    }
}

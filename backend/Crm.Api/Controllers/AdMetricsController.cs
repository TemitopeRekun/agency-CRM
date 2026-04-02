using Crm.Application.DTOs.AdMetrics;
using Crm.Application.Interfaces;
using Crm.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AdMetricsController : ControllerBase
{
    private readonly IAdMetricService _adMetricService;

    public AdMetricsController(IAdMetricService adMetricService)
    {
        _adMetricService = adMetricService;
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<AdMetricResponse>>> GetProjectMetrics(Guid projectId)
    {
        var metrics = await _adMetricService.GetProjectMetricsAsync(projectId);
        return Ok(metrics);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdMetricResponse>>> GetAllMetrics()
    {
        var metrics = await _adMetricService.GetAllAsync();
        return Ok(metrics);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<ActionResult<AdMetricResponse>> CreateMetric(CreateAdMetricRequest request)
    {
        var response = await _adMetricService.CreateAsync(request);
        return Ok(response);
    }

    [HttpGet("project/{projectId}/analytics")]
    public async Task<ActionResult<AdMetricAnalyticsResponse>> GetProjectAnalytics(Guid projectId)
    {
        var analytics = await _adMetricService.GetProjectAnalyticsAsync(projectId);
        return Ok(analytics);
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<AdMetricAnalyticsResponse>> GetGlobalAnalytics()
    {
        var analytics = await _adMetricService.GetGlobalAnalyticsAsync();
        return Ok(analytics);
    }
}

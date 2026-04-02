using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Crm.Application.Interfaces;

namespace Crm.Api.Controllers;

[Authorize] // Require Auth for manual triggers
[ApiController]
[Route("api/automation")]
public class AutomationController : ControllerBase
{
    private readonly IAutomationService _automationService;

    public AutomationController(IAutomationService automationService)
    {
        _automationService = automationService;
    }

    [HttpPost("run-monthly-billing")]
    public async Task<IActionResult> RunMonthlyBilling()
    {
        await _automationService.GenerateMonthlyInvoicesAsync();
        return Ok(new { message = "Monthly Billing Job triggered manually" });
    }

    [HttpPost("sync-ad-metrics")]
    public async Task<IActionResult> SyncAdMetrics([FromQuery] Guid projectId)
    {
        var adMetricService = HttpContext.RequestServices.GetRequiredService<IAdMetricService>();
        await adMetricService.SyncMetricsAsync(projectId);
        return Ok(new { message = $"Ad metrics sync triggered for project {projectId}" });
    }
}

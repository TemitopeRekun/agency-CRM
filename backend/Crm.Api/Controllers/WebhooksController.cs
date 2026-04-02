using Crm.Application.DTOs.AdMetrics;
using Crm.Application.DTOs.Leads;
using Crm.Application.Services;
using Crm.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

public class MetaWebhookRequest
{
    public string? lead_id { get; set; }
    public string? form_id { get; set; }
    public Dictionary<string, string>? field_data { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly LeadService _leadService;
    private readonly AdMetricService _adMetricService;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        LeadService leadService, 
        AdMetricService adMetricService,
        ILogger<WebhooksController> logger)
    {
        _leadService = leadService;
        _adMetricService = adMetricService;
        _logger = logger;
    }

    [HttpPost("meta/lead")]
    public async Task<IActionResult> MetaLeadWebhook([FromBody] MetaWebhookRequest payload)
    {
        _logger.LogInformation("Received Meta Lead Webhook: {Payload}", payload);
        
        // Mock extraction logic for Meta Lead Ads payload
        var request = new CreateLeadRequest
        {
            Title = $"Meta Lead: {payload?.lead_id ?? "Unknown"}",
            Description = $"Ingested from Meta at {DateTime.UtcNow}. Full payload: {payload}"
        };

        await _leadService.CreateAsync(request);
        return Ok(new { status = "success" });
    }

    [HttpPost("google/performance")]
    public async Task<IActionResult> GooglePerformanceWebhook([FromBody] CreateAdMetricRequest request)
    {
        _logger.LogInformation("Received Google Performance Webhook for Project: {ProjectId}", request.ProjectId);
        
        await _adMetricService.CreateAsync(request);
        return Ok(new { status = "success" });
    }

    [HttpPost("tiktok/performance")]
    public async Task<IActionResult> TikTokPerformanceWebhook([FromBody] CreateAdMetricRequest request)
    {
        _logger.LogInformation("Received TikTok Performance Webhook for Project: {ProjectId}", request.ProjectId);
        
        await _adMetricService.CreateAsync(request);
        return Ok(new { status = "success" });
    }
}

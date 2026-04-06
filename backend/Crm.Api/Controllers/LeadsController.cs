using Crm.Application.DTOs.Leads;
using Crm.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize(Roles = "Admin,SalesManager")]
[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly LeadService _leadService;
    private readonly ILogger<LeadsController> _logger;

    public LeadsController(LeadService leadService, ILogger<LeadsController> logger)
    {
        _leadService = leadService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeadResponse>>> GetLeads()
    {
        var leads = await _leadService.GetAllAsync();
        return Ok(leads);
    }

    [HttpPost]
    public async Task<ActionResult<LeadResponse>> CreateLead(CreateLeadRequest request)
    {
        _logger.LogInformation("Creating new lead: {Title}", request.Title);
        var response = await _leadService.CreateAsync(request);

        _logger.LogInformation("Lead created successfully with ID: {Id}", response.Id);
        return CreatedAtAction(nameof(GetLeads), new { id = response.Id }, response);
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<LeadResponse>> UpdateStatus(Guid id, UpdateLeadStatusRequest request)
    {
        _logger.LogInformation("Updating status for lead: {Id}", id);
        var response = await _leadService.UpdateStatusAsync(id, request);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<LeadResponse>> UpdateLead(Guid id, UpdateLeadRequest request)
    {
        _logger.LogInformation("Updating lead: {Id}", id);
        var response = await _leadService.UpdateAsync(id, request);
        if (response == null) return NotFound();
        return Ok(response);
    }
}

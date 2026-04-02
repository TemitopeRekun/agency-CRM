using Crm.Application.Interfaces;
using Crm.Application.DTOs.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[ApiController]
[Route("api/portal/contracts")]
public class ContractPortalController : ControllerBase
{
    private readonly IContractPortalService _portalService;

    public ContractPortalController(IContractPortalService portalService)
    {
        _portalService = portalService;
    }

    [AllowAnonymous]
    [HttpGet("{token}")]
    public async Task<IActionResult> GetContract(Guid token)
    {
        var contract = await _portalService.GetContractByTokenAsync(token);
        if (contract == null) return NotFound();
        return Ok(contract);
    }

    [AllowAnonymous]
    [HttpPost("{token}/sign")]
    public async Task<IActionResult> SignContract(Guid token, [FromBody] SignContractRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var contract = await _portalService.SignContractAsync(token, request.DigitalSignature, ipAddress);
        if (contract == null) return NotFound();
        return Ok(contract);
    }

    [AllowAnonymous]
    [HttpPost("{token}/view")]
    public async Task<IActionResult> MarkAsViewed(Guid token)
    {
        var success = await _portalService.MarkViewedAsync(token);
        if (!success) return NotFound();
        return Ok(new { success = true });
    }
}

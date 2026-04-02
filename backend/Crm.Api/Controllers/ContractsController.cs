using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Crm.Application.Services;
using Crm.Application.DTOs.Contracts;

namespace Crm.Api.Controllers;

[Authorize(Roles = "Admin,SalesManager,ProjectManager")]
[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly ContractService _contractService;

    public ContractsController(ContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContractResponse>>> GetContracts()
    {
        var contracts = await _contractService.GetAllAsync();
        return Ok(contracts);
    }

    [HttpPost]
    public async Task<ActionResult<ContractResponse>> CreateContract(CreateContractRequest request)
    {
        var response = await _contractService.CreateAsync(request);
        return CreatedAtAction(nameof(GetContracts), new { id = response.Id }, response);
    }

    [HttpPost("generate/{projectId}")]
    public async Task<ActionResult<ContractResponse>> GenerateFromProject(Guid projectId)
    {
        var response = await _contractService.GenerateFromProjectAsync(projectId);
        return Ok(response);
    }

    [HttpPost("{id}/sign")]
    public async Task<ActionResult<ContractResponse>> SignContract(Guid id, [FromBody] SignContractRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DigitalSignature))
        {
            return BadRequest("A digital signature is required.");
        }

        var response = await _contractService.SignContractAsync(id, request.DigitalSignature);
        if (response == null) return NotFound();

        return Ok(response);
    }
}

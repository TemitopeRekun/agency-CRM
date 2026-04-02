using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;

namespace Crm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId}/adaccounts")]
public class ProjectAdAccountsController : ControllerBase
{
    private readonly IGenericRepository<ProjectAdAccount> _repository;
    private readonly IAdMetricService _adMetricService;

    public ProjectAdAccountsController(
        IGenericRepository<ProjectAdAccount> repository, 
        IAdMetricService adMetricService)
    {
        _repository = repository;
        _adMetricService = adMetricService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectAdAccount>>> GetAll(Guid projectId)
    {
        var accounts = (await _repository.GetAllAsync())
            .Where(a => a.ProjectId == projectId);
        return Ok(accounts);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectAdAccount>> Create(Guid projectId, ProjectAdAccount account)
    {
        account.ProjectId = projectId;
        account.Id = Guid.NewGuid();
        
        await _repository.AddAsync(account);
        await _repository.SaveChangesAsync();
        
        // Initial sync
        await _adMetricService.SyncMetricsAsync(projectId);
        
        return Ok(account);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var account = await _repository.GetByIdAsync(id);
        if (account == null) return NotFound();

        _repository.Delete(account);
        await _repository.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync(Guid projectId)
    {
        await _adMetricService.SyncMetricsAsync(projectId);
        return Ok(new { message = "Sync triggered successfully" });
    }
}

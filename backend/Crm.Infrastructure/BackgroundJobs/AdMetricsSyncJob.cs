using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;

namespace Crm.Infrastructure.BackgroundJobs;

public class AdMetricsSyncJob
{
    private readonly ILogger<AdMetricsSyncJob> _logger;
    private readonly IAdMetricService _adMetricService;
    private readonly IGenericRepository<Project> _projectRepository;

    public AdMetricsSyncJob(
        ILogger<AdMetricsSyncJob> logger, 
        IAdMetricService adMetricService,
        IGenericRepository<Project> projectRepository)
    {
        _logger = logger;
        _adMetricService = adMetricService;
        _projectRepository = projectRepository;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting Nightly Ad Metrics Sync Job at {Time}", DateTimeOffset.Now);
        
        // Explicitly ignore tenant filters as this job must process all projects across all tenants
        var projects = await _projectRepository.AsQueryable()
            .IgnoreQueryFilters()
            .ToListAsync();
        int syncCount = 0;

        foreach (var project in projects.Where(p => p.Status == ProjectStatus.Active))
        {
            try 
            {
                await _adMetricService.SyncMetricsAsync(project.Id);
                syncCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync metrics for Project {ProjectId}: {Message}", project.Id, ex.Message);
            }
        }

        _logger.LogInformation("Ad Metrics Sync Job completed. Synced {Count} projects at {Time}", syncCount, DateTimeOffset.Now);
    }
}

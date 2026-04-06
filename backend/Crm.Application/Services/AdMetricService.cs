using Crm.Application.DTOs.AdMetrics;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Crm.Application.Services;

public class AdMetricService : IAdMetricService
{
    private readonly IGenericRepository<AdMetric> _repository;
    private readonly IGenericRepository<Project> _projectRepository;
    private readonly IGenericRepository<Contract> _contractRepository;
    private readonly IGenericRepository<ProjectAdAccount> _adAccountRepository;
    private readonly IEnumerable<IAdPlatformClient> _platformClients;
    private readonly ICurrentUserContext _currentUserContext;

    public AdMetricService(
        IGenericRepository<AdMetric> repository, 
        IGenericRepository<Project> projectRepository,
        IGenericRepository<Contract> contractRepository,
        IGenericRepository<ProjectAdAccount> adAccountRepository,
        IEnumerable<IAdPlatformClient> platformClients,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _projectRepository = projectRepository;
        _contractRepository = contractRepository;
        _adAccountRepository = adAccountRepository;
        _platformClients = platformClients;
        _currentUserContext = currentUserContext;
    }

    public async Task<IEnumerable<AdMetricResponse>> GetProjectMetricsAsync(Guid projectId)
    {
        var metrics = await _repository.GetAllAsync();
        return metrics.Where(m => m.ProjectId == projectId).Select(MapToResponse).ToList();
    }

    public async Task<IEnumerable<AdMetricResponse>> GetAllAsync()
    {
        var metrics = await _repository.GetAllAsync();
        return metrics.Select(MapToResponse).ToList();
    }

    public async Task<AdMetricAnalyticsResponse> GetProjectAnalyticsAsync(Guid projectId)
    {
        var metrics = await _repository.AsQueryable()
            .Where(m => m.ProjectId == projectId)
            .ToListAsync();
        var contracts = await _contractRepository.GetAllAsync();
        var projectContracts = contracts.Where(c => c.ProjectId == projectId && c.Status == ContractStatus.Signed).ToList();

        var totalSpend = metrics.Sum(m => m.Spend);
        var totalRevenue = projectContracts.Sum(c => c.TotalAmount);

        var analytics = new AdMetricAnalyticsResponse
        {
            ProjectId = projectId,
            TotalSpend = totalSpend,
            TotalImpressions = metrics.Sum(m => m.Impressions),
            TotalClicks = metrics.Sum(m => m.Clicks),
            TotalConversions = metrics.Sum(m => m.Conversions),
            RawMetrics = metrics.Select(MapToResponse).ToList(),
            ROAS = totalSpend > 0 ? totalRevenue / totalSpend : 0,
            ProjectROI = totalSpend > 0 ? (totalRevenue - totalSpend) / totalSpend * 100 : 0
        };

        return analytics;
    }

    public async Task<AdMetricAnalyticsResponse> GetGlobalAnalyticsAsync()
    {
        var metrics = (await _repository.GetAllAsync()).ToList();
        var contracts = await _contractRepository.GetAllAsync();
        var signedContracts = contracts.Where(c => c.Status == ContractStatus.Signed).ToList();

        var totalSpend = metrics.Sum(m => m.Spend);
        var totalRevenue = signedContracts.Sum(c => c.TotalAmount);

        var analytics = new AdMetricAnalyticsResponse
        {
            ProjectId = Guid.Empty,
            TotalSpend = totalSpend,
            TotalImpressions = metrics.Sum(m => m.Impressions),
            TotalClicks = metrics.Sum(m => m.Clicks),
            TotalConversions = metrics.Sum(m => m.Conversions),
            RawMetrics = metrics.Select(MapToResponse).ToList(),
            ROAS = totalSpend > 0 ? totalRevenue / totalSpend : 0,
            ProjectROI = totalSpend > 0 ? (totalRevenue - totalSpend) / totalSpend * 100 : 0
        };

        return analytics;
    }

    public async Task SyncMetricsAsync(Guid projectId)
    {
        var accounts = (await _adAccountRepository.GetAllAsync())
            .Where(a => a.ProjectId == projectId && a.IsActive);

        foreach (var account in accounts)
        {
            var client = _platformClients.FirstOrDefault(c => c.Platform == account.Platform);
            if (client == null) continue;

            // Sync last 2 days to ensure no gaps
            for (int i = 0; i <= 1; i++)
            {
                var date = DateTime.UtcNow.AddDays(-i).Date;
                var newMetrics = await client.FetchDailyMetricsAsync(account.ExternalAccountId, date);

                foreach (var m in newMetrics)
                {
                    // Check if already exists to prevent duplicates
                    var existing = (await _repository.GetAllAsync())
                        .FirstOrDefault(em => em.AdAccountId == account.Id && em.Date.Date == date.Date);

                    if (existing != null)
                    {
                        existing.Spend = m.Spend;
                        existing.Impressions = m.Impressions;
                        existing.Clicks = m.Clicks;
                        existing.Conversions = m.Conversions;
                        await _repository.UpdateAsync(existing);
                    }
                    else
                    {
                        m.ProjectId = projectId;
                        m.AdAccountId = account.Id;
                        m.TenantId = account.TenantId;
                        await _repository.AddAsync(m);
                    }
                }
            }
        }
        await _repository.SaveChangesAsync();
    }

    public async Task<decimal> GetSpendByRangeAsync(Guid projectId, DateTime start, DateTime end)
    {
        var metrics = await _repository.AsQueryable()
            .Where(m => m.ProjectId == projectId && m.Date >= start && m.Date <= end)
            .ToListAsync();
        
        return metrics.Sum(m => m.Spend);
    }

    public async Task<AdMetricResponse> CreateAsync(CreateAdMetricRequest request)
    {
        var metric = new AdMetric
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Platform = request.Platform,
            Spend = request.Spend,
            Impressions = request.Impressions,
            Clicks = request.Clicks,
            Conversions = request.Conversions,
            Date = request.Date,
            TenantId = _currentUserContext.TenantId ?? Guid.Empty
        };

        await _repository.AddAsync(metric);
        await _repository.SaveChangesAsync();

        return MapToResponse(metric);
    }

    private AdMetricResponse MapToResponse(AdMetric m)
    {
        return new AdMetricResponse
        {
            Id = m.Id,
            ProjectId = m.ProjectId,
            Platform = m.Platform,
            Spend = m.Spend,
            Impressions = m.Impressions,
            Clicks = m.Clicks,
            Conversions = m.Conversions,
            Date = m.Date,
            CreatedAt = m.CreatedAt
        };
    }
}

using Crm.Application.DTOs.AdMetrics;

namespace Crm.Application.Interfaces;

public interface IAdMetricService
{
    Task<IEnumerable<AdMetricResponse>> GetProjectMetricsAsync(Guid projectId);
    Task<IEnumerable<AdMetricResponse>> GetAllAsync();
    Task<AdMetricAnalyticsResponse> GetProjectAnalyticsAsync(Guid projectId);
    Task<AdMetricAnalyticsResponse> GetGlobalAnalyticsAsync();
    Task<AdMetricResponse> CreateAsync(CreateAdMetricRequest request);
    Task SyncMetricsAsync(Guid projectId);
    Task<decimal> GetSpendByRangeAsync(Guid projectId, DateTime start, DateTime end);
}

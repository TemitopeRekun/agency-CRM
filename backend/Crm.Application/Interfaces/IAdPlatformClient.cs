using Crm.Domain.Entities;

namespace Crm.Application.Interfaces;

public interface IAdPlatformClient
{
    AdPlatform Platform { get; }
    Task<IEnumerable<AdMetric>> FetchDailyMetricsAsync(string externalAccountId, DateTime date);
}

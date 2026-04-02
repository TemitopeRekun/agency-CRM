using Crm.Domain.Entities;

namespace Crm.Application.DTOs.AdMetrics;

public class AdMetricAnalyticsResponse
{
    public Guid ProjectId { get; set; }
    public decimal TotalSpend { get; set; }
    public long TotalImpressions { get; set; }
    public long TotalClicks { get; set; }
    public long TotalConversions { get; set; }
    
    // Calculated ROI fields
    public decimal CostPerLead => TotalConversions > 0 ? TotalSpend / TotalConversions : 0;
    public decimal ConversionRate => TotalClicks > 0 ? (decimal)TotalConversions / TotalClicks * 100 : 0;
    public decimal CTR => TotalImpressions > 0 ? (decimal)TotalClicks / TotalImpressions * 100 : 0;
    public decimal ROAS { get; set; } // (Revenue - Spend) / Spend
    public decimal ProjectROI { get; set; }

    public List<AdMetricResponse> RawMetrics { get; set; } = new();
}

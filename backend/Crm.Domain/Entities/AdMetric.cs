using Crm.Domain.Entities;

namespace Crm.Domain.Entities;

public enum AdPlatform
{
    Google,
    Meta,
    TikTok,
    LinkedIn
}

public class AdMetric : BaseEntity, ITenantedEntity
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public Guid? AdAccountId { get; set; }
    public ProjectAdAccount? AdAccount { get; set; }
    
    public AdPlatform Platform { get; set; }
    public decimal Spend { get; set; }
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public long Conversions { get; set; }
    public DateTime Date { get; set; }
    
    public Guid TenantId { get; set; }
}

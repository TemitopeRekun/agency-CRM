using Crm.Domain.Entities;

namespace Crm.Domain.Entities;

public class ProjectAdAccount : BaseEntity, ITenantedEntity
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public AdPlatform Platform { get; set; }
    public string ExternalAccountId { get; set; } = string.Empty;
    public string? AccessToken { get; set; } // Simplified for MVP
    public bool IsActive { get; set; } = true;
    
    public Guid TenantId { get; set; }
    
    public ICollection<AdMetric> Metrics { get; set; } = new List<AdMetric>();
}

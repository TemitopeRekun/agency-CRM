namespace Crm.Domain.Entities;

public class TaskTemplate : BaseEntity, ITenantedEntity
{
    public string ServiceType { get; set; } = string.Empty; // e.g., "Google Ads", "SEO", "Content Marketing"
    public string TaskTitle { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public string DefaultPriority { get; set; } = "Normal";
    public Guid TenantId { get; set; }
}

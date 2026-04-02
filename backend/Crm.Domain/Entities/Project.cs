namespace Crm.Domain.Entities;

public enum ProjectStatus
{
    Active,
    OnHold,
    Completed,
    Cancelled
}

public class Project : BaseEntity, ITenantedEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public Guid? OfferId { get; set; }
    public Offer? Offer { get; set; }
    public Guid TenantId { get; set; }
    public ICollection<CrmTask> Tasks { get; set; } = new List<CrmTask>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<ProjectAdAccount> AdAccounts { get; set; } = new List<ProjectAdAccount>();
}

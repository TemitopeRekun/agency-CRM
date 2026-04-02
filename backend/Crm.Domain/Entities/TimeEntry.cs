namespace Crm.Domain.Entities;

public class TimeEntry : BaseEntity, ITenantedEntity
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public Guid TaskId { get; set; }
    public CrmTask? Task { get; set; }
    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public decimal Hours { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    
    public Guid TenantId { get; set; }
}

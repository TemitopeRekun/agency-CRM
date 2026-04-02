namespace Crm.Domain.Entities;

// Named CrmTask to avoid conflict with System.Threading.Tasks.Task
public class CrmTask : BaseEntity, ITenantedEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Todo";
    public string Priority { get; set; } = "Normal";
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public Guid TenantId { get; set; }
}

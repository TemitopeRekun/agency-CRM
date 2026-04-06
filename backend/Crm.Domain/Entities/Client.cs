namespace Crm.Domain.Entities;

public enum PriorityTier
{
    Tier1,
    Tier2,
    Tier3
}

public class Client : BaseEntity, ITenantedEntity
{
    public string Name { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public PriorityTier Priority { get; set; } = PriorityTier.Tier3;
    
    public Guid TenantId { get; set; }
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}

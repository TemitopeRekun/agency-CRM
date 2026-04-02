namespace Crm.Domain.Entities;

public enum OfferStatus
{
    Draft,
    Sent,
    Accepted,
    Rejected
}

public class Offer : BaseEntity, ITenantedEntity
{
    public string Title { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OfferStatus Status { get; set; } = OfferStatus.Draft;
    public string Notes { get; set; } = string.Empty;
    public DateTimeOffset? ViewedAt { get; set; }
    public Guid LeadId { get; set; }
    public Lead? Lead { get; set; }
    public string? QuoteTemplateId { get; set; }
    public DateTimeOffset? QuoteOpenedAt { get; set; }
    public bool HasBeenViewed { get; set; }
    public Guid TenantId { get; set; }
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}

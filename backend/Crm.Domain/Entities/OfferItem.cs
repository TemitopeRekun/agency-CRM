using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Domain.Entities;

public class OfferItem : BaseEntity, ITenantedEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Order { get; set; }
    
    public Guid OfferId { get; set; }
    public Offer? Offer { get; set; }
    public Guid TenantId { get; set; }
}

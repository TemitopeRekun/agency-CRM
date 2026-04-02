using Crm.Domain.Entities;

namespace Crm.Application.DTOs.Offers;

public class CreateOfferRequest
{
    public string Title { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public Guid LeadId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? QuoteTemplateId { get; set; }
}

public class UpdateOfferStatusRequest
{
    public OfferStatus Status { get; set; }
}

public class OfferResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OfferStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid LeadId { get; set; }
    public string? QuoteTemplateId { get; set; }
    public DateTimeOffset? QuoteOpenedAt { get; set; }
    public bool HasBeenViewed { get; set; }
    public DateTime CreatedAt { get; set; }
}

using Crm.Application.DTOs.Offers;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;

namespace Crm.Application.Services;

public class OfferService
{
    private readonly IGenericRepository<Offer> _repository;
    private readonly IAutomationService _automationService;
    private readonly IEmailService _emailService;

    public OfferService(
        IGenericRepository<Offer> repository,
        IAutomationService automationService,
        IEmailService emailService)
    {
        _repository = repository;
        _automationService = automationService;
        _emailService = emailService;
    }

    public async Task<IEnumerable<OfferResponse>> GetAllAsync()
    {
        var offers = await _repository.GetAllAsync();
        return offers.Select(MapToResponse);
    }

    public async Task<OfferResponse> CreateAsync(CreateOfferRequest request)
    {
        var offer = new Offer
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            TotalAmount = request.TotalAmount,
            Notes = request.Notes,
            LeadId = request.LeadId,
            QuoteTemplateId = request.QuoteTemplateId
        };

        await _repository.AddAsync(offer);
        await _repository.SaveChangesAsync();

        return MapToResponse(offer);
    }

    public async Task<OfferResponse?> UpdateStatusAsync(Guid id, UpdateOfferStatusRequest request)
    {
        var offer = await _repository.GetByIdAsync(id);
        if (offer == null) return null;

        var previousStatus = offer.Status;
        offer.Status = request.Status;
        
        await _repository.UpdateAsync(offer);

        // Automation: When moving to Accepted, trigger Automation Engine
        if (previousStatus != OfferStatus.Accepted && request.Status == OfferStatus.Accepted)
        {
            await _automationService.ProcessAcceptedOfferAsync(id);
        }

        // Notification: When moving to Sent, trigger Email
        if (request.Status == OfferStatus.Sent)
        {
            await _emailService.SendEmailAsync("client@example.com", "New Proposal: " + offer.Title, "Hello, your proposal is ready for review at /portal/" + id);
        }

        await _repository.SaveChangesAsync();

        return MapToResponse(offer);
    }

    public async Task<OfferResponse?> MarkAsViewedAsync(Guid id)
    {
        var offer = await _repository.GetByIdAsync(id);
        if (offer == null) return null;

        offer.HasBeenViewed = true;
        offer.QuoteOpenedAt ??= DateTimeOffset.UtcNow;
        
        await _repository.UpdateAsync(offer);
        await _repository.SaveChangesAsync();

        return MapToResponse(offer);
    }

    private OfferResponse MapToResponse(Offer o)
    {
        return new OfferResponse
        {
            Id = o.Id,
            Title = o.Title,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            Notes = o.Notes,
            LeadId = o.LeadId,
            QuoteTemplateId = o.QuoteTemplateId,
            QuoteOpenedAt = o.QuoteOpenedAt,
            HasBeenViewed = o.HasBeenViewed,
            CreatedAt = o.CreatedAt
        };
    }
}

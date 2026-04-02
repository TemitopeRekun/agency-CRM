using Crm.Application.DTOs.Offers;
using Crm.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize(Roles = "Admin,SalesManager")]
[ApiController]
[Route("api/[controller]")]
public class OffersController : ControllerBase
{
    private readonly OfferService _offerService;
    private readonly ILogger<OffersController> _logger;

    public OffersController(OfferService offerService, ILogger<OffersController> logger)
    {
        _offerService = offerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OfferResponse>>> GetOffers()
    {
        var offers = await _offerService.GetAllAsync();
        return Ok(offers);
    }

    [HttpPost]
    public async Task<ActionResult<OfferResponse>> CreateOffer(CreateOfferRequest request)
    {
        _logger.LogInformation("Creating new offer: {Title}", request.Title);
        var response = await _offerService.CreateAsync(request);
        _logger.LogInformation("Offer created successfully with ID: {Id}", response.Id);
        return CreatedAtAction(nameof(GetOffers), new { id = response.Id }, response);
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<OfferResponse>> UpdateStatus(Guid id, UpdateOfferStatusRequest request)
    {
        _logger.LogInformation("Updating status for offer: {Id}", id);
        var response = await _offerService.UpdateStatusAsync(id, request);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpPost("{id}/view")]
    public async Task<ActionResult<OfferResponse>> MarkAsViewed(Guid id)
    {
        _logger.LogInformation("Marking offer as viewed: {Id}", id);
        var response = await _offerService.MarkAsViewedAsync(id);
        if (response == null) return NotFound();
        return Ok(response);
    }
}

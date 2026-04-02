namespace Crm.Application.Interfaces;

public interface IAutomationService
{
    Task ProcessAcceptedOfferAsync(Guid offerId);
    Task NotifyOverdueInvoicesAsync();
    Task GenerateMonthlyInvoicesAsync();
}

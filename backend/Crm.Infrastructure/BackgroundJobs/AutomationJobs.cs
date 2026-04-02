using Crm.Application.Interfaces;
using Crm.Application.Services;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.BackgroundJobs;

public class AutomationJobs
{
    private readonly IAutomationService _automationService;
    private readonly ILogger<AutomationJobs> _logger;

    public AutomationJobs(IAutomationService automationService, ILogger<AutomationJobs> logger)
    {
        _automationService = automationService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task CheckOverdueInvoicesJob()
    {
        _logger.LogInformation("Hangfire: Starting CheckOverdueInvoicesJob at {Timestamp}", DateTime.UtcNow);
        await _automationService.NotifyOverdueInvoicesAsync();
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task MonthlyBillingJob()
    {
        _logger.LogInformation("Hangfire: Starting MonthlyBillingJob at {Timestamp}", DateTime.UtcNow);
        await _automationService.GenerateMonthlyInvoicesAsync();
    }
}

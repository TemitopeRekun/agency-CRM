using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Crm.Application.Services;

public class AutomationService : IAutomationService
{
    private readonly IGenericRepository<Offer> _offerRepository;
    private readonly IGenericRepository<Project> _projectRepository;
    private readonly IGenericRepository<Contract> _contractRepository;
    private readonly IGenericRepository<CrmTask> _taskRepository;
    private readonly IGenericRepository<TaskTemplate> _templateRepository;
    private readonly IGenericRepository<Invoice> _invoiceRepository;
    private readonly IGenericRepository<Lead> _leadRepository;
    private readonly IGenericRepository<Client> _clientRepository;
    private readonly ILogger<AutomationService> _logger;
    private readonly ICurrentUserContext _userContext;
    private readonly ISlackService _slackService;
    private readonly IEmailService _emailService;
    private readonly InvoiceService _invoiceService;

    public AutomationService(
        IGenericRepository<Offer> offerRepository,
        IGenericRepository<Project> projectRepository,
        IGenericRepository<Contract> contractRepository,
        IGenericRepository<CrmTask> taskRepository,
        IGenericRepository<TaskTemplate> templateRepository,
        IGenericRepository<Invoice> invoiceRepository,
        IGenericRepository<Lead> leadRepository,
        IGenericRepository<Client> clientRepository,
        ILogger<AutomationService> logger,
        ICurrentUserContext userContext,
        ISlackService slackService,
        IEmailService emailService,
        InvoiceService invoiceService)
    {
        _offerRepository = offerRepository;
        _projectRepository = projectRepository;
        _contractRepository = contractRepository;
        _taskRepository = taskRepository;
        _templateRepository = templateRepository;
        _invoiceRepository = invoiceRepository;
        _leadRepository = leadRepository;
        _clientRepository = clientRepository;
        _logger = logger;
        _userContext = userContext;
        _slackService = slackService;
        _emailService = emailService;
        _invoiceService = invoiceService;
    }

    public async Task ProcessAcceptedOfferAsync(Guid offerId)
    {
        _logger.LogInformation("Starting automation for accepted offer {OfferId}", offerId);

        var offer = await _offerRepository.AsQueryable()
            .Include(o => o.Lead)
            .FirstOrDefaultAsync(o => o.Id == offerId);

        if (offer == null || offer.Status != OfferStatus.Accepted)
        {
            _logger.LogWarning("Offer {OfferId} not found or not in Accepted status.", offerId);
            return;
        }

        var lead = await _leadRepository.GetByIdAsync(offer.LeadId);
        if (lead == null)
        {
            _logger.LogWarning("Lead not found for Offer {OfferId}. Aborting automation.", offerId);
            return;
        }

        // 1. Create or Reuse Client from Lead
        Guid clientId;
        if (lead.ConvertedClientId.HasValue)
        {
            clientId = lead.ConvertedClientId.Value;
        }
        else
        {
            var newClient = new Client
            {
                Id = Guid.NewGuid(),
                Name = lead.Title,
                TenantId = offer.TenantId
            };
            await _clientRepository.AddAsync(newClient);
            clientId = newClient.Id;

            lead.ConvertedClientId = clientId;
            await _leadRepository.UpdateAsync(lead);
        }

        // 2. Create Project
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = offer.Title,
            Description = lead.Description ?? $"Project created automatically from accepted offer: {offer.Title}",
            Status = ProjectStatus.Active,
            ClientId = clientId,
            TenantId = offer.TenantId
        };
        await _projectRepository.AddAsync(project);

        // 2. Create Contract (Draft)
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            Title = $"Contract for {offer.Title}",
            Status = ContractStatus.Draft,
            ProjectId = project.Id,
            TenantId = offer.TenantId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(12) // Default 1 year
        };
        await _contractRepository.AddAsync(contract);

        // 3. Create Tasks from Templates (Modular Mapping)
        // For the MVP, we'll look for templates matching common agency services.
        // We can parse the offer title or notes for keywords.
        var templates = await _templateRepository.GetAllAsync();
        var automatedTasksCount = 0;

        foreach (var template in templates)
        {
            // Simple keyword matching for MVP automation
            if (offer.Title.Contains(template.ServiceType, StringComparison.OrdinalIgnoreCase) || 
                offer.Notes.Contains(template.ServiceType, StringComparison.OrdinalIgnoreCase))
            {
                var task = new CrmTask
                {
                    Id = Guid.NewGuid(),
                    Title = template.TaskTitle,
                    Description = template.TaskDescription,
                    Status = "Todo",
                    Priority = template.DefaultPriority,
                    ProjectId = project.Id,
                    TenantId = offer.TenantId
                };
                await _taskRepository.AddAsync(task);
                automatedTasksCount++;
            }
        }

        // If no templates matched, create a default kickoff task
        if (automatedTasksCount == 0)
        {
            await _taskRepository.AddAsync(new CrmTask
            {
                Id = Guid.NewGuid(),
                Title = "Project Kickoff",
                Description = "Initial setup and client meeting for the new project.",
                Status = "Todo",
                Priority = "High",
                ProjectId = project.Id,
                TenantId = offer.TenantId
            });
        }

        await _projectRepository.SaveChangesAsync();
        
        await _slackService.SendNotificationAsync($"🚀 Offer Accepted: {offer.Title}. Project and Contract successfully created.");

        _logger.LogInformation("Automation completed for Offer {OfferId}. Created Project {ProjectId} with {TaskCount} tasks.", offerId, project.Id, automatedTasksCount);
    }

    public async Task NotifyOverdueInvoicesAsync()
    {
        _logger.LogInformation("Scanning for overdue invoices...");
        
        var overdueInvoices = await _invoiceRepository.AsQueryable()
            .Include(i => i.Client)
            .Where(i => i.Status == InvoiceStatus.Sent && i.DueDate < DateTime.UtcNow)
            .ToListAsync();

        foreach (var invoice in overdueInvoices)
        {
            _logger.LogWarning("OVERDUE INVOICE ALERT: Invoice {InvoiceNumber} is overdue.", invoice.InvoiceNumber);
            
            // Notification: Trigger Placeholder Email
            await _emailService.SendEmailAsync("client@example.com", $"Overdue Invoice: {invoice.InvoiceNumber}", 
                $"Dear Client, your invoice {invoice.InvoiceNumber} for {invoice.TotalAmount} {invoice.Currency} is overdue since {invoice.DueDate.ToShortDateString()}. Please settle as soon as possible.");
        }
    }

    public async Task GenerateMonthlyInvoicesAsync()
    {
        _logger.LogInformation("Starting automated Monthly Billing cycle...");
        
        var contracts = await _contractRepository.AsQueryable()
            .Where(c => c.Status == ContractStatus.Signed)
            .ToListAsync();

        int generatedCount = 0;
        foreach (var contract in contracts)
        {
            // Only bill if not already billed this month
            if (contract.LastInvoicedAt.HasValue && 
                contract.LastInvoicedAt.Value.Month == DateTime.UtcNow.Month &&
                contract.LastInvoicedAt.Value.Year == DateTime.UtcNow.Year)
            {
                continue;
            }

            try
            {
                _logger.LogInformation("Generating monthly invoice for Contract {ContractId} (Project {ProjectId})", contract.Id, contract.ProjectId);
                await _invoiceService.GenerateFromContractAsync(contract.Id);
                generatedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate monthly invoice for Contract {ContractId}: {Message}", contract.Id, ex.Message);
            }
        }

        _logger.LogInformation("Monthly Billing cycle completed. Generated {Count} draft invoices.", generatedCount);
        if (generatedCount > 0)
        {
            await _slackService.SendNotificationAsync($"💰 Monthly Billing Complete: {generatedCount} draft invoices generated for review.");
        }
    }
}

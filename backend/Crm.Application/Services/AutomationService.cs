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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AutomationService> _logger;
    private readonly ICurrentUserContext _userContext;
    private readonly ISlackService _slackService;
    private readonly IEmailService _emailService;
    private readonly IInvoiceService _invoiceService;

    public AutomationService(
        IGenericRepository<Offer> offerRepository,
        IGenericRepository<Project> projectRepository,
        IGenericRepository<Contract> contractRepository,
        IGenericRepository<CrmTask> taskRepository,
        IGenericRepository<TaskTemplate> templateRepository,
        IGenericRepository<Invoice> invoiceRepository,
        IGenericRepository<Lead> leadRepository,
        IGenericRepository<Client> clientRepository,
        IUnitOfWork unitOfWork,
        ILogger<AutomationService> logger,
        ICurrentUserContext userContext,
        ISlackService slackService,
        IEmailService emailService,
        IInvoiceService invoiceService)
    {
        _offerRepository = offerRepository;
        _projectRepository = projectRepository;
        _contractRepository = contractRepository;
        _taskRepository = taskRepository;
        _templateRepository = templateRepository;
        _invoiceRepository = invoiceRepository;
        _leadRepository = leadRepository;
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _userContext = userContext;
        _slackService = slackService;
        _emailService = emailService;
        _invoiceService = invoiceService;
    }

    public async Task ProcessAcceptedOfferAsync(Guid offerId)
    {
        _logger.LogInformation("Starting automation for accepted offer {OfferId}", offerId);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var offer = await _offerRepository.AsQueryable()
                .Include(o => o.Lead)
                .Include(o => o.Items)
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

            // 3. Create Tasks from Offer Items (Detailed Phase Breakdown)
            var automatedTasksCount = 0;
            if (offer.Items != null && offer.Items.Any())
            {
                foreach (var item in offer.Items.OrderBy(i => i.Order))
                {
                    var task = new CrmTask
                    {
                        Id = Guid.NewGuid(),
                        Title = item.Title,
                        Description = item.Description ?? $"Phase: {item.Title}",
                        Status = "Todo",
                        Priority = "High",
                        ProjectId = project.Id,
                        TenantId = offer.TenantId
                    };
                    await _taskRepository.AddAsync(task);
                    automatedTasksCount++;
                }
            }
            else
            {
                // Fallback: look for templates matching common agency services.
                var templates = await _templateRepository.GetAllAsync();

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
            }

            // If no templates matched, create a default series of kickoff tasks
            if (automatedTasksCount == 0)
            {
                var defaultTasks = new[]
                {
                    new { Title = "Project Kickoff Meeting", Description = "Schedule and hold the internal and client kickoff meetings.", Priority = "High" },
                    new { Title = "Resource Allocation", Description = "Assign team members and define roles for this project.", Priority = "Medium" },
                    new { Title = "Client Information Gathering", Description = "Gather all necessary assets and access from the client.", Priority = "High" }
                };

                foreach (var dt in defaultTasks)
                {
                    await _taskRepository.AddAsync(new CrmTask
                    {
                        Id = Guid.NewGuid(),
                        Title = dt.Title,
                        Description = dt.Description,
                        Status = "Todo",
                        Priority = dt.Priority,
                        ProjectId = project.Id,
                        TenantId = offer.TenantId
                    });
                }
            }

            await _unitOfWork.CommitAsync();

            await _slackService.SendNotificationAsync($"🚀 Offer Accepted: {offer.Title}. Project and Contract successfully created.");
            _logger.LogInformation("Automation completed for Offer {OfferId}. Created Project {ProjectId} with {TaskCount} tasks.", offerId, project.Id, automatedTasksCount);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error processing accepted offer {OfferId}. Transaction rolled back.", offerId);
            throw;
        }
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
        _logger.LogInformation("Starting automated Monthly Billing cycle via InvoiceService...");
        var generatedCount = await _invoiceService.ProcessPendingBillingAsync();
        
        _logger.LogInformation("Monthly Billing cycle completed. Generated {Count} draft invoices.", generatedCount);
        if (generatedCount > 0)
        {
            await _slackService.SendNotificationAsync($"💰 Monthly Billing Complete: {generatedCount} draft invoices generated for review.");
        }
    }
}

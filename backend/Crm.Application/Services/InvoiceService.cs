using Crm.Application.DTOs.Invoices;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IGenericRepository<Invoice> _repository;
    private readonly IGenericRepository<Contract> _contractRepository;
    private readonly IGenericRepository<Project> _projectRepository;
    private readonly IAdMetricService _adMetricService;

    public InvoiceService(
        IGenericRepository<Invoice> repository,
        IGenericRepository<Contract> contractRepository,
        IGenericRepository<Project> projectRepository,
        IAdMetricService adMetricService)
    {
        _repository = repository;
        _contractRepository = contractRepository;
        _projectRepository = projectRepository;
        _adMetricService = adMetricService;
    }

    public async Task<IEnumerable<InvoiceResponse>> GetAllAsync()
    {
        var invoices = await _repository.AsQueryable()
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .ToListAsync();
        return invoices.Select(MapToResponse).ToList();
    }

    public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request)
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = request.InvoiceNumber,
            TotalAmount = request.TotalAmount,
            Currency = request.Currency,
            DueDate = request.DueDate,
            ContractId = request.ContractId,
            ProjectId = request.ProjectId,
            Status = InvoiceStatus.Draft,
            Items = request.Items?.Select(i => new InvoiceItem
            {
                Id = Guid.NewGuid(),
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList() ?? new List<InvoiceItem>()
        };

        await _repository.AddAsync(invoice);
        await _repository.SaveChangesAsync();

        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse> GenerateFromContractAsync(Guid contractId)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId)
            ?? throw new KeyNotFoundException("Contract not found");

        var startDate = contract.LastInvoicedAt?.DateTime ?? contract.CreatedAt;
        var endDate = DateTime.UtcNow;

        var items = new List<InvoiceItem>();

        // 1. Base Retainer
        if (contract.BaseRetainer > 0)
        {
            items.Add(new InvoiceItem
            {
                Id = Guid.NewGuid(),
                Description = $"Monthly Management Retainer: {contract.Title}",
                Quantity = 1,
                UnitPrice = contract.BaseRetainer
            });
        }

        // 2. Success Fee Calculation
        if (contract.SuccessFeeType != SuccessFeeType.None && contract.SuccessFeeValue > 0)
        {
            var analytics = await _adMetricService.GetProjectAnalyticsAsync(contract.ProjectId);
            
            if (contract.SuccessFeeType == SuccessFeeType.FixedPerLead)
            {
                var totalLeads = analytics.TotalConversions;
                if (totalLeads > 0)
                {
                    items.Add(new InvoiceItem
                    {
                        Id = Guid.NewGuid(),
                        Description = $"Success Bonus: {totalLeads} Leads @ ${contract.SuccessFeeValue}/lead",
                        Quantity = (decimal)totalLeads,
                        UnitPrice = contract.SuccessFeeValue
                    });
                }
            }
            else if (contract.SuccessFeeType == SuccessFeeType.RevenueShare)
            {
                // Calculate previous month's spend
                var now = DateTime.UtcNow;
                var firstOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
                var endOfLastMonth = firstOfCurrentMonth.AddDays(-1);
                var firstOfLastMonth = new DateTime(endOfLastMonth.Year, endOfLastMonth.Month, 1);

                var monthlySpend = await _adMetricService.GetSpendByRangeAsync(contract.ProjectId, firstOfLastMonth, endOfLastMonth);
                
                var bonus = monthlySpend * (contract.SuccessFeeValue / 100); 
                if (bonus > 0)
                {
                    items.Add(new InvoiceItem
                    {
                        Id = Guid.NewGuid(),
                        Description = $"Performance Bonus: {contract.SuccessFeeValue}% of {firstOfLastMonth:MMMM yyyy} Ad Spend (${monthlySpend:N2})",
                        Quantity = 1,
                        UnitPrice = bonus
                    });
                }
            }
        }

        // Standard line if no billing terms are defined
        if (items.Count == 0)
        {
            items.Add(new InvoiceItem
            {
                Id = Guid.NewGuid(),
                Description = $"Services as per contract: {contract.Title}",
                Quantity = 1,
                UnitPrice = contract.TotalAmount
            });
        }

        var totalAmount = items.Sum(i => i.Quantity * i.UnitPrice);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{contract.Title.Substring(0, Math.Min(3, contract.Title.Length)).ToUpper()}",
            TotalAmount = totalAmount,
            Currency = "USD",
            DueDate = DateTime.UtcNow.AddDays(30),
            ContractId = contractId,
            ProjectId = contract.ProjectId,
            Status = InvoiceStatus.Draft,
            Items = items
        };

        contract.LastInvoicedAt = DateTimeOffset.UtcNow;
        await _contractRepository.UpdateAsync(contract);
        await _repository.AddAsync(invoice);
        await _repository.SaveChangesAsync();

        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse> GenerateFromProjectAsync(Guid projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new KeyNotFoundException("Project not found");

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{project.Name.Substring(0, Math.Min(3, project.Name.Length)).ToUpper()}",
            TotalAmount = 0,
            Currency = "USD",
            DueDate = DateTime.UtcNow.AddDays(14),
            ProjectId = projectId,
            Status = InvoiceStatus.Draft,
            Items = new List<InvoiceItem>
            {
                new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    Description = $"Project Services: {project.Name}",
                    Quantity = 1,
                    UnitPrice = 0 // Needs manual update
                }
            }
        };

        await _repository.AddAsync(invoice);
        await _repository.SaveChangesAsync();

        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse?> UpdateStatusAsync(Guid id, InvoiceStatus newStatus)
    {
        var invoice = await _repository.GetByIdAsync(id);
        if (invoice == null) return null;

        invoice.Status = newStatus;
        await _repository.UpdateAsync(invoice);
        await _repository.SaveChangesAsync();

        return MapToResponse(invoice);
    }

    public async Task<int> ProcessPendingBillingAsync()
    {
        var contracts = await _contractRepository.AsQueryable()
            .Where(c => c.Status == ContractStatus.Signed)
            .ToListAsync();

        int generatedCount = 0;
        var now = DateTimeOffset.UtcNow;

        foreach (var contract in contracts)
        {
            // If never invoiced, check StartDate. If invoiced, check 30 days gap.
            bool needsInvoicing = false;
            if (contract.LastInvoicedAt == null)
            {
                needsInvoicing = contract.StartDate <= now.DateTime;
            }
            else
            {
                needsInvoicing = (now - contract.LastInvoicedAt.Value).TotalDays >= 30;
            }

            if (needsInvoicing)
            {
                try 
                {
                    await GenerateFromContractAsync(contract.Id);
                    generatedCount++;
                }
                catch (Exception ex)
                {
                    // Log error for specific contract but continue with others
                    Console.WriteLine($"Error billing contract {contract.Id}: {ex.Message}");
                }
            }
        }

        return generatedCount;
    }

    public async Task<InvoiceResponse?> UpdateAsync(Guid id, UpdateInvoiceRequest request)
    {
        var invoice = await _repository.AsQueryable()
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);
            
        if (invoice == null) return null;

        invoice.TotalAmount = request.TotalAmount;
        invoice.DueDate = request.DueDate;
        invoice.Status = request.Status;

        invoice.Items.Clear();
        foreach (var item in request.Items)
        {
            invoice.Items.Add(new InvoiceItem
            {
                Id = Guid.NewGuid(),
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                InvoiceId = invoice.Id
            });
        }

        await _repository.UpdateAsync(invoice);
        await _repository.SaveChangesAsync();

        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse?> RecordPaymentAsync(Guid invoiceId, RecordPaymentRequest request)
    {
        var invoice = await _repository.AsQueryable()
            .Include(i => i.Payments)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null) return null;

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            PaymentDate = request.PaymentDate,
            Method = request.Method,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            InvoiceId = invoiceId,
            TenantId = invoice.TenantId
        };

        invoice.Payments.Add(payment);
        invoice.PaidAmount += request.Amount;

        if (invoice.PaidAmount >= invoice.TotalAmount)
        {
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        await _repository.UpdateAsync(invoice);
        await _repository.SaveChangesAsync();

        return MapToResponse(invoice);
    }

    private InvoiceResponse MapToResponse(Invoice i)
    {
        return new InvoiceResponse
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            TotalAmount = i.TotalAmount,
            PaidAmount = i.PaidAmount,
            BalanceAmount = i.BalanceAmount,
            Currency = i.Currency,
            Status = i.Status,
            DueDate = i.DueDate,
            ProjectId = i.ProjectId,
            CreatedAt = i.CreatedAt,
            Items = (i.Items ?? new List<InvoiceItem>()).Select(item => new InvoiceItemResponse
            {
                Id = item.Id,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Amount = item.Amount
            }).ToList(),
            Payments = (i.Payments ?? new List<Payment>()).Select(p => new PaymentResponse
            {
                Id = p.Id,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Method = p.Method,
                ReferenceNumber = p.ReferenceNumber,
                Notes = p.Notes
            }).ToList()
        };
    }
}

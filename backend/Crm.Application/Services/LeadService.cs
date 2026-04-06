using Crm.Application.DTOs.Leads;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;

namespace Crm.Application.Services;

public class LeadService
{
    private readonly IGenericRepository<Lead> _repository;

    public LeadService(IGenericRepository<Lead> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<LeadResponse>> GetAllAsync()
    {
        var leads = await _repository.GetAllAsync();
        return leads.Select(l => new LeadResponse
        {
            Id = l.Id,
            Title = l.Title,
            Description = l.Description,
            ContactName = l.ContactName,
            CompanyName = l.CompanyName,
            Email = l.Email,
            Phone = l.Phone,
            Source = l.Source,
            Interest = l.Interest,
            BudgetRange = l.BudgetRange,
            Status = l.Status,
            PipelineStage = l.PipelineStage,
            Probability = l.Probability,
            DealValue = l.DealValue,
            OwnerId = l.OwnerId,
            CreatedAt = l.CreatedAt
        });
    }

    public async Task<LeadResponse> CreateAsync(CreateLeadRequest request)
    {
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            ContactName = request.ContactName,
            CompanyName = request.CompanyName,
            Email = request.Email,
            Phone = request.Phone,
            Source = request.Source,
            Interest = request.Interest,
            BudgetRange = request.BudgetRange,
            PipelineStage = PipelineStage.Discovery,
            Status = LeadStatus.New
        };

        await _repository.AddAsync(lead);
        await _repository.SaveChangesAsync();

        return new LeadResponse
        {
            Id = lead.Id,
            Title = lead.Title,
            Description = lead.Description,
            ContactName = lead.ContactName,
            CompanyName = lead.CompanyName,
            Email = lead.Email,
            Phone = lead.Phone,
            Source = lead.Source,
            Interest = lead.Interest,
            BudgetRange = lead.BudgetRange,
            Status = lead.Status,
            PipelineStage = lead.PipelineStage,
            Probability = lead.Probability,
            DealValue = lead.DealValue,
            OwnerId = lead.OwnerId,
            CreatedAt = lead.CreatedAt
        };
    }

    public async Task<LeadResponse?> UpdateAsync(Guid id, UpdateLeadRequest request)
    {
        var lead = await _repository.GetByIdAsync(id);
        if (lead == null) return null;

        lead.Title = request.Title;
        lead.Description = request.Description;
        lead.ContactName = request.ContactName;
        lead.CompanyName = request.CompanyName;
        lead.Email = request.Email;
        lead.Phone = request.Phone;
        lead.Source = request.Source;
        lead.Interest = request.Interest;
        lead.BudgetRange = request.BudgetRange;
        lead.Status = request.Status;
        lead.PipelineStage = request.PipelineStage;
        lead.Probability = request.Probability;
        lead.DealValue = request.DealValue;
        lead.OwnerId = request.OwnerId;

        await _repository.UpdateAsync(lead);
        await _repository.SaveChangesAsync();

        return new LeadResponse
        {
            Id = lead.Id,
            Title = lead.Title,
            Description = lead.Description,
            ContactName = lead.ContactName,
            CompanyName = lead.CompanyName,
            Email = lead.Email,
            Phone = lead.Phone,
            Source = lead.Source,
            Interest = lead.Interest,
            BudgetRange = lead.BudgetRange,
            Status = lead.Status,
            PipelineStage = lead.PipelineStage,
            Probability = lead.Probability,
            DealValue = lead.DealValue,
            OwnerId = lead.OwnerId,
            CreatedAt = lead.CreatedAt
        };
    }

    public async Task<LeadResponse?> UpdateStatusAsync(Guid id, UpdateLeadStatusRequest request)
    {
        var lead = await _repository.GetByIdAsync(id);
        if (lead == null) return null;

        lead.Status = request.Status;
        await _repository.UpdateAsync(lead);
        await _repository.SaveChangesAsync();

        return new LeadResponse
        {
            Id = lead.Id,
            Title = lead.Title,
            Description = lead.Description,
            ContactName = lead.ContactName,
            CompanyName = lead.CompanyName,
            Email = lead.Email,
            Phone = lead.Phone,
            Source = lead.Source,
            Interest = lead.Interest,
            BudgetRange = lead.BudgetRange,
            Status = lead.Status,
            PipelineStage = lead.PipelineStage,
            Probability = lead.Probability,
            DealValue = lead.DealValue,
            OwnerId = lead.OwnerId,
            CreatedAt = lead.CreatedAt
        };
    }
}

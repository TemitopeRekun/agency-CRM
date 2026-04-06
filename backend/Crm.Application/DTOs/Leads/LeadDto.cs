using System.ComponentModel.DataAnnotations;
using Crm.Domain.Entities;

namespace Crm.Application.DTOs.Leads;

public class CreateLeadRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ContactName { get; set; } = string.Empty;

    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(50)]
    public string Phone { get; set; } = string.Empty;

    public LeadSource Source { get; set; }
    public ServiceType Interest { get; set; }
    public string BudgetRange { get; set; } = string.Empty;
}

public class UpdateLeadRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ContactName { get; set; } = string.Empty;

    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(50)]
    public string Phone { get; set; } = string.Empty;

    public LeadSource Source { get; set; }
    public ServiceType Interest { get; set; }
    public string BudgetRange { get; set; } = string.Empty;
    public LeadStatus Status { get; set; }
    public PipelineStage PipelineStage { get; set; }
    public int Probability { get; set; }
    public decimal? DealValue { get; set; }
    public Guid? OwnerId { get; set; }
}

public class UpdateLeadStatusRequest
{
    public LeadStatus Status { get; set; }
}

public class LeadResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public LeadSource Source { get; set; }
    public ServiceType Interest { get; set; }
    public string BudgetRange { get; set; } = string.Empty;
    public LeadStatus Status { get; set; }
    public PipelineStage PipelineStage { get; set; }
    public int Probability { get; set; }
    public decimal? DealValue { get; set; }
    public Guid? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
}

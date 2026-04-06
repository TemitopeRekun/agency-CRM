namespace Crm.Domain.Entities;

public enum LeadStatus
{
    New,
    Contacted,
    Qualified,
    Lost
}

public enum LeadSource
{
    Facebook,
    Google,
    Website,
    Referral,
    Manual
}

public enum ServiceType
{
    Development,
    Marketing,
    Staffing,
    Other
}

public enum PipelineStage
{
    Discovery,
    Proposal,
    Negotiation
}

public class Lead : BaseEntity, ITenantedEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    
    public LeadSource Source { get; set; } = LeadSource.Manual;
    public ServiceType Interest { get; set; } = ServiceType.Other;
    public string BudgetRange { get; set; } = string.Empty;
    
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public PipelineStage PipelineStage { get; set; } = PipelineStage.Discovery;
    public int Probability { get; set; } = 0; // 0-100
    public decimal? DealValue { get; set; }
    
    public Guid? OwnerId { get; set; }
    public User? Owner { get; set; }
    
    public Guid? ConvertedClientId { get; set; }
    public Client? ConvertedClient { get; set; }
    public Guid TenantId { get; set; }
    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
}

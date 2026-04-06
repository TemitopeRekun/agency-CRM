namespace Crm.Domain.Entities;

public enum ContractStatus
{
    Draft,
    Sent,
    Signed,
    Completed,
    Cancelled,
    Archived
}

public enum SuccessFeeType
{
    None,
    FixedPerLead,
    RevenueShare
}

public class Contract : BaseEntity, ITenantedEntity
{
    public string Title { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
    public string Terms { get; set; } = string.Empty;
    public string Kpis { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(12);
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    
    // Portal and Signature
    public Guid PortalToken { get; set; } = Guid.NewGuid();
    public string? SignatureData { get; set; }
    public DateTimeOffset? SignedAt { get; set; }
    public string? SignerIp { get; set; }
    public DateTimeOffset? ViewedAt { get; set; }
    public bool HasBeenViewed { get; set; }
    
    // Billing Terms
    public decimal BaseRetainer { get; set; }
    public SuccessFeeType SuccessFeeType { get; set; } = SuccessFeeType.None;
    public decimal SuccessFeeValue { get; set; } // $ amount or % percentage
    public DateTimeOffset? LastInvoicedAt { get; set; }
    
    public int Version { get; set; } = 1;
    public string SignatureStatus { get; set; } = "Not Sent";
    public bool IsWaitingSignature { get; set; } = false;
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public Guid TenantId { get; set; }
}

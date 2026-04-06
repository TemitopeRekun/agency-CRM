using Crm.Domain.Entities;

namespace Crm.Application.DTOs.Contracts;

public class CreateContractRequest
{
    public string Title { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Terms { get; set; } = string.Empty;
    public string Kpis { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public Guid ClientId { get; set; }
    
    // Billing Terms
    public decimal BaseRetainer { get; set; }
    public SuccessFeeType SuccessFeeType { get; set; }
    public decimal SuccessFeeValue { get; set; }
}

public class ContractResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Terms { get; set; } = string.Empty;
    public ContractStatus Status { get; set; }
    public Guid ProjectId { get; set; }
    public Guid ClientId { get; set; }
    public int Version { get; set; }
    public string SignatureStatus { get; set; } = string.Empty;
    public bool IsWaitingSignature { get; set; }
    public DateTimeOffset? SignedAt { get; set; }
    public string? SignerIp { get; set; }
    
    // Billing Terms
    public decimal BaseRetainer { get; set; }
    public SuccessFeeType SuccessFeeType { get; set; }
    public decimal SuccessFeeValue { get; set; }
    public DateTimeOffset? LastInvoicedAt { get; set; }
    public bool HasBeenViewed { get; set; }
    public DateTimeOffset? ViewedAt { get; set; }

    public Guid Token { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SignContractRequest
{
    public string DigitalSignature { get; set; } = string.Empty;
}

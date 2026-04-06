namespace Crm.Domain.Entities;

public enum InvoiceStatus
{
    Draft,
    Sent,
    PartiallyPaid,
    Paid,
    Overdue,
    Cancelled
}

public class Invoice : BaseEntity, ITenantedEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount => TotalAmount - PaidAmount;
    public string Currency { get; set; } = "USD";
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    
    public Guid? ContractId { get; set; }
    public Contract? Contract { get; set; }
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
    public Guid TenantId { get; set; }
    
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class InvoiceItem : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount => Quantity * UnitPrice;
    
    public Guid InvoiceId { get; set; }
}

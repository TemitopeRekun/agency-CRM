using System.ComponentModel.DataAnnotations;

namespace Crm.Domain.Entities;

public enum PaymentMethod
{
    BankTransfer,
    CreditCard,
    PayPal,
    Stripe,
    Cash,
    Other
}

public class Payment : BaseEntity, ITenantedEntity
{
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public Guid TenantId { get; set; }
}

using Crm.Application.DTOs.Invoices;
using Crm.Domain.Entities;

namespace Crm.Application.Interfaces;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceResponse>> GetAllAsync();
    Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request);
    Task<InvoiceResponse> GenerateFromContractAsync(Guid contractId);
    Task<InvoiceResponse> GenerateFromProjectAsync(Guid projectId);
    Task<InvoiceResponse?> UpdateStatusAsync(Guid id, InvoiceStatus newStatus);
    Task<int> ProcessPendingBillingAsync();
    Task<InvoiceResponse?> UpdateAsync(Guid id, UpdateInvoiceRequest request);
    Task<InvoiceResponse?> RecordPaymentAsync(Guid invoiceId, RecordPaymentRequest request);
}

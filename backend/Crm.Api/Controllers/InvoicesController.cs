using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Crm.Application.Services;
using Crm.Application.DTOs.Invoices;
using Crm.Domain.Entities;

namespace Crm.Api.Controllers;

[Authorize(Roles = "Admin,Accountant")]
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly InvoiceService _invoiceService;

    public InvoicesController(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceResponse>>> GetInvoices()
    {
        var invoices = await _invoiceService.GetAllAsync();
        return Ok(invoices);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceResponse>> CreateInvoice(CreateInvoiceRequest request)
    {
        var response = await _invoiceService.CreateAsync(request);
        return CreatedAtAction(nameof(GetInvoices), new { id = response.Id }, response);
    }

    [HttpPost("generate/contract/{contractId}")]
    public async Task<ActionResult<InvoiceResponse>> GenerateFromContract(Guid contractId)
    {
        var response = await _invoiceService.GenerateFromContractAsync(contractId);
        return Ok(response);
    }

    [HttpPost("generate/project/{projectId}")]
    public async Task<ActionResult<InvoiceResponse>> GenerateFromProject(Guid projectId)
    {
        var response = await _invoiceService.GenerateFromProjectAsync(projectId);
        return Ok(response);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<InvoiceResponse>> UpdateStatus(Guid id, [FromBody] UpdateInvoiceStatusRequest request)
    {
        var response = await _invoiceService.UpdateStatusAsync(id, request.Status);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<InvoiceResponse>> UpdateInvoice(Guid id, [FromBody] UpdateInvoiceRequest request)
    {
        var response = await _invoiceService.UpdateAsync(id, request);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpPost("{id}/payments")]
    public async Task<ActionResult<InvoiceResponse>> RecordPayment(Guid id, [FromBody] RecordPaymentRequest request)
    {
        var response = await _invoiceService.RecordPaymentAsync(id, request);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpPost("trigger-automated-billing")]
    public async Task<ActionResult<int>> TriggerBilling()
    {
        var count = await _invoiceService.ProcessPendingBillingAsync();
        return Ok(count);
    }
}

public class UpdateInvoiceStatusRequest
{
    public InvoiceStatus Status { get; set; }
}


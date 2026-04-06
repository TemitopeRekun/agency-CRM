using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.Invoices;
using Crm.Application.DTOs.Projects;
using Crm.Application.DTOs.Clients;
using FluentAssertions;
using System.Net;
using Crm.Domain.Entities;

namespace Crm.IntegrationTests.Controllers;

public class InvoicesControllerTests : BaseIntegrationTest
{
    public InvoicesControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetInvoices_Unauthorized_Returns401()
    {
        var response = await _client.GetAsync("/api/invoices");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RecordPayment_ValidInvoice_UpdatesStatus()
    {
        // Arrange
        await AuthenticateAsync("Accountant");
        
        // 1. Create a client and project first
        var clientReq = new CreateClientRequest { Name = "Invoice Client" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", clientReq);
        var client = await clientResp.Content.ReadFromJsonAsync<ClientResponse>();

        var projReq = new CreateProjectRequest { Name = "Invoice Proj", ClientId = client!.Id };
        var projResp = await _client.PostAsJsonAsync("/api/projects", projReq);
        var proj = await projResp.Content.ReadFromJsonAsync<ProjectResponse>();

        // 2. Create an invoice
        var invReq = new CreateInvoiceRequest 
        { 
            ProjectId = proj!.Id, 
            TotalAmount = 500, 
            DueDate = DateTime.UtcNow.AddDays(30) 
        };
        var invResp = await _client.PostAsJsonAsync("/api/invoices", invReq);
        var invoice = await invResp.Content.ReadFromJsonAsync<InvoiceResponse>();

        // 3. Record payment
        var payReq = new RecordPaymentRequest 
        { 
            Amount = 500, 
            PaymentDate = DateTime.UtcNow, 
            Method = (PaymentMethod)1 
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/invoices/{invoice!.Id}/payments", payReq);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        result!.Status.Should().Be(InvoiceStatus.Paid);
        result.PaidAmount.Should().Be(500);
    }

    [Fact]
    public async Task GenerateFromProject_ValidId_CreatesInvoice()
    {
        // Arrange
        await AuthenticateAsync("Admin");
        
        var clientReq = new CreateClientRequest { Name = "Gen Client" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", clientReq);
        var client = await clientResp.Content.ReadFromJsonAsync<ClientResponse>();

        var projReq = new CreateProjectRequest { Name = "Gen Proj", ClientId = client!.Id };
        var projResp = await _client.PostAsJsonAsync("/api/projects", projReq);
        var proj = await projResp.Content.ReadFromJsonAsync<ProjectResponse>();

        // Act
        var response = await _client.PostAsync($"/api/invoices/generate/project/{proj!.Id}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        result.Should().NotBeNull();
        result!.ProjectId.Should().Be(proj.Id);
    }
}

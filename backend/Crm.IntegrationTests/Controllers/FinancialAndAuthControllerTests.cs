using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.Auth;
using Crm.Application.DTOs.Invoices;
using FluentAssertions;
using Crm.Domain.Entities;

namespace Crm.IntegrationTests;

public class AuthControllerTests : BaseIntegrationTest
{
    public AuthControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokensAndCookies()
    {
        // Arrange
        var request = new LoginRequest { Email = "admin@tenanta.com", Password = "Admin123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        
        // Verify Cookies
        response.Headers.GetValues("Set-Cookie").Should().Contain(c => c.Contains("access_token"));
        response.Headers.GetValues("Set-Cookie").Should().Contain(c => c.Contains("refresh_token"));
    }

    [Fact]
    public async Task Logout_ClearsCookies()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.PostAsync("/api/auth/logout", null);

        // Assert
        response.EnsureSuccessStatusCode();
        response.Headers.GetValues("Set-Cookie").Should().Contain(c => c.Contains("access_token=;"));
    }
}

public class InvoicesControllerTests : BaseIntegrationTest
{
    public InvoicesControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateInvoice_Authorized_SavesToDb()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateInvoiceRequest 
        { 
            InvoiceNumber = "INV-IT-001", 
            TotalAmount = 1500,
            Currency = "USD",
            DueDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/invoices", request);

        // Assert
        await EnsureSuccessAsync(response);
        var result = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        result.Should().NotBeNull();
        result!.InvoiceNumber.Should().Be(request.InvoiceNumber);
    }

    [Fact]
    public async Task RecordPayment_UpdatesInvoiceStatus()
    {
        // Arrange
        await AuthenticateAsync();
        // Create an invoice first
        var invResp = await _client.PostAsJsonAsync("/api/invoices", new CreateInvoiceRequest { InvoiceNumber = "INV-PAY", TotalAmount = 100 });
        var invoice = await invResp.Content.ReadFromJsonAsync<InvoiceResponse>();

        var paymentReq = new RecordPaymentRequest 
        { 
            Amount = 100, 
            PaymentDate = DateTime.UtcNow, 
            Method = PaymentMethod.CreditCard 
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/invoices/{invoice!.Id}/payments", paymentReq);

        // Assert
        await EnsureSuccessAsync(response);
        var result = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        result!.Status.Should().Be(InvoiceStatus.Paid);
        result.PaidAmount.Should().Be(100);
    }
}

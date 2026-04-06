using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.Leads;
using FluentAssertions;
using System.Net;
using Crm.Domain.Entities;

namespace Crm.IntegrationTests.Controllers;

public class LeadsControllerTests : BaseIntegrationTest
{
    public LeadsControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetLeads_Unauthorized_Returns401()
    {
        var response = await _client.GetAsync("/api/leads");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateLead_Admin_CreatesAndReturnsLead()
    {
        // Arrange
        await AuthenticateAsync("Admin");
        var request = new CreateLeadRequest 
        { 
            Title = "High Value Lead", 
            ContactName = "Jane Doe", 
            Email = "jane@example.com" 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/leads", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<LeadResponse>();
        result.Should().NotBeNull();
        result!.Title.Should().Be(request.Title);
    }

    [Fact]
    public async Task UpdateStatus_ValidId_UpdatesAndReturns200()
    {
        // Arrange
        await AuthenticateAsync("SalesManager");
        var createRequest = new CreateLeadRequest { Title = "Status Test", ContactName = "X", Email = "x@x.com" };
        var createResponse = await _client.PostAsJsonAsync("/api/leads", createRequest);
        var lead = await createResponse.Content.ReadFromJsonAsync<LeadResponse>();

        var updateRequest = new UpdateLeadStatusRequest { Status = LeadStatus.Contacted };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/leads/{lead!.Id}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LeadResponse>();
        result!.Status.Should().Be(LeadStatus.Contacted);
    }

    [Fact]
    public async Task UpdateLead_InvalidId_Returns404()
    {
        // Arrange
        await AuthenticateAsync("Admin");
        var request = new UpdateLeadRequest { Title = "Ghost Lead" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/leads/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

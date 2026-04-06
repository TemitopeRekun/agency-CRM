using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.Clients;
using Crm.Application.DTOs.Leads;
using FluentAssertions;

namespace Crm.IntegrationTests;

public class ClientsControllerTests : BaseIntegrationTest
{
    public ClientsControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetClients_Unauthorized_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/clients");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateClient_Authorized_CreatesAndReturnsClient()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateClientRequest { Name = "New Integration Client" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        await EnsureSuccessAsync(response);
        var result = await response.Content.ReadFromJsonAsync<ClientResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be(request.Name);
    }
}

public class LeadsControllerTests : BaseIntegrationTest
{
    public LeadsControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateLead_Authorized_CreatesAndReturnsLead()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateLeadRequest 
        { 
            Title = "Integration Lead", 
            Description = "Test Lead Desc" 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/leads", request);

        // Assert
        await EnsureSuccessAsync(response);
        var result = await response.Content.ReadFromJsonAsync<LeadResponse>();
        result.Should().NotBeNull();
        result!.Title.Should().Be(request.Title);
    }
}

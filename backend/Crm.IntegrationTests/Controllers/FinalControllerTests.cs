using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.Contracts;
using Crm.Application.DTOs.Projects;
using Crm.Application.DTOs.Clients;
using FluentAssertions;
using System.Net;
using Crm.Domain.Entities;

namespace Crm.IntegrationTests.Controllers;

public class ContractPortalControllerTests : BaseIntegrationTest
{
    public ContractPortalControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetContractByToken_ValidToken_ReturnsContract()
    {
        // 1. Arrange: Create contract via regular controller
        await AuthenticateAsync("Admin");
        var clientReq = new CreateClientRequest { Name = "Portal Client" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", clientReq);
        var client = await clientResp.Content.ReadFromJsonAsync<ClientResponse>();

        var projReq = new CreateProjectRequest { Name = "Portal Proj", ClientId = client!.Id };
        var projResp = await _client.PostAsJsonAsync("/api/projects", projReq);
        var proj = await projResp.Content.ReadFromJsonAsync<ProjectResponse>();

        var genResp = await _client.PostAsync($"/api/contracts/generate/{proj!.Id}", null);
        var contract = await genResp.Content.ReadFromJsonAsync<ContractResponse>();

        // 2. Act: Use portal endpoint with token
        var portalToken = contract!.Token; // Assuming this is returned
        var response = await _client.GetAsync($"/api/portal/contracts/{portalToken}");

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class ProjectAdAccountsControllerTests : BaseIntegrationTest
{
    public ProjectAdAccountsControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_Authorized_ReturnsList()
    {
        // Arrange
        await AuthenticateAsync("Admin");

        // Act
        var response = await _client.GetAsync("/api/projectadaccounts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class HealthControllerTests : BaseIntegrationTest
{
    public HealthControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Ping_ReturnsPong()
    {
        var response = await _client.GetAsync("/api/ping");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("pong");
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/api/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.Clients;
using FluentAssertions;
using System.Net;

namespace Crm.IntegrationTests.Controllers;

public class ClientsControllerTests : BaseIntegrationTest
{
    public ClientsControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetClients_Unauthorized_Returns401()
    {
        var response = await _client.GetAsync("/api/clients");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateClient_ValidRequest_CreatesAndReturnsClient()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateClientRequest { Name = "Int Test Client" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ClientResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task CreateClient_InvalidRequest_Returns400()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateClientRequest { Name = "" }; // Required Name is empty

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

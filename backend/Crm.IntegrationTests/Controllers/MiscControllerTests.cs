using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.AdMetrics;
using Crm.Application.DTOs.Projects;
using Crm.Application.DTOs.Clients;
using FluentAssertions;
using System.Net;

namespace Crm.IntegrationTests.Controllers;

public class AdMetricsControllerTests : BaseIntegrationTest
{
    public AdMetricsControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetProjectAnalytics_ValidId_ReturnsAnalytics()
    {
        // Arrange
        await AuthenticateAsync("Admin");
        var clientReq = new CreateClientRequest { Name = "Ad Client" };
        var clientResp = await _client.PostAsJsonAsync("/api/clients", clientReq);
        var client = await clientResp.Content.ReadFromJsonAsync<ClientResponse>();

        var projReq = new CreateProjectRequest { Name = "Ad Proj", ClientId = client!.Id };
        var projResp = await _client.PostAsJsonAsync("/api/projects", projReq);
        var proj = await projResp.Content.ReadFromJsonAsync<ProjectResponse>();

        // Act
        var response = await _client.GetAsync($"/api/admetrics/project/{proj!.Id}/analytics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AdMetricAnalyticsResponse>();
        result.Should().NotBeNull();
    }
}

public class AutomationControllerTests : BaseIntegrationTest
{
    public AutomationControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task TriggerAutomation_Admin_ReturnsOk()
    {
        // Arrange
        await AuthenticateAsync("Admin");

        // Act
        var response = await _client.PostAsync("/api/automation/trigger", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

public class WebhooksControllerTests : BaseIntegrationTest
{
    public WebhooksControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task StripeWebhook_ValidEvent_ReturnsOk()
    {
        // Arrange
        var request = new { type = "payment_intent.succeeded", data = new { @object = new { amount = 5000 } } };

        // Act
        var response = await _client.PostAsJsonAsync("/api/webhooks/stripe", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

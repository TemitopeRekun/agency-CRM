using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.AdMetrics;
using Crm.Application.DTOs.Auth;
using Crm.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Crm.Application.Interfaces;
using Crm.Infrastructure.Data;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Crm.Application.DTOs.Projects;

namespace Crm.IntegrationTests;

public class AdSyncIntegrationTests : IClassFixture<CrmWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IAdPlatformClient> _googleMock = new();
    private readonly Mock<IAdPlatformClient> _metaMock = new();

    public AdSyncIntegrationTests(CrmWebApplicationFactory factory)
    {
        _googleMock.Setup(x => x.Platform).Returns(AdPlatform.Google);
        _metaMock.Setup(x => x.Platform).Returns(AdPlatform.Meta);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real clients with mocks
                services.RemoveAll<IAdPlatformClient>();
                services.AddScoped(_ => _googleMock.Object);
                services.AddScoped(_ => _metaMock.Object);
            });
        });
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await DbInitializer.SeedAsync(_factory.Services);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task AuthenticateAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = "admin@tenanta.com", Password = "Admin123!" });
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.AccessToken);
    }

    [Fact]
    public async Task Sync_Metrics_Persists_External_Data_To_Db()
    {
        await AuthenticateAsync();

        // 1. Setup: Create a Project and an Ad Account
        var projectResp = await _client.PostAsJsonAsync("/api/projects", new CreateProjectRequest { Name = "Ad Project" });
        var project = await projectResp.Content.ReadFromJsonAsync<ProjectResponse>();

        // We'll create the AdAccount directly via DB for simplicity in test setup if no endpoint exists,
        // or use an endpoint if available. Let's assume we use a private setup.
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.ProjectAdAccounts.Add(new ProjectAdAccount
            {
                Id = Guid.NewGuid(),
                ProjectId = project!.Id,
                Platform = AdPlatform.Google,
                ExternalAccountId = "acc_123",
                IsActive = true,
                TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001")
            });
            await context.SaveChangesAsync();
        }

        // 2. Mock external platform response
        var yesterday = DateTime.UtcNow.AddDays(-1).Date;
        _googleMock.Setup(x => x.FetchDailyMetricsAsync("acc_123", It.IsAny<DateTime>()))
            .ReturnsAsync(new List<AdMetric> 
            { 
                new AdMetric { Spend = 150, Impressions = 1000, Clicks = 50, Conversions = 5, Date = yesterday } 
            });

        // 3. Trigger Sync via Automation Endpoint
        var syncResp = await _client.PostAsync($"/api/automation/sync-ad-metrics?projectId={project!.Id}", null);
        Assert.True(syncResp.IsSuccessStatusCode);

        // 4. Verify Metrics exist in DB
        var metricsResp = await _client.GetAsync($"/api/admetrics/project/{project.Id}");
        var metrics = await metricsResp.Content.ReadFromJsonAsync<IEnumerable<AdMetricResponse>>();
        
        Assert.Contains(metrics!, m => m.Spend == 150 && m.Date.Date == yesterday);
    }
}

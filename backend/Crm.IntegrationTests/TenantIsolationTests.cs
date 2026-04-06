using Xunit;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.Auth;
using Crm.Application.DTOs.Clients;
using Crm.Application.DTOs.Leads;
using Crm.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Crm.Infrastructure.Data;
using System.Net.Http.Headers;

namespace Crm.IntegrationTests;

public class TenantIsolationTests : IClassFixture<CrmWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CrmWebApplicationFactory _factory;

    public TenantIsolationTests(CrmWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // For isolation tests, we don't reset the DB between facts to ensure we can have data for both tenants
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await DbInitializer.SeedAsync(_factory.Services);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task AuthenticateAsTenantA()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = "admin@tenanta.com", Password = "Admin123!" });
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.AccessToken);
    }

    private async Task AuthenticateAsTenantB()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = "admin@tenantb.com", Password = "Admin123!" });
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.AccessToken);
    }

    [Fact]
    public async Task TenantA_Cannot_Access_TenantB_Client_By_Id()
    {
        // 1. Log in as Tenant B to find their client ID
        await AuthenticateAsTenantB();
        var clientsResp = await _client.GetFromJsonAsync<IEnumerable<ClientResponse>>("/api/clients");
        var tenantBClient = clientsResp!.First();

        // 2. Log in as Tenant A
        await AuthenticateAsTenantA();

        // 3. Try to access Tenant B's client
        var forbiddenResp = await _client.GetAsync($"/api/clients/{tenantBClient.Id}");
        
        // Assert: Should be 404 (NotFound) or 403 (Forbidden) depending on security policy. 
        // Our controller uses FirstOrDefaultAsync with TenantId filter, so it should return 404.
        Assert.Equal(HttpStatusCode.NotFound, forbiddenResp.StatusCode);
    }

    [Fact]
    public async Task TenantA_Cannot_Update_TenantB_Lead()
    {
         // 1. Get Tenant B's lead
        await AuthenticateAsTenantB();
        var leadsResp = await _client.GetFromJsonAsync<IEnumerable<LeadResponse>>("/api/leads");
        var tenantBLead = leadsResp!.First();

        // 2. Authenticate as Tenant A
        await AuthenticateAsTenantA();

        // 3. Try to update
        var updateReq = new { Title = "Hacked Title" };
        var forbiddenResp = await _client.PutAsJsonAsync($"/api/leads/{tenantBLead.Id}", updateReq);

        Assert.Equal(HttpStatusCode.NotFound, forbiddenResp.StatusCode);
    }

    [Fact]
    public async Task List_Only_Returns_Current_Tenant_Data()
    {
        await AuthenticateAsTenantA();
        var clientsA = await _client.GetFromJsonAsync<IEnumerable<ClientResponse>>("/api/clients");
        Assert.All(clientsA!, c => Assert.Contains("Solutions", c.Name)); // Tenant A data from Seed

        await AuthenticateAsTenantB();
        var clientsB = await _client.GetFromJsonAsync<IEnumerable<ClientResponse>>("/api/clients");
        Assert.All(clientsB!, b => Assert.Contains("Studio", b.Name)); // Tenant B data from Seed
    }
}

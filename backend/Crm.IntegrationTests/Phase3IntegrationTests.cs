using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.Auth;
using Crm.Application.DTOs.Clients;
using Crm.Application.DTOs.Leads;
using Crm.Application.DTOs.Offers;
using Crm.Application.DTOs.Projects;
using Crm.Application.DTOs.Contracts;
using Crm.Application.DTOs.Invoices;
using Crm.Application.DTOs.AdMetrics;
using Crm.Domain.Entities;
using Respawn;
using Respawn.Graph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Crm.Infrastructure.Data;
using System.Net.Http.Headers;

namespace Crm.IntegrationTests;

public class Phase3IntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private Respawner? _respawner;

    public Phase3IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new Table[] { "Tenants", "Users", "__EFMigrationsHistory" }
        });

        await _respawner.ResetAsync(connection);

        // Ensure database is seeded for each test
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
    public async Task Portal_Signature_Flow_Activates_Project()
    {
        await AuthenticateAsync();

        // 1. Setup: Create Lead -> Offer -> Accept (Auto-creates Project & Contract)
        var clientResp = await _client.PostAsJsonAsync("/api/clients", new { Name = "Portal Test Client" });
        var client = await clientResp.Content.ReadFromJsonAsync<ClientResponse>();

        var leadResp = await _client.PostAsJsonAsync("/api/leads", new { Title = "Portal Lead", ClientId = client!.Id });
        var lead = await leadResp.Content.ReadFromJsonAsync<LeadResponse>();

        var offerResp = await _client.PostAsJsonAsync("/api/offers", new { Title = "Portal Offer", TotalAmount = 1000, LeadId = lead!.Id });
        var offer = await offerResp.Content.ReadFromJsonAsync<OfferResponse>();

        // Approve offer triggers automation (creates project & contract)
        await _client.PatchAsJsonAsync($"/api/offers/{offer!.Id}/status", new { Status = OfferStatus.Accepted });

        // 2. Find the auto-created Contract
        var contractsResp = await _client.GetAsync($"/api/contracts/offer/{offer.Id}");
        var contract = await contractsResp.Content.ReadFromJsonAsync<ContractResponse>();
        Assert.NotNull(contract);
        Assert.True(contract!.IsWaitingSignature);

        // 3. Portal Sign Action (using Token)
        var signResp = await _client.PostAsJsonAsync($"/api/portal/contracts/{contract!.Token}/sign", new { DigitalSignature = "Ayoola Ogunrekun" });
        Assert.True(signResp.IsSuccessStatusCode);

        // 4. Verify Project is now Active and Audit Data is present
        var projectResp = await _client.GetAsync($"/api/projects/{contract.ProjectId}");
        var project = await projectResp.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.Equal(ProjectStatus.Active, project!.Status);

        var contractAfterSign = await _client.GetFromJsonAsync<ContractResponse>($"/api/contracts/{contract.Id}");
        Assert.NotNull(contractAfterSign!.SignerIp);
        Assert.Equal("::1", contractAfterSign.SignerIp); // Since it's a local test call
    }

    [Fact]
    public async Task Automated_Monthly_Billing_Calculates_Success_Fees()
    {
        await AuthenticateAsync();

        // 1. Setup: Create Project and Contract with Revenue Share (10%)
        var clientResp = await _client.PostAsJsonAsync("/api/clients", new { Name = "Billing Test Client" });
        var client = await clientResp.Content.ReadFromJsonAsync<ClientResponse>();

        var projectReq = new { Name = "Billing Project", ClientId = client!.Id };
        var projectResp = await _client.PostAsJsonAsync("/api/projects", projectReq);
        var project = await projectResp.Content.ReadFromJsonAsync<ProjectResponse>();

        var contractReq = new CreateContractRequest 
        { 
            Title = "Performance Contract", 
            ProjectId = project!.Id,
            ClientId = client.Id,
            BaseRetainer = 500,
            SuccessFeeType = SuccessFeeType.RevenueShare,
            SuccessFeeValue = 10 // 10%
        };
        var contractResp = await _client.PostAsJsonAsync("/api/contracts", contractReq);
        var contract = await contractResp.Content.ReadFromJsonAsync<ContractResponse>();

        // Mark contract as Signed so it's picked up by billing job
        await _client.PostAsJsonAsync($"/api/portal/contracts/{contract!.Id}/sign", new { DigitalSignature = "Test Sign" });

        // 2. Inject Ad Spend for the Previous Month
        var now = DateTime.UtcNow;
        var lastMonth = now.AddMonths(-1);
        var adMetricReq = new CreateAdMetricRequest 
        { 
            ProjectId = project.Id, 
            Spend = 5000, 
            Platform = AdPlatform.Google, 
            Date = new DateTime(lastMonth.Year, lastMonth.Month, 15) 
        };
        await _client.PostAsJsonAsync("/api/admetrics", adMetricReq);

        // 3. Trigger Monthly Billing Job (We'll use a hidden/internal endpoint if we had one, or call service directly if testing logic)
        // For integration test, we can use an internal admin endpoint we added for testing automations
        var billingResp = await _client.PostAsync("/api/automation/run-monthly-billing", null);
        Assert.True(billingResp.IsSuccessStatusCode);

        // 4. Verify Invoice
        var invoicesResp = await _client.GetAsync("/api/invoices");
        var invoices = await invoicesResp.Content.ReadFromJsonAsync<IEnumerable<InvoiceResponse>>();
        var invoice = invoices!.FirstOrDefault(i => i.ProjectId == project.Id);

        Assert.NotNull(invoice);
        // Expecting: $500 (Base) + $500 (10% of $5000) = $1000
        Assert.Equal(1000, invoice!.TotalAmount);
        Assert.Contains(invoice.Items, i => i.Description.Contains("Performance Bonus"));
    }
}

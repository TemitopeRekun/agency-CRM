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
using Crm.Application.DTOs.TimeEntries;
using Crm.Application.DTOs.AdMetrics;
using Crm.Domain.Entities;
using Respawn;
using Respawn.Graph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Crm.Infrastructure.Data;
using System.Net.Http.Headers;

namespace Crm.IntegrationTests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private Respawner? _respawner;
    private static bool _dbInitialized = false;
    private static readonly object _dbLock = new();

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Ensure database is created and migrated once per process
        lock (_dbLock)
        {
            if (!_dbInitialized)
            {
                context.Database.Migrate();
                _dbInitialized = true;
            }
        }

        // Always Seed CRM Data before each test (DbInitializer handles idempotency)
        await DbInitializer.SeedAsync(_factory.Services);

        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        _respawner ??= await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            // Ignore Tenants and Users (seeded) AND Hangfire internal tables
            TablesToIgnore = new Table[] 
            { 
                "Tenants", 
                "Users",
                "__EFMigrationsHistory",
                new Table("public", "Job"),
                new Table("public", "JobParameter"),
                new Table("public", "JobQueue"),
                new Table("public", "List"),
                new Table("public", "Schema"),
                new Table("public", "Server"),
                new Table("public", "Set"),
                new Table("public", "State"),
                new Table("public", "Counter"),
                new Table("public", "Hash"),
                new Table("public", "AggregatedCounter")
            }
        });

        if (connection != null)
        {
            await _respawner.ResetAsync(connection);
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task AuthenticateAsync(string email = "admin@tenanta.com", string password = "Admin123!")
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = password });
        await CheckSuccessAsync(response);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.AccessToken);
    }

    private async Task CheckSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TEST FAILURE] Status: {response.StatusCode}, Body: {content}");
        }
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Full_Workflow_Lead_To_Invoice_Works()
    {
        await AuthenticateAsync();

        // 1. Create Client
        var clientResp = await _client.PostAsJsonAsync("/api/clients", new { Name = "Test Client" });
        await CheckSuccessAsync(clientResp);
        var client = await clientResp.Content.ReadFromJsonAsync<ClientResponse>();

        // 2. Create Lead
        var leadReq = new { Title = "Test Lead", Description = "Lead Desc", ClientId = client!.Id };
        var leadResp = await _client.PostAsJsonAsync("/api/leads", leadReq);
        await CheckSuccessAsync(leadResp);
        var lead = await leadResp.Content.ReadFromJsonAsync<LeadResponse>();

        // 3. Create Offer -> Approve -> Contract
        var offerReq = new { Title = "Test Offer", TotalAmount = 5000, LeadId = lead!.Id };
        var offerResp = await _client.PostAsJsonAsync("/api/offers", offerReq);
        await CheckSuccessAsync(offerResp);
        var offer = await offerResp.Content.ReadFromJsonAsync<OfferResponse>();

        var approveResp = await _client.PatchAsJsonAsync($"/api/offers/{offer!.Id}/status", new UpdateOfferStatusRequest { Status = OfferStatus.Accepted });
        await CheckSuccessAsync(approveResp);

        var contractResp = await _client.GetAsync($"/api/contracts/offer/{offer.Id}");
        await CheckSuccessAsync(contractResp);
        var contract = await contractResp.Content.ReadFromJsonAsync<ContractResponse>();

        // 4. Create Invoice from Contract
        var invoiceReq = new { InvoiceNumber = "INV-2024-001", TotalAmount = 5000, ContractId = contract!.Id, ProjectId = contract.ProjectId, Items = new List<object>() };
        var invoiceResp = await _client.PostAsJsonAsync("/api/invoices", invoiceReq);
        await CheckSuccessAsync(invoiceResp);

        var invoice = await invoiceResp.Content.ReadFromJsonAsync<InvoiceResponse>();
        Assert.Equal("INV-2024-001", invoice!.InvoiceNumber);
        Assert.Equal(5000, invoice.TotalAmount);

        // 5. Verify Engagement Tracking (ViewedAt)
        var viewResp = await _client.PostAsync($"/api/portal/contracts/{contract.Token}/view", null);
        await CheckSuccessAsync(viewResp);

        var contractAfterView = await _client.GetFromJsonAsync<ContractResponse>($"/api/contracts/offer/{offer.Id}");
        Assert.True(contractAfterView!.HasBeenViewed);
        Assert.NotNull(contractAfterView.ViewedAt);
    }

    [Fact]
    public async Task Accepting_Multiple_Offers_From_Same_Lead_Reuses_Client()
    {
        await AuthenticateAsync();

        var clientResp = await _client.PostAsJsonAsync("/api/clients", new { Name = "Shared Client" });
        await CheckSuccessAsync(clientResp);
        var client = await clientResp.Content.ReadFromJsonAsync<ClientResponse>();

        var leadReq = new { Title = "Multi Offer Lead", ClientId = client!.Id };
        var leadResp = await _client.PostAsJsonAsync("/api/leads", leadReq);
        await CheckSuccessAsync(leadResp);
        var lead = await leadResp.Content.ReadFromJsonAsync<LeadResponse>();

        // Offer 1
        var offer1Req = new { Title = "Offer 1", TotalAmount = 1000, LeadId = lead!.Id };
        var offer1Resp = await _client.PostAsJsonAsync("/api/offers", offer1Req);
        await CheckSuccessAsync(offer1Resp);
        var offer1 = await offer1Resp.Content.ReadFromJsonAsync<OfferResponse>();
        var patch1Resp = await _client.PatchAsJsonAsync($"/api/offers/{offer1!.Id}/status", new UpdateOfferStatusRequest { Status = OfferStatus.Accepted });
        await CheckSuccessAsync(patch1Resp);

        // Offer 2
        var offer2Req = new { Title = "Offer 2", TotalAmount = 2000, LeadId = lead.Id };
        var offer2Resp = await _client.PostAsJsonAsync("/api/offers", offer2Req);
        await CheckSuccessAsync(offer2Resp);
        var offer2 = await offer2Resp.Content.ReadFromJsonAsync<OfferResponse>();
        var patch2Resp = await _client.PatchAsJsonAsync($"/api/offers/{offer2!.Id}/status", new UpdateOfferStatusRequest { Status = OfferStatus.Accepted });
        await CheckSuccessAsync(patch2Resp);

        var clientsResp = await _client.GetAsync("/api/clients");
        var clients = await clientsResp.Content.ReadFromJsonAsync<IEnumerable<ClientResponse>>();
        
        Assert.Single(clients!.Where(c => c.Name == "Shared Client"));
    }

    [Fact]
    public async Task Project_Invoice_Can_Be_Generated_And_Deep_Updated()
    {
        await AuthenticateAsync();

        var clientResp = await _client.PostAsJsonAsync("/api/clients", new { Name = "Invoice Test Client" });
        await CheckSuccessAsync(clientResp);
        var client = await clientResp.Content.ReadFromJsonAsync<ClientResponse>();

        var leadReq = new { Title = "Invoice Lead", ClientId = client!.Id };
        var leadResp = await _client.PostAsJsonAsync("/api/leads", leadReq);
        var lead = await leadResp.Content.ReadFromJsonAsync<LeadResponse>();

        var offerReq = new { Title = "Invoice Offer", TotalAmount = 1000, LeadId = lead!.Id };
        var offerResp = await _client.PostAsJsonAsync("/api/offers", offerReq);
        var offer = await offerResp.Content.ReadFromJsonAsync<OfferResponse>();

        var projectReq = new { Name = "Invoice Project", ClientId = client!.Id, OfferId = offer!.Id };
        var projectResp = await _client.PostAsJsonAsync("/api/projects", projectReq);
        await CheckSuccessAsync(projectResp);
        var project = await projectResp.Content.ReadFromJsonAsync<ProjectResponse>();

        // Generate $0 invoice
        var genResp = await _client.PostAsync($"/api/invoices/generate/project/{project!.Id}", null);
        await CheckSuccessAsync(genResp);
        var generatedInvoice = await genResp.Content.ReadFromJsonAsync<InvoiceResponse>();

        Assert.Equal(0, generatedInvoice!.TotalAmount);

        // Update Invoice with new line items
        var updateReq = new UpdateInvoiceRequest 
        { 
            TotalAmount = 500, 
            DueDate = DateTime.UtcNow.AddDays(7),
            Status = InvoiceStatus.Paid,
            Items = new List<CreateInvoiceItemRequest> 
            {
                new CreateInvoiceItemRequest { Description = "Consulting", Quantity = 5, UnitPrice = 100 }
            }
        };

        var updateResp = await _client.PutAsJsonAsync($"/api/invoices/{generatedInvoice.Id}", updateReq);
        await CheckSuccessAsync(updateResp);
        var updatedInvoice = await updateResp.Content.ReadFromJsonAsync<InvoiceResponse>();

        Assert.Equal(500, updatedInvoice!.TotalAmount);
        Assert.Single(updatedInvoice.Items);
    }

    [Fact]
    public async Task Global_Metrics_Endpoints_Return_Aggregated_Data()
    {
        await AuthenticateAsync();

        var clientResp = await _client.PostAsJsonAsync("/api/clients", new { Name = "Metrics Client" });
        await CheckSuccessAsync(clientResp);
        var client = await clientResp.Content.ReadFromJsonAsync<ClientResponse>();

        var leadReq = new { Title = "Metrics Lead", ClientId = client!.Id };
        var leadResp = await _client.PostAsJsonAsync("/api/leads", leadReq);
        var lead = await leadResp.Content.ReadFromJsonAsync<LeadResponse>();

        var offerReq = new { Title = "Metrics Offer", TotalAmount = 500, LeadId = lead!.Id };
        var offerResp = await _client.PostAsJsonAsync("/api/offers", offerReq);
        var offer = await offerResp.Content.ReadFromJsonAsync<OfferResponse>();

        var projectReq = new { Name = "Metrics Project", ClientId = client!.Id, OfferId = offer!.Id };
        var projectResp = await _client.PostAsJsonAsync("/api/projects", projectReq);
        await CheckSuccessAsync(projectResp);
        var project = await projectResp.Content.ReadFromJsonAsync<ProjectResponse>();

        // Seed some data
        var timeReq = new CreateTimeEntryRequest { ProjectId = project!.Id, Hours = 5, Description = "Dev", Date = DateTime.UtcNow };
        var timeResp = await _client.PostAsJsonAsync("/api/timeentries", timeReq);
        await CheckSuccessAsync(timeResp);

        var adReq = new CreateAdMetricRequest { ProjectId = project.Id, Spend = 100, Impressions = 1000, Clicks = 50, Conversions = 5, Platform = AdPlatform.Google, Date = DateTime.UtcNow };
        var adMetricsResp = await _client.PostAsJsonAsync("/api/admetrics", adReq);
        await CheckSuccessAsync(adMetricsResp);

        // Verify across global endpoints
        var timeStatsResp = await _client.GetFromJsonAsync<IEnumerable<TimeEntryResponse>>("/api/timeentries");
        Assert.Contains(timeStatsResp!, t => t.ProjectId == project.Id);

        var adStatsResp = await _client.GetFromJsonAsync<IEnumerable<AdMetricResponse>>("/api/admetrics");
        Assert.Contains(adStatsResp!, a => a.ProjectId == project.Id);
    }
}

using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.Auth;
using Crm.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Hangfire;
using Hangfire.MemoryStorage;
using System.Net.Http.Headers;

namespace Crm.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<CrmWebApplicationFactory>, IAsyncLifetime
{
    protected readonly HttpClient _client;
    protected readonly CrmWebApplicationFactory _factory;

    protected BaseIntegrationTest(CrmWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Reset DB for each test to ensure isolation
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await DbInitializer.SeedAsync(scope.ServiceProvider);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task AuthenticateAsync(string email = "admin@tenanta.com", string password = "Admin123!")
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = password });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.AccessToken);
    }

    protected async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"[TEST FAILURE] Status: {response.StatusCode}, Body: {content}");
        }
    }
}

public class CrmWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DB registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Create and open a shared connection to keep the in-memory database alive
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Use Memory Storage for Hangfire in tests
            var hangfireDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(JobStorage));
            if (hangfireDescriptor != null) services.Remove(hangfireDescriptor);
            services.AddHangfire(config => config.UseMemoryStorage());
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Dispose();
        }
        base.Dispose(disposing);
    }
}

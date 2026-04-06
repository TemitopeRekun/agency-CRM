using Xunit;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Crm.Infrastructure.Data;
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;

namespace Crm.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<CrmWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CrmWebApplicationFactory _factory;

    public AuthIntegrationTests(CrmWebApplicationFactory factory)
    {
        _factory = factory;
        // Create client with cookie support
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await DbInitializer.SeedAsync(_factory.Services);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_Sets_Secure_Cookies_And_Returns_DTO()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest 
        { 
            Email = "admin@tenanta.com", 
            Password = "Admin123!" 
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result!.AccessToken);
        Assert.Equal("admin@tenanta.com", result.Email);

        // Verify Cookies
        Assert.True(response.Headers.Contains("Set-Cookie"));
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        Assert.Contains(cookies, c => c.Contains("refresh_token"));
        Assert.Contains(cookies, c => c.Contains("access_token"));
        Assert.Contains(cookies, c => c.Contains("HttpOnly"));
    }

    [Fact]
    public async Task Refresh_Token_Rotation_Works()
    {
        // 1. Initial Login
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest 
        { 
            Email = "admin@tenanta.com", 
            Password = "Admin123!" 
        });
        var originalCookies = loginResp.Headers.GetValues("Set-Cookie").ToList();
        var originalRefreshToken = originalCookies.First(c => c.Contains("refresh_token"));

        // 2. Perform Refresh
        // The client automatically sends the cookies from the login response
        var refreshResp = await _client.PostAsync("/api/auth/refresh", null);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, refreshResp.StatusCode);
        var refreshResult = await refreshResp.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(refreshResult!.AccessToken);

        // 3. Verify Refresh Token was rotated (New Set-Cookie header)
        Assert.True(refreshResp.Headers.Contains("Set-Cookie"));
        var newCookies = refreshResp.Headers.GetValues("Set-Cookie").ToList();
        var newRefreshToken = newCookies.First(c => c.Contains("refresh_token"));
        
        Assert.NotEqual(originalRefreshToken, newRefreshToken);
    }

    [Fact]
    public async Task Logout_Clears_Cookies_And_Revokes_Token()
    {
        // 1. Login
        await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest 
        { 
            Email = "admin@tenanta.com", 
            Password = "Admin123!" 
        });

        // 2. Logout
        var logoutResp = await _client.PostAsync("/api/auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, logoutResp.StatusCode);
        
        // Verify Cookies are expired
        var cookies = logoutResp.Headers.GetValues("Set-Cookie").ToList();
        Assert.Contains(cookies, c => c.Contains("access_token=;")); // Expired cookie
        Assert.Contains(cookies, c => c.Contains("refresh_token=;"));

        // 3. Try to Refresh after Logout
        var forbiddenRefresh = await _client.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, forbiddenRefresh.StatusCode);
    }

    [Fact]
    public async Task Invalid_Login_Returns_401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest 
        { 
            Email = "admin@tenanta.com", 
            Password = "WrongPassword" 
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

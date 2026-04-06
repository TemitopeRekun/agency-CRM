using Xunit;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Crm.Application.DTOs.Auth;
using FluentAssertions;
using System.Net;

namespace Crm.IntegrationTests.Controllers;

public class AuthControllerTests : BaseIntegrationTest
{
    public AuthControllerTests(CrmWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_ValidCredentials_Returns200AndSetsCookies()
    {
        // Arrange
        var request = new LoginRequest 
        { 
            Email = "admin@example.com", 
            Password = "AdminPassword123!" 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();

        // Check for cookies
        response.Headers.Should().ContainKey("Set-Cookie");
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.Contains("access_token"));
        cookies.Should().Contain(c => c.Contains("refresh_token"));
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        // Arrange
        var request = new LoginRequest 
        { 
            Email = "wrong@example.com", 
            Password = "wrong" 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ClearsCookies()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.Contains("access_token=;"));
    }

    [Fact]
    public async Task Refresh_NoCookie_Returns400()
    {
        // Act
        var response = await _client.PostAsync("/api/auth/refresh", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

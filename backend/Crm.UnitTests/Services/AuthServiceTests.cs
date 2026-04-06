using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.Auth;
using Xunit;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Crm.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Fixture _fixture;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _configurationMock = new Mock<IConfiguration>();
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Setup IConfiguration mock for JWT
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(s => s["Key"]).Returns("super_secret_long_key_for_agency_crm_mvp_development_123!");
        jwtSectionMock.Setup(s => s["Issuer"]).Returns("agency_crm");
        jwtSectionMock.Setup(s => s["Audience"]).Returns("agency_crm");

        _configurationMock.Setup(c => c.GetSection("Jwt")).Returns(jwtSectionMock.Object);

        _service = new AuthService(_userRepositoryMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var password = "Password123!";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Admin,
            TenantId = Guid.NewGuid(),
            RefreshTokens = new List<RefreshToken>()
        };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var request = new LoginRequest { Email = user.Email, Password = password };

        // Act
        var result = await _service.LoginAsync(request, "127.0.0.1");

        // Assert
        result.Response.Should().NotBeNull();
        result.Response!.Email.Should().Be(user.Email);
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var user = new User { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectOne") };
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var request = new LoginRequest { Email = user.Email, Password = "WrongPassword" };

        // Act
        var result = await _service.LoginAsync(request, "127.0.0.1");

        // Assert
        result.Response.Should().BeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_RotatesToken()
    {
        // Arrange
        var oldToken = "old-token";
        var user = _fixture.Create<User>();
        var refreshToken = new RefreshToken { Token = oldToken, Expires = DateTime.UtcNow.AddDays(1), Created = DateTime.UtcNow };
        user.RefreshTokens = new List<RefreshToken> { refreshToken };

        _userRepositoryMock.Setup(r => r.GetByRefreshTokenAsync(oldToken)).ReturnsAsync(user);

        // Act
        var result = await _service.RefreshTokenAsync(oldToken, "127.0.0.1");

        // Assert
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(oldToken);
        refreshToken.Revoked.Should().NotBeNull();
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }
}

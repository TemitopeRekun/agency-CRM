using Microsoft.Extensions.DependencyInjection;
using Crm.Infrastructure.Data;
using Crm.Domain.Entities;
using Crm.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Crm.IntegrationTests.Infrastructure;

public class DbContextTests : IClassFixture<CrmWebApplicationFactory>
{
    private readonly CrmWebApplicationFactory _factory;

    public DbContextTests(CrmWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldInjectTenantIdAndTimestamps()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userContextMock = new Mock<ICurrentUserContext>();
        userContextMock.Setup(c => c.TenantId).Returns(tenantId);
        userContextMock.Setup(c => c.IsAuthenticated).Returns(true);

        using var scope = _factory.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
        
        // Use a clean context for this test
        using var context = new AppDbContext(options, userContextMock.Object);
        
        var client = new Client 
        { 
            Id = Guid.NewGuid(),
            Name = "Timestamp Test Client" 
        };

        // Act
        context.Clients.Add(client);
        await context.SaveChangesAsync();

        // Assert
        client.TenantId.Should().Be(tenantId);
        client.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        // Update
        client.Name = "Updated Name";
        await context.SaveChangesAsync();
        client.UpdatedAt.Should().NotBeNull();
        client.UpdatedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GlobalQueryFilter_ShouldFilterByTenantId()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        
        var userContextMock = new Mock<ICurrentUserContext>();
        userContextMock.Setup(c => c.TenantId).Returns(tenantA);
        userContextMock.Setup(c => c.IsAuthenticated).Returns(true);

        using var scope = _factory.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
        
        using (var setupContext = new AppDbContext(options, new Mock<ICurrentUserContext>().Object))
        {
            // Seed directly without filters for setup
            setupContext.Clients.Add(new Client { Id = Guid.NewGuid(), Name = "Client A", TenantId = tenantA });
            setupContext.Clients.Add(new Client { Id = Guid.NewGuid(), Name = "Client B", TenantId = tenantB });
            await setupContext.SaveChangesAsync();
        }

        // Act
        using var testContext = new AppDbContext(options, userContextMock.Object);
        var filteredClients = await testContext.Clients.ToListAsync();

        // Assert
        filteredClients.Should().HaveCount(1);
        filteredClients.First().Name.Should().Be("Client A");
    }
}

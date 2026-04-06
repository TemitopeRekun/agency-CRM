using Microsoft.AspNetCore.Http;
using Moq;
using Crm.Api.Middleware;
using Crm.Application.Interfaces;
using FluentAssertions;

namespace Crm.UnitTests.Middleware;

public class LogEnrichmentMiddlewareTests
{
    private readonly Mock<ICurrentUserContext> _userContextMock;
    private readonly DefaultHttpContext _context;

    public LogEnrichmentMiddlewareTests()
    {
        _userContextMock = new Mock<ICurrentUserContext>();
        _context = new DefaultHttpContext();
        _context.TraceIdentifier = "test-request-id";
    }

    [Fact]
    public async Task InvokeAsync_ShouldRetrieveContextAndCallNext()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _userContextMock.Setup(c => c.TenantId).Returns(tenantId);
        _userContextMock.Setup(c => c.UserId).Returns(userId);

        bool nextCalled = false;
        var middleware = new LogEnrichmentMiddleware((innerContext) => 
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(_context, _userContextMock.Object);

        // Assert
        nextCalled.Should().BeTrue();
        _userContextMock.Verify(c => c.TenantId, Times.Once);
        _userContextMock.Verify(c => c.UserId, Times.Once);
    }
}

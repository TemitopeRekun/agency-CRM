using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Crm.Api.Middleware;
using System.Text.Json;
using System.Net;
using FluentAssertions;

namespace Crm.UnitTests.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionMiddleware>> _loggerMock;
    private readonly DefaultHttpContext _context;

    public GlobalExceptionMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        _context = new DefaultHttpContext();
        _context.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionOccurs_ShouldLogAndReturnProblemDetails()
    {
        // Arrange
        var middleware = new GlobalExceptionMiddleware(
            next: (innerContext) => throw new Exception("Test Exception"),
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        _context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        _context.Response.ContentType.Should().Be("application/json");

        _context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(_context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var response = JsonDocument.Parse(responseBody).RootElement;

        response.GetProperty("status").GetInt32().Should().Be(500);
        response.GetProperty("message").GetString().Should().Be("Test Exception");
        response.GetProperty("type").GetString().Should().Be("Exception");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}

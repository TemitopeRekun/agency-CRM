using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.TimeEntries;
using Xunit;
using AutoFixture;
using FluentAssertions;

namespace Crm.UnitTests.Services;

public class TimeEntryServiceTests
{
    private readonly Mock<IGenericRepository<TimeEntry>> _repositoryMock;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly Fixture _fixture;
    private readonly TimeEntryService _service;

    public TimeEntryServiceTests()
    {
        _repositoryMock = new Mock<IGenericRepository<TimeEntry>>();
        _currentUserContextMock = new Mock<ICurrentUserContext>();
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _service = new TimeEntryService(_repositoryMock.Object, _currentUserContextMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenUserLoggedIn_ShouldCreateTimeEntry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var request = _fixture.Create<CreateTimeEntryRequest>();

        _currentUserContextMock.Setup(c => c.UserId).Returns(userId);
        _currentUserContextMock.Setup(c => c.TenantId).Returns(tenantId);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Hours.Should().Be(request.Hours);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<TimeEntry>(e => e.UserId == userId && e.TenantId == tenantId)), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenUserNotLoggedIn_ShouldThrowUnauthorized()
    {
        // Arrange
        _currentUserContextMock.Setup(c => c.UserId).Returns((Guid?)null);
        var request = _fixture.Create<CreateTimeEntryRequest>();

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntries()
    {
        // Arrange
        var entries = _fixture.CreateMany<TimeEntry>(3).ToList();
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(entries);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }
}

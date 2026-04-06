using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.Clients;
using Xunit;
using AutoFixture;
using FluentAssertions;

namespace Crm.UnitTests.Services;

public class ClientServiceTests
{
    private readonly Mock<IGenericRepository<Client>> _repositoryMock;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly Fixture _fixture;
    private readonly ClientService _service;

    public ClientServiceTests()
    {
        _repositoryMock = new Mock<IGenericRepository<Client>>();
        _currentUserContextMock = new Mock<ICurrentUserContext>();
        _fixture = new Fixture();
        
        // Customize AutoFixture to avoid circular references if any
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _service = new ClientService(
            _repositoryMock.Object,
            _currentUserContextMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllClients()
    {
        // Arrange
        var clients = _fixture.CreateMany<Client>(3).ToList();
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(clients);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.First().Name.Should().Be(clients.First().Name);
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateClientWithTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var request = _fixture.Create<CreateClientRequest>();
        _currentUserContextMock.Setup(c => c.TenantId).Returns(tenantId);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Client>(c => c.TenantId == tenantId && c.Name == request.Name)), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenTenantIdIsNull_ShouldUseEmptyGuid()
    {
        // Arrange
        var request = _fixture.Create<CreateClientRequest>();
        _currentUserContextMock.Setup(c => c.TenantId).Returns((Guid?)null);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Client>(c => c.TenantId == Guid.Empty)), Times.Once);
    }
}

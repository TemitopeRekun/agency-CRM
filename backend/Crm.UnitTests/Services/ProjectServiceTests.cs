using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.Projects;
using Xunit;
using AutoFixture;
using FluentAssertions;

namespace Crm.UnitTests.Services;

public class ProjectServiceTests
{
    private readonly Mock<IGenericRepository<Project>> _repositoryMock;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly Fixture _fixture;
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        _repositoryMock = new Mock<IGenericRepository<Project>>();
        _currentUserContextMock = new Mock<ICurrentUserContext>();
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _service = new ProjectService(_repositoryMock.Object, _currentUserContextMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProjectAndSave()
    {
        // Arrange
        var request = _fixture.Create<CreateProjectRequest>();
        var tenantId = Guid.NewGuid();
        _currentUserContextMock.Setup(c => c.TenantId).Returns(tenantId);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Project>(p => p.TenantId == tenantId)), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProjects()
    {
        // Arrange
        var projects = _fixture.CreateMany<Project>(3).ToList();
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(projects);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
}

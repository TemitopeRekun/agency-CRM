using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.Tasks;
using Xunit;
using AutoFixture;
using FluentAssertions;

namespace Crm.UnitTests.Services;

public class TaskServiceTests
{
    private readonly Mock<IGenericRepository<CrmTask>> _repositoryMock;
    private readonly Fixture _fixture;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _repositoryMock = new Mock<IGenericRepository<CrmTask>>();
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _service = new TaskService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTaskAndSave()
    {
        // Arrange
        var request = _fixture.Create<CreateTaskRequest>();

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<CrmTask>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnTasks()
    {
        // Arrange
        var tasks = _fixture.CreateMany<CrmTask>(2).ToList();
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tasks);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Title.Should().Be(tasks.First().Title);
    }
}

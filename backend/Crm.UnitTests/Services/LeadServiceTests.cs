using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.Leads;
using Xunit;
using AutoFixture;
using FluentAssertions;

namespace Crm.UnitTests.Services;

public class LeadServiceTests
{
    private readonly Mock<IGenericRepository<Lead>> _repositoryMock;
    private readonly Fixture _fixture;
    private readonly LeadService _service;

    public LeadServiceTests()
    {
        _repositoryMock = new Mock<IGenericRepository<Lead>>();
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _service = new LeadService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateLeadAndSave()
    {
        // Arrange
        var request = _fixture.Create<CreateLeadRequest>();

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Lead>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnLeads()
    {
        // Arrange
        var leads = _fixture.CreateMany<Lead>(2).ToList();
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(leads);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(l => l.Title == leads[0].Title);
        result.Should().Contain(l => l.Title == leads[1].Title);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lead = _fixture.Build<Lead>().With(l => l.Id, id).With(l => l.Status, LeadStatus.New).Create();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(lead);

        var request = new UpdateLeadStatusRequest { Status = LeadStatus.Qualified };

        // Act
        var result = await _service.UpdateStatusAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(LeadStatus.Qualified);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Lead>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenLeadNotFound_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Lead?)null);

        // Act
        var result = await _service.UpdateStatusAsync(id, _fixture.Create<UpdateLeadStatusRequest>());

        // Assert
        result.Should().BeNull();
    }
}

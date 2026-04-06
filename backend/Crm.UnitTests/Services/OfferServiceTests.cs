using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.Offers;
using Xunit;
using AutoFixture;
using FluentAssertions;
using MockQueryable.Moq;

namespace Crm.UnitTests.Services;

public class OfferServiceTests
{
    private readonly Mock<IGenericRepository<Offer>> _repositoryMock;
    private readonly Mock<IAutomationService> _automationServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Fixture _fixture;
    private readonly OfferService _service;

    public OfferServiceTests()
    {
        _repositoryMock = new Mock<IGenericRepository<Offer>>();
        _automationServiceMock = new Mock<IAutomationService>();
        _emailServiceMock = new Mock<IEmailService>();
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _service = new OfferService(
            _repositoryMock.Object,
            _automationServiceMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateOfferAndSave()
    {
        // Arrange
        var request = _fixture.Create<CreateOfferRequest>();

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Offer>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ToAccepted_TriggersAutomation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var offer = _fixture.Build<Offer>()
            .With(o => o.Id, id)
            .With(o => o.Status, OfferStatus.Draft)
            .Create();
            
        var mock = new List<Offer> { offer }.AsQueryable().BuildMock();
        _repositoryMock.Setup(r => r.AsQueryable()).Returns(mock);

        var request = new UpdateOfferStatusRequest { Status = OfferStatus.Accepted };

        // Act
        var result = await _service.UpdateStatusAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(OfferStatus.Accepted);
        _automationServiceMock.Verify(a => a.ProcessAcceptedOfferAsync(id), Times.Once);
    }

    [Fact]
    public async Task MarkAsViewedAsync_SetsFlagAndTimestamp()
    {
        // Arrange
        var id = Guid.NewGuid();
        var offer = _fixture.Build<Offer>().With(o => o.Id, id).With(o => o.HasBeenViewed, false).Create();
        var mock = new List<Offer> { offer }.AsQueryable().BuildMock();
        _repositoryMock.Setup(r => r.AsQueryable()).Returns(mock);

        // Act
        var result = await _service.MarkAsViewedAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.HasBeenViewed.Should().BeTrue();
        result.QuoteOpenedAt.Should().NotBeNull();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Offer>()), Times.Once);
    }
}

using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Application.DTOs.Offers;
using Xunit;

namespace Crm.UnitTests.Services;

public class OfferServiceTests
{
    private readonly Mock<IGenericRepository<Offer>> _repositoryMock;
    private readonly Mock<IAutomationService> _automationServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly OfferService _service;

    public OfferServiceTests()
    {
        _repositoryMock = new Mock<IGenericRepository<Offer>>();
        _automationServiceMock = new Mock<IAutomationService>();
        _emailServiceMock = new Mock<IEmailService>();
        
        _service = new OfferService(
            _repositoryMock.Object,
            _automationServiceMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateOfferAndSave()
    {
        // Arrange
        var request = new CreateOfferRequest
        {
            Title = "Test Offer",
            TotalAmount = 1000,
            LeadId = Guid.NewGuid()
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.TotalAmount, result.TotalAmount);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Offer>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var offer = new Offer { Id = id, Title = "Test", Status = OfferStatus.Draft };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(offer);

        var request = new UpdateOfferStatusRequest { Status = OfferStatus.Accepted };

        // Act
        var result = await _service.UpdateStatusAsync(id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OfferStatus.Accepted, result.Status);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Offer>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ToAccepted_TriggersAutomationService()
    {
        // Arrange
        var id = Guid.NewGuid();
        var offer = new Offer { Id = id, Title = "Big Offer", Status = OfferStatus.Draft };
        
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(offer);

        var request = new UpdateOfferStatusRequest { Status = OfferStatus.Accepted };

        // Act
        var result = await _service.UpdateStatusAsync(id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OfferStatus.Accepted, result.Status);
        
        // Verify AutomationService call
        _automationServiceMock.Verify(a => a.ProcessAcceptedOfferAsync(id), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_NonExistentOffer_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Offer?)null);

        // Act
        var result = await _service.UpdateStatusAsync(id, new UpdateOfferStatusRequest { Status = OfferStatus.Accepted });

        // Assert
        Assert.Null(result);
    }
}

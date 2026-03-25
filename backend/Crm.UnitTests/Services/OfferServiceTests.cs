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
    private readonly Mock<IGenericRepository<Lead>> _leadRepositoryMock;
    private readonly Mock<IGenericRepository<Client>> _clientRepositoryMock;
    private readonly Mock<IGenericRepository<Project>> _projectRepositoryMock;
    private readonly Mock<IGenericRepository<CrmTask>> _taskRepositoryMock;
    private readonly OfferService _service;

    public OfferServiceTests()
    {
        _repositoryMock = new Mock<IGenericRepository<Offer>>();
        _leadRepositoryMock = new Mock<IGenericRepository<Lead>>();
        _clientRepositoryMock = new Mock<IGenericRepository<Client>>();
        _projectRepositoryMock = new Mock<IGenericRepository<Project>>();
        _taskRepositoryMock = new Mock<IGenericRepository<CrmTask>>();
        
        _service = new OfferService(
            _repositoryMock.Object,
            _leadRepositoryMock.Object,
            _clientRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _taskRepositoryMock.Object);
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
    public async Task UpdateStatusAsync_ToAccepted_CreatesProjectClientAndTasks()
    {
        // Arrange
        var leadId = Guid.NewGuid();
        var lead = new Lead { Id = leadId, Title = "Acme Corp Lead", Description = "Lead Desc" };
        var id = Guid.NewGuid();
        var offer = new Offer { Id = id, Title = "Big Offer", Status = OfferStatus.Draft, LeadId = leadId };
        
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(offer);
        _leadRepositoryMock.Setup(r => r.GetByIdAsync(leadId)).ReturnsAsync(lead);

        var request = new UpdateOfferStatusRequest { Status = OfferStatus.Accepted };

        // Act
        var result = await _service.UpdateStatusAsync(id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OfferStatus.Accepted, result.Status);
        
        // Verify Client creation
        _clientRepositoryMock.Verify(c => c.AddAsync(It.Is<Client>(cl => cl.Name == lead.Title)), Times.Once);
        
        // Verify Project creation
        _projectRepositoryMock.Verify(p => p.AddAsync(It.Is<Project>(pr => 
            pr.Name == offer.Title && 
            pr.OfferId == offer.Id && 
            pr.Description == lead.Description)), Times.Once);
            
        // Verify Task creation (we added 3 default tasks)
        _taskRepositoryMock.Verify(t => t.AddAsync(It.IsAny<CrmTask>()), Times.Exactly(3));
        
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

    [Fact]
    public async Task UpdateStatusAsync_ToAccepted_ReusesExistingClientIfLeadConverted()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var lead = new Lead { Id = leadId, Title = "Existing Client Lead", ConvertedClientId = clientId };
        var id = Guid.NewGuid();
        var offer = new Offer { Id = id, Title = "Follow-up Offer", Status = OfferStatus.Draft, LeadId = leadId };
        
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(offer);
        _leadRepositoryMock.Setup(r => r.GetByIdAsync(leadId)).ReturnsAsync(lead);

        var request = new UpdateOfferStatusRequest { Status = OfferStatus.Accepted };

        // Act
        await _service.UpdateStatusAsync(id, request);

        // Assert
        // Verify Client was NOT created again
        _clientRepositoryMock.Verify(c => c.AddAsync(It.IsAny<Client>()), Times.Never);
        
        // Verify Project was created with EXISTING ClientId
        _projectRepositoryMock.Verify(p => p.AddAsync(It.Is<Project>(pr => pr.ClientId == clientId)), Times.Once);
    }
}

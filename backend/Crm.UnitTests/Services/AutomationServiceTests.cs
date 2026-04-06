using Moq;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Xunit;
using AutoFixture;
using FluentAssertions;
using MockQueryable.Moq;
using Microsoft.Extensions.Logging;

namespace Crm.UnitTests.Services;

public class AutomationServiceTests
{
    private readonly Mock<IGenericRepository<Offer>> _offerRepositoryMock;
    private readonly Mock<IGenericRepository<Project>> _projectRepositoryMock;
    private readonly Mock<IGenericRepository<Contract>> _contractRepositoryMock;
    private readonly Mock<IGenericRepository<CrmTask>> _taskRepositoryMock;
    private readonly Mock<IGenericRepository<TaskTemplate>> _templateRepositoryMock;
    private readonly Mock<IGenericRepository<Invoice>> _invoiceRepositoryMock;
    private readonly Mock<IGenericRepository<Lead>> _leadRepositoryMock;
    private readonly Mock<IGenericRepository<Client>> _clientRepositoryMock;
    private readonly Mock<ILogger<AutomationService>> _loggerMock;
    private readonly Mock<ICurrentUserContext> _userContextMock;
    private readonly Mock<ISlackService> _slackServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IInvoiceService> _invoiceServiceMock;
    private readonly Fixture _fixture;
    private readonly AutomationService _service;

    public AutomationServiceTests()
    {
        _offerRepositoryMock = new Mock<IGenericRepository<Offer>>();
        _projectRepositoryMock = new Mock<IGenericRepository<Project>>();
        _contractRepositoryMock = new Mock<IGenericRepository<Contract>>();
        _taskRepositoryMock = new Mock<IGenericRepository<CrmTask>>();
        _templateRepositoryMock = new Mock<IGenericRepository<TaskTemplate>>();
        _invoiceRepositoryMock = new Mock<IGenericRepository<Invoice>>();
        _leadRepositoryMock = new Mock<IGenericRepository<Lead>>();
        _clientRepositoryMock = new Mock<IGenericRepository<Client>>();
        _loggerMock = new Mock<ILogger<AutomationService>>();
        _userContextMock = new Mock<ICurrentUserContext>();
        _slackServiceMock = new Mock<ISlackService>();
        _emailServiceMock = new Mock<IEmailService>();
        _invoiceServiceMock = new Mock<IInvoiceService>();

        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _service = new AutomationService(
            _offerRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _contractRepositoryMock.Object,
            _taskRepositoryMock.Object,
            _templateRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _leadRepositoryMock.Object,
            _clientRepositoryMock.Object,
            _loggerMock.Object,
            _userContextMock.Object,
            _slackServiceMock.Object,
            _emailServiceMock.Object,
            _invoiceServiceMock.Object);
    }

    [Fact]
    public async Task ProcessAcceptedOfferAsync_OfferFound_CreatesProjectAndContract()
    {
        // Arrange
        var offerId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var offer = new Offer 
        { 
            Id = offerId, 
            Status = OfferStatus.Accepted, 
            Title = "Test Offer", 
            LeadId = leadId,
            Items = new List<OfferItem>()
        };
        var lead = new Lead { Id = leadId, Title = "Test Lead" };

        var offerQueryable = new List<Offer> { offer }.AsQueryable().BuildMock();
        _offerRepositoryMock.Setup(r => r.AsQueryable()).Returns(offerQueryable);
        _leadRepositoryMock.Setup(r => r.GetByIdAsync(leadId)).ReturnsAsync(lead);
        _templateRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskTemplate>());

        // Act
        await _service.ProcessAcceptedOfferAsync(offerId);

        // Assert
        _projectRepositoryMock.Verify(r => r.AddAsync(It.Is<Project>(p => p.Name == offer.Title)), Times.Once);
        _contractRepositoryMock.Verify(r => r.AddAsync(It.Is<Contract>(c => c.ProjectId != Guid.Empty)), Times.Once);
        _slackServiceMock.Verify(s => s.SendNotificationAsync(It.IsAny<string>()), Times.Once);
        _projectRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task NotifyOverdueInvoicesAsync_FindsOverdue_SendsEmail()
    {
        // Arrange
        var invoice = new Invoice 
        { 
            Status = InvoiceStatus.Sent, 
            DueDate = DateTime.UtcNow.AddDays(-1), 
            InvoiceNumber = "INV-001",
            TotalAmount = 100,
            Currency = "USD"
        };
        var queryableInvoices = new List<Invoice> { invoice }.AsQueryable().BuildMock();
        _invoiceRepositoryMock.Setup(r => r.AsQueryable()).Returns(queryableInvoices);

        // Act
        await _service.NotifyOverdueInvoicesAsync();

        // Assert
        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GenerateMonthlyInvoicesAsync_CallsInvoiceService()
    {
        // Act
        await _service.GenerateMonthlyInvoicesAsync();

        // Assert
        _invoiceServiceMock.Verify(s => s.ProcessPendingBillingAsync(), Times.Once);
    }
}
